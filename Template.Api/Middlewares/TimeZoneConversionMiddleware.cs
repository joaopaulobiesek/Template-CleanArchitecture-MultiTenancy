using Template.Application.Common.Interfaces.Services;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Template.Api.Middlewares;

/// <summary>
/// Middleware que converte automaticamente todos os DateTimes da response para o timezone do tenant
/// </summary>
public class TimeZoneConversionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimeZoneConversionMiddleware> _logger;
    private static readonly Regex DateTimeRegex = new Regex(
        @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[\+\-]\d{2}:\d{2})?",
        RegexOptions.Compiled);

    public TimeZoneConversionMiddleware(RequestDelegate next, ILogger<TimeZoneConversionMiddleware> _logger)
    {
        _next = next;
        this._logger = _logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantCacheService tenantCacheService)
    {
        // Verifica se tem TenantId no contexto
        if (!context.Items.TryGetValue("TenantId", out var tenantIdObj) || tenantIdObj is not Guid tenantId || tenantId == Guid.Empty)
        {
            // Sem tenant, apenas continua
            await _next(context);
            return;
        }

        // Captura o body original
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Só processa se for sucesso e JSON
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 &&
                context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Busca timezone do tenant
                var timeZoneId = await tenantCacheService.GetTimeZoneIdAsync(tenantId);

                // ⚠️ Se for UTC, não precisa converter (pula conversão)
                if (!string.IsNullOrWhiteSpace(timeZoneId) &&
                    !timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                    // Converte DateTimes
                    var convertedText = ConvertDateTimesToTimeZone(responseText, timeZoneId);

                    // Escreve resposta convertida
                    var convertedBytes = Encoding.UTF8.GetBytes(convertedText);
                    context.Response.ContentLength = convertedBytes.Length;
                    await originalBodyStream.WriteAsync(convertedBytes);
                    return;
                }
            }

            // Fallback: copia resposta original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter timezone para tenant {TenantId}", tenantId);

            // Em caso de erro, copia resposta original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private string ConvertDateTimesToTimeZone(string json, string timeZoneId)
    {
        try
        {
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return DateTimeRegex.Replace(json, match =>
            {
                var dateTimeStr = match.Value;

                if (DateTime.TryParse(dateTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    // Servidor está em UTC, então DateTime.Now salva em UTC
                    // Quando Kind = Unspecified, assume UTC e converte para timezone do tenant
                    if (dateTime.Kind == DateTimeKind.Unspecified)
                        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    else if (dateTime.Kind == DateTimeKind.Local)
                        dateTime = dateTime.ToUniversalTime();

                    // Converte de UTC para o timezone do tenant
                    var converted = TimeZoneInfo.ConvertTimeFromUtc(dateTime, targetTimeZone);
                    return converted.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                }

                return match.Value; // Se não conseguir parsear, retorna original
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter DateTimes para timezone {TimeZoneId}", timeZoneId);
            return json; // Retorna JSON original em caso de erro
        }
    }
}
