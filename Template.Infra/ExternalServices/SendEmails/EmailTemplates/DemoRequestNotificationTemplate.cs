namespace Template.Infra.ExternalServices.SendEmails.EmailTemplates;

/// <summary>
/// Template de email para notificação de solicitação de demonstração do site.
/// Enviado para o admin quando alguém solicita uma demonstração.
/// </summary>
public static class DemoRequestNotificationTemplate
{
    /// <summary>
    /// Gera o HTML do email de notificação de solicitação de demonstração.
    /// </summary>
    public static string Generate(
        string fullName,
        string email,
        string phone,
        string? companyName,
        string? eventType,
        string? estimatedAudience,
        string? message,
        DateTime requestedAt)
    {
        var companySection = string.IsNullOrWhiteSpace(companyName)
            ? string.Empty
            : $@"
                <tr>
                    <td style=""padding: 6px 16px;"">
                        <span style=""color: #6b7280; font-size: 13px;"">Empresa/Evento:</span>
                    </td>
                    <td style=""padding: 6px 16px; text-align: right;"">
                        <span style=""color: #111827; font-size: 14px;"">{System.Net.WebUtility.HtmlEncode(companyName)}</span>
                    </td>
                </tr>";

        var eventTypeSection = string.IsNullOrWhiteSpace(eventType)
            ? string.Empty
            : $@"
                <tr>
                    <td style=""padding: 6px 16px;"">
                        <span style=""color: #6b7280; font-size: 13px;"">Tipo de Evento:</span>
                    </td>
                    <td style=""padding: 6px 16px; text-align: right;"">
                        <span style=""color: #111827; font-size: 14px;"">{System.Net.WebUtility.HtmlEncode(eventType)}</span>
                    </td>
                </tr>";

        var audienceSection = string.IsNullOrWhiteSpace(estimatedAudience)
            ? string.Empty
            : $@"
                <tr>
                    <td style=""padding: 6px 16px;"">
                        <span style=""color: #6b7280; font-size: 13px;"">Estimativa de Público:</span>
                    </td>
                    <td style=""padding: 6px 16px; text-align: right;"">
                        <span style=""color: #111827; font-size: 14px;"">{System.Net.WebUtility.HtmlEncode(estimatedAudience)}</span>
                    </td>
                </tr>";

        var messageSection = string.IsNullOrWhiteSpace(message)
            ? string.Empty
            : $@"
                <tr>
                    <td style=""padding: 16px 0 0 0;"" colspan=""2"">
                        <p style=""margin: 0 0 8px 0; color: #6b7280; font-size: 13px;"">Mensagem:</p>
                        <p style=""margin: 0; color: #374151; font-size: 14px; font-style: italic; background-color: #f9fafb; padding: 12px; border-radius: 6px; border-left: 3px solid #3b82f6;"">
                            ""{System.Net.WebUtility.HtmlEncode(message)}""
                        </p>
                    </td>
                </tr>";

        return $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Nova Solicitação de Demonstração - Template</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f3f4f6;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""max-width: 560px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08); overflow: hidden;"">

                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #1e3a5f; padding: 24px 32px; text-align: center;"">
                            <span style=""color: #ffffff; font-size: 22px; font-weight: 700;"">Template</span>
                        </td>
                    </tr>

                    <!-- Status Banner -->
                    <tr>
                        <td style=""background-color: #dbeafe; padding: 16px 32px; text-align: center;"">
                            <span style=""color: #1e40af; font-size: 14px; font-weight: 600;"">Nova Solicitação de Demonstração</span>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 32px;"">
                            <table role=""presentation"" style=""width: 100%;"">

                                <!-- Title -->
                                <tr>
                                    <td style=""padding-bottom: 24px; text-align: center;"" colspan=""2"">
                                        <h1 style=""margin: 0 0 8px 0; color: #111827; font-size: 20px; font-weight: 600;"">
                                            Solicitação de Demonstração
                                        </h1>
                                        <p style=""margin: 0; color: #6b7280; font-size: 14px;"">
                                            Alguém solicitou uma demonstração do Template pelo site.
                                        </p>
                                    </td>
                                </tr>

                                <!-- Contact Details -->
                                <tr>
                                    <td style=""padding-bottom: 24px;"" colspan=""2"">
                                        <table role=""presentation"" style=""width: 100%; background-color: #f9fafb; border-radius: 8px; padding: 16px;"">
                                            <tr>
                                                <td style=""padding: 6px 16px;"">
                                                    <span style=""color: #6b7280; font-size: 13px;"">Nome:</span>
                                                </td>
                                                <td style=""padding: 6px 16px; text-align: right;"">
                                                    <span style=""color: #111827; font-size: 14px; font-weight: 500;"">{System.Net.WebUtility.HtmlEncode(fullName)}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 6px 16px;"">
                                                    <span style=""color: #6b7280; font-size: 13px;"">Email:</span>
                                                </td>
                                                <td style=""padding: 6px 16px; text-align: right;"">
                                                    <a href=""mailto:{System.Net.WebUtility.HtmlEncode(email)}"" style=""color: #3b82f6; font-size: 14px; text-decoration: none;"">{System.Net.WebUtility.HtmlEncode(email)}</a>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 6px 16px;"">
                                                    <span style=""color: #6b7280; font-size: 13px;"">Telefone:</span>
                                                </td>
                                                <td style=""padding: 6px 16px; text-align: right;"">
                                                    <a href=""tel:{System.Net.WebUtility.HtmlEncode(phone)}"" style=""color: #3b82f6; font-size: 14px; text-decoration: none;"">{System.Net.WebUtility.HtmlEncode(phone)}</a>
                                                </td>
                                            </tr>
                                            {companySection}
                                            {eventTypeSection}
                                            {audienceSection}
                                            <tr>
                                                <td style=""padding: 6px 16px;"">
                                                    <span style=""color: #6b7280; font-size: 13px;"">Data:</span>
                                                </td>
                                                <td style=""padding: 6px 16px; text-align: right;"">
                                                    <span style=""color: #111827; font-size: 14px;"">{requestedAt:dd/MM/yyyy HH:mm} <span style=""color: #9ca3af; font-size: 12px;"">(GMT 0)</span></span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                {messageSection}

                                <!-- Action Info -->
                                <tr>
                                    <td style=""padding-top: 24px;"" colspan=""2"">
                                        <table role=""presentation"" style=""width: 100%; background-color: #ecfdf5; border-radius: 8px; padding: 16px;"">
                                            <tr>
                                                <td style=""padding: 12px 16px;"">
                                                    <p style=""margin: 0 0 6px 0; color: #065f46; font-size: 13px; font-weight: 600;"">Próximo passo:</p>
                                                    <p style=""margin: 0; color: #047857; font-size: 13px; line-height: 1.5;"">
                                                        Entre em contato com o solicitante para agendar a demonstração. Atualize o status da solicitação no painel administrativo.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 20px 32px; border-top: 1px solid #e5e7eb; text-align: center;"">
                            <p style=""margin: 0; color: #9ca3af; font-size: 11px;"">
                                Este email foi enviado automaticamente pelo Template. Não responda este email.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    /// <summary>
    /// Gera o assunto do email
    /// </summary>
    public static string GetSubject()
    {
        return "[Template] Nova Solicitação de Demonstração";
    }
}
