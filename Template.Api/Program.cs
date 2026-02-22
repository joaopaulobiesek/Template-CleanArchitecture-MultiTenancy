using Hangfire;
using Template.Api.Middlewares;
using Template.Application.Common.Interfaces.Services;
using Template.Infra.BackgroundJobs;
using Template.Infra.Persistence.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AdicionarSwagger(builder.Configuration, typeof(Program).Assembly.GetName().Version.ToString());

builder.Services.AddSwaggerGen();

builder.Services.AdicionarInfra(builder.Configuration);

builder.Services.AddApplication();

var app = builder.Build();// CORS dinâmico - carrega origens do appsettings + banco de dados (Client.Url)
// Inicializa o cache de CORS na inicialização
var corsService = app.Services.GetRequiredService<ICorsOriginService>();
await corsService.RefreshCacheAsync();

app.UseCors(options => options
    .SetIsOriginAllowed(origin =>
    {
        // Valida origem dinamicamente usando o serviço de CORS
        // Nota: Usamos Task.Run para chamar método async de forma síncrona
        // pois SetIsOriginAllowed não suporta async
        return Task.Run(async () => await corsService.IsOriginAllowedAsync(origin)).Result;
    })
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());  // ← IMPORTANTE para cookies HTTP-Only funcionarem

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/Core.Api.v1/swagger.json", "Core API v1");
        options.SwaggerEndpoint("/swagger/Tenant.Api.v1/swagger.json", "Tenant API v1");
    });
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<TimeZoneConversionMiddleware>(); // Converte DateTimes para timezone do tenant
app.UseHttpsRedirection();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.InitializeDatabaseAsync();

app.Services.ConfigureRecurringJobs();

await app.RunAsync();