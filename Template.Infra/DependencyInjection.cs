using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Domain.Constants;
using Template.Infra.BackgroundJobs;
using Template.Infra.ExternalServices.Audit;
using Template.Infra.ExternalServices.AzureBlobStorage;
using Template.Infra.ExternalServices.Cors;
using Template.Infra.ExternalServices.Google;
using Template.Infra.ExternalServices.SendEmails;
using Template.Infra.ExternalServices.TenantCache;
using Template.Infra.Persistence;
using Template.Infra.Persistence.Contexts;
using Template.Infra.Persistence.Repositories;
using Template.Infra.Settings.Configurations;

namespace Template.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfra(this IServiceCollection services, IConfiguration config)
    {
        var jwtConfigration = new JwtConfiguration();
        config.GetSection(JwtConfiguration.Key).Bind(jwtConfigration);

        services.AddOptions<JwtConfiguration>()
            .BindConfiguration(JwtConfiguration.Key);

        services.AddScoped<IUser, User>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHttpContextAccessor();

        services.AddContext(config);

        services.AddHttpClient();

        services.AddMemoryCache();
        services.AddTenantCacheService();
        services.AddSingleton<ICorsOriginService, CorsOriginService>();
        services.AddRepository();
        services.AdicionarSendGrid(config);
        services.AddGoogleAPI(config);
        services.AdicionarStorage(config);
        services.AddHangfireJobs(config); // Hangfire Background Jobs

        services.AddTransient<CustomInitializerIdentity>();

        // TokenService precisa do UserManager do tenant correto + TenantCacheService para Issuer/Audience dinâmico
        services.AddScoped<ITokenService>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var tenantId = GetTenantId(httpContextAccessor);

            var userManager = (tenantId.HasValue && tenantId != Guid.Empty)
                ? GetUserManager(provider, tenantId.Value)
                : provider.GetRequiredService<UserManager<ContextUser>>();

            return new TokenService(
                provider.GetRequiredService<IOptions<JwtConfiguration>>(),
                userManager,
                provider.GetRequiredService<ITenantCacheService>(),
                provider.GetRequiredService<IConfiguration>());
        });

        services.AddScoped<IIdentityService>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var tenantId = GetTenantId(httpContextAccessor);

            var userManager = (tenantId.HasValue && tenantId != Guid.Empty)
                ? GetUserManager(provider, tenantId.Value)
                : provider.GetRequiredService<UserManager<ContextUser>>();

            return new IdentityService(userManager,
                provider.GetRequiredService<ITokenService>());
        });

        services.AddScoped(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var tenantId = GetTenantId(httpContextAccessor);

            var userManager = (tenantId.HasValue && tenantId != Guid.Empty)
                ? GetUserManager(provider, tenantId.Value)
                : provider.GetRequiredService<UserManager<ContextUser>>();

            var roleManager = (tenantId.HasValue && tenantId != Guid.Empty)
                ? GetRoleManager(provider, tenantId.Value)
                : provider.GetRequiredService<RoleManager<ContextRole>>();

            var configuration = provider.GetRequiredService<IConfiguration>();
            var tenantContext = provider.GetRequiredService<ITenantContext>();

            return new DatabaseInitializer(userManager, roleManager, configuration, tenantContext, httpContextAccessor);
        });

        AdicionarJwt(services, jwtConfigration);

        return services;
    }

    private static void AdicionarJwt(IServiceCollection services, JwtConfiguration jwtConfiguration)
    {
        services.AddAuthentication(authentication =>
        {
            authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtConfiguration.Issuer,
                // ValidAudience é setado dinamicamente no OnMessageReceived baseado no TenantID
                ClockSkew = TimeSpan.Zero
            };

            x.Events = new JwtBearerEvents
            {
                OnMessageReceived = async context =>
                {
                    // Lê o token do cookie se não estiver no header Authorization
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token = context.Request.Cookies["auth_token"];
                    }

                    // SignalR: lê token do query string para conexões WebSocket
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                    }

                    var tenantIdHeader = context.Request.Headers["X-Tenant-ID"].ToString();

                    // SignalR WebSocket: browser não envia headers customizados no upgrade,
                    // então lê o X-Tenant-ID do query string (fallback)
                    if (string.IsNullOrEmpty(tenantIdHeader))
                    {
                        var path = context.HttpContext.Request.Path;
                        if (path.StartsWithSegments("/hubs"))
                        {
                            tenantIdHeader = context.Request.Query["tenant_id"].ToString();

                            // Propaga para o header para que o Hub e middleware consigam ler
                            if (!string.IsNullOrEmpty(tenantIdHeader))
                                context.Request.Headers["X-Tenant-ID"] = tenantIdHeader;
                        }
                    }

                    // Define a chave de assinatura baseada no TenantID
                    byte[] key;
                    if (!string.IsNullOrEmpty(tenantIdHeader))
                        key = Encoding.ASCII.GetBytes($"{tenantIdHeader}_{jwtConfiguration.Secret}");
                    else
                        key = Encoding.ASCII.GetBytes($"{Guid.Empty}_{jwtConfiguration.Secret}");

                    context.Options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);

                    // Define Issuer/Audience dinâmico baseado no TenantID
                    // Em Development: Issuer vem do appsettings, Audience é dinâmico
                    // Em Production: Issuer e Audience são dinâmicos (vêm do cache/CorsSettings)
                    var isDevelopment = string.Equals(
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                        "Development",
                        StringComparison.OrdinalIgnoreCase);

                    if (!string.IsNullOrEmpty(tenantIdHeader) && Guid.TryParse(tenantIdHeader, out var tenantId) && tenantId != Guid.Empty)
                    {
                        // Tenant: busca URL do tenant no cache
                        var tenantCacheService = context.HttpContext.RequestServices.GetService<ITenantCacheService>();
                        if (tenantCacheService != null)
                        {
                            var tenantUrl = await tenantCacheService.GetTenantUrlByIdAsync(tenantId, context.HttpContext.RequestAborted);
                            if (!string.IsNullOrWhiteSpace(tenantUrl))
                            {
                                var normalizedUrl = NormalizeUrl(tenantUrl);
                                context.Options.TokenValidationParameters.ValidAudience = normalizedUrl;

                                // Em produção, Issuer também é dinâmico
                                if (!isDevelopment)
                                {
                                    context.Options.TokenValidationParameters.ValidIssuer = normalizedUrl;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Core: usa primeiro URL do CorsSettings:AllowedOrigins
                        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
                        if (configuration != null)
                        {
                            var allowedOrigins = configuration["CorsSettings:AllowedOrigins"];
                            if (!string.IsNullOrWhiteSpace(allowedOrigins))
                            {
                                var firstOrigin = allowedOrigins.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                                if (!string.IsNullOrWhiteSpace(firstOrigin))
                                {
                                    var normalizedUrl = NormalizeUrl(firstOrigin);
                                    context.Options.TokenValidationParameters.ValidAudience = normalizedUrl;

                                    // Em produção, Issuer também é dinâmico
                                    if (!isDevelopment)
                                    {
                                        context.Options.TokenValidationParameters.ValidIssuer = normalizedUrl;
                                    }
                                }
                            }
                        }
                    }
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanList, policy => policy.RequireClaim("Permission", Policies.CanList));
            options.AddPolicy(Policies.CanView, policy => policy.RequireClaim("Permission", Policies.CanView));
            options.AddPolicy(Policies.CanCreate, policy => policy.RequireClaim("Permission", Policies.CanCreate));
            options.AddPolicy(Policies.CanEdit, policy => policy.RequireClaim("Permission", Policies.CanEdit));
            options.AddPolicy(Policies.CanDelete, policy => policy.RequireClaim("Permission", Policies.CanDelete));
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireClaim("Permission", Policies.CanPurge));
            options.AddPolicy(Policies.CanArchive, policy => policy.RequireClaim("Permission", Policies.CanArchive));
            options.AddPolicy(Policies.CanRestore, policy => policy.RequireClaim("Permission", Policies.CanRestore));
            options.AddPolicy(Policies.CanApprove, policy => policy.RequireClaim("Permission", Policies.CanApprove));
            options.AddPolicy(Policies.CanReject, policy => policy.RequireClaim("Permission", Policies.CanReject));
            options.AddPolicy(Policies.CanExport, policy => policy.RequireClaim("Permission", Policies.CanExport));
            options.AddPolicy(Policies.CanImport, policy => policy.RequireClaim("Permission", Policies.CanImport));
            options.AddPolicy(Policies.CanManageSettings, policy => policy.RequireClaim("Permission", Policies.CanManageSettings));
            options.AddPolicy(Policies.CanManageUsers, policy => policy.RequireClaim("Permission", Policies.CanManageUsers));
            options.AddPolicy(Policies.CanAssignRoles, policy => policy.RequireClaim("Permission", Policies.CanAssignRoles));
            options.AddPolicy(Policies.CanAssignPolicies, policy => policy.RequireClaim("Permission", Policies.CanAssignPolicies));
            options.AddPolicy(Policies.CanViewReports, policy => policy.RequireClaim("Permission", Policies.CanViewReports));
            options.AddPolicy(Policies.CanGenerateReports, policy => policy.RequireClaim("Permission", Policies.CanGenerateReports));

            options.AddPolicy("AdminAccess", policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy("UserAccess", policy => policy.RequireRole(Roles.User));
            options.AddPolicy("TIAccess", policy => policy.RequireRole(Roles.TI));
        });
    }
    public static IServiceCollection AdicionarSwagger(this IServiceCollection services, IConfiguration configuration, string version)
    {
        services.AddSwaggerGen(delegate (SwaggerGenOptions c)
        {
            c.SwaggerDoc("Core.Api.v1", new OpenApiInfo
            {
                Title = "Template.API",
                Version = "v" + version,
                Description = string.Format("{0} - App {1} - Env {2}", "Core.API", configuration["Config:Ambiente"], Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            });

            c.SwaggerDoc("Tenant.Api.v1", new OpenApiInfo
            {
                Title = "MultiTenancy.API",
                Version = "v" + version,
                Description = string.Format("{0} - App {1} - Env {2}", "Tenant.API - MultiTenancy Support", configuration["Config:Ambiente"], Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            });

            c.CustomSchemaIds((Type x) => x.FullName);
            string applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            string applicationName = PlatformServices.Default.Application.ApplicationName;
            string text = Path.Combine(applicationBasePath, applicationName + ".xml");
            if (File.Exists(text))
            {
                c.IncludeXmlComments(text);
            }

            OpenApiSecurityScheme jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization Header.\r\n\r\nExample: (Inform without quotes): 'Bearer TokenJWT'",
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

            OpenApiSecurityScheme tenantSecurityScheme = new OpenApiSecurityScheme
            {
                Description = "Tenant Identifier Header.\r\n\r\nExample: 'X-Tenant-ID: 00000000-0000-0000-0000-000000000000'",
                Name = "X-Tenant-ID",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Reference = new OpenApiReference
                {
                    Id = "TenantID",
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(tenantSecurityScheme.Reference.Id, tenantSecurityScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            { jwtSecurityScheme, Array.Empty<string>() },
            { tenantSecurityScheme, Array.Empty<string>() }
            });
        });
        return services;
    }

    private static Guid? GetTenantId(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
    }

    private static UserManager<ContextUser> GetUserManager(IServiceProvider provider, Guid tenantId)
    {
        var customInitializer = provider.GetRequiredService<CustomInitializerIdentity>();
        return customInitializer.GetUserManagerForTenant(tenantId);
    }

    private static RoleManager<ContextRole> GetRoleManager(IServiceProvider provider, Guid tenantId)
    {
        var customInitializer = provider.GetRequiredService<CustomInitializerIdentity>();
        return customInitializer.GetRoleManagerForTenant(tenantId);
    }

    /// <summary>
    /// Normaliza URL para usar como Issuer/Audience do JWT.
    /// Adiciona https:// se não tiver protocolo.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        url = url.Trim();

        // Se já tem protocolo, retorna como está
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url.TrimEnd('/');
        }

        // Adiciona https:// por padrão
        return $"https://{url}".TrimEnd('/');
    }
}