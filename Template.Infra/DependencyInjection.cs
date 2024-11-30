using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Domain.Constants;
using Template.Infra.ExternalServices.Google;
using Template.Infra.ExternalServices.SendEmails;
using Template.Infra.ExternalServices.Storage;
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

        services.AddHttpContextAccessor();

        services.AddContext(config);

        services.AddHttpClient();

        services.AddMemoryCache();
        services.AddRepository();
        services.AdicionarSendGrid(config);
        services.AddGoogleAPI(config);
        services.AdicionarStorage(config);

        services.AddTransient<CustomInitializerIdentity>();

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

            return new DatabaseInitializer(userManager, roleManager, provider.GetRequiredService<IConfiguration>());
        });

        services.AddScoped<ITokenService, TokenService>();

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
                ValidAudience = jwtConfiguration.Audience,
                ClockSkew = TimeSpan.Zero
            };

            x.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var key = new byte[0];
                    var tenantId = context.Request.Headers["X-Tenant-ID"].ToString();
                    if (!string.IsNullOrEmpty(tenantId))
                        key = Encoding.ASCII.GetBytes($"{tenantId}_{jwtConfiguration.Secret}");
                    else
                        key = Encoding.ASCII.GetBytes($"{Guid.Empty.ToString()}_{jwtConfiguration.Secret}");

                    context.Options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
                    return Task.CompletedTask;
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
}