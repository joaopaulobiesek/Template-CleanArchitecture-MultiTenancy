using Template.Application.Common.Interfaces.Services;
using Template.Infra.Settings.Configurations;

namespace Template.Infra.ExternalServices.SendEmails;

internal static class DependencyInjection
{
    public static IServiceCollection AdicionarSendGrid(this IServiceCollection services, IConfiguration config)
    {
        var sendGridConfigration = new SendGridConfiguration();
        config.GetSection(SendGridConfiguration.Key).Bind(sendGridConfigration);

        services.AddOptions<SendGridConfiguration>()
            .BindConfiguration(SendGridConfiguration.Key);

        // Serviço de email do TENANT (usa configuração do tenant para sender)
        services.AddScoped<IEmailService, EmailService>();

        // Serviço de email do SISTEMA (usa configuração do appsettings - contato@wiesoo.com)
        services.AddScoped<IEmailSystemService, EmailSystemService>();

        return services;
    }
}