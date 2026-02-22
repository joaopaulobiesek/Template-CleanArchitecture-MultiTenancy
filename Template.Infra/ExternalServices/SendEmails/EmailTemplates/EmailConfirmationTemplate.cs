namespace Template.Infra.ExternalServices.SendEmails.EmailTemplates;

/// <summary>
/// Template de email para confirmação de email.
/// Enviado para o usuário após o registro.
/// </summary>
public static class EmailConfirmationTemplate
{
    /// <summary>
    /// Gera o HTML do email de confirmação de email.
    /// </summary>
    public static string Generate(
        string userName,
        string confirmationUrl,
        DateTime requestedAt)
    {
        return $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirme seu Email - Template</title>
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
                            <span style=""color: #1e40af; font-size: 14px; font-weight: 600;"">Confirmação de Email</span>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 32px;"">
                            <table role=""presentation"" style=""width: 100%;"">

                                <!-- Title -->
                                <tr>
                                    <td style=""padding-bottom: 24px; text-align: center;"">
                                        <h1 style=""margin: 0 0 8px 0; color: #111827; font-size: 20px; font-weight: 600;"">
                                            Olá, {System.Net.WebUtility.HtmlEncode(userName)}!
                                        </h1>
                                        <p style=""margin: 0; color: #6b7280; font-size: 14px;"">
                                            Bem-vindo(a) ao Template! Para concluir seu cadastro, confirme seu email clicando no botão abaixo.
                                        </p>
                                    </td>
                                </tr>

                                <!-- CTA Button -->
                                <tr>
                                    <td style=""padding-bottom: 24px; text-align: center;"">
                                        <a href=""{confirmationUrl}""
                                           style=""display: inline-block; background-color: #1e40af; color: #ffffff; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-size: 14px; font-weight: 600;"">
                                            Confirmar meu Email
                                        </a>
                                    </td>
                                </tr>

                                <!-- Alternative Link -->
                                <tr>
                                    <td style=""padding-bottom: 24px;"">
                                        <table role=""presentation"" style=""width: 100%; background-color: #f9fafb; border-radius: 8px; padding: 16px;"">
                                            <tr>
                                                <td style=""padding: 12px 16px;"">
                                                    <p style=""margin: 0 0 8px 0; color: #6b7280; font-size: 13px;"">
                                                        Se o botão não funcionar, copie e cole o link abaixo no seu navegador:
                                                    </p>
                                                    <p style=""margin: 0; color: #3b82f6; font-size: 12px; word-break: break-all;"">
                                                        {confirmationUrl}
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <!-- Security Notice -->
                                <tr>
                                    <td style=""padding-bottom: 24px;"">
                                        <table role=""presentation"" style=""width: 100%; background-color: #fef3c7; border-radius: 8px; padding: 16px;"">
                                            <tr>
                                                <td style=""padding: 12px 16px;"">
                                                    <p style=""margin: 0; color: #92400e; font-size: 13px;"">
                                                        <strong>Importante:</strong> Este link expira em 24 horas. Se você não solicitou este email, ignore-o.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <!-- Request Time -->
                                <tr>
                                    <td style=""text-align: center; padding-bottom: 8px;"">
                                        <p style=""margin: 0; color: #9ca3af; font-size: 12px;"">
                                            Solicitado em: {requestedAt:dd/MM/yyyy HH:mm} <span style=""font-size: 11px;"">(GMT 0)</span>
                                        </p>
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
        return "[Template] Confirme seu Email";
    }
}
