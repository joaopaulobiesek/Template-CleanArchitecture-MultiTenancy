using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Common.Security;

namespace Template.Application.Common.Behaviours;

/// <summary>
/// Behaviour de auditoria que ENVOLVE toda a execução.
/// Substitui o LoggingBehaviour - faz log no console E salva no banco.
/// Captura sucesso E erro, duração, e enfileira no Hangfire.
/// RequestBody é criptografado com AES-256 usando JTI do token como chave.
///
/// REGRAS DE AUDITORIA:
/// - Anonymous: NUNCA auditado (sem usuário identificado)
/// - Commands: SEMPRE auditados (ações que modificam dados)
/// - Queries: NUNCA auditadas, EXCETO se marcadas com [Auditable]
/// </summary>
public class AuditBehaviour<TRequest, TResponse> where TResponse : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUser _user;
    private readonly IAuditService _auditService;
    private readonly ITenantCacheService _tenantCacheService;

    public AuditBehaviour(
        ILogger<TRequest> logger,
        ICurrentUser user,
        IAuditService auditService,
        ITenantCacheService tenantCacheService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _user = user;
        _auditService = auditService;
        _tenantCacheService = tenantCacheService;
    }

    public async Task<ApiResponse<TResponse>> Handle(
        Func<Task<ApiResponse<TResponse>>> executeCore,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var category = ExtractCategory(requestName);
        var userId = _user.Id ?? "anonymous";
        var executedAt = DateTime.UtcNow;

        // Verifica se deve auditar este request e obtém configurações
        var (shouldAudit, auditableAttr) = ShouldAuditRequest(requestName, userId);

        // Log no console (sempre, independente de auditar ou não)
        _logger.LogInformation("[Audit] Request: {Name} | User: {UserId} | Tenant: {TenantId} | WillAudit: {WillAudit}",
            requestName, userId, _user.X_Tenant_ID, shouldAudit);

        _timer.Start();

        // Executa o pipeline - ExceptionBehaviour já converte exceções em ErrorResponse
        var response = await executeCore();

        _timer.Stop();

        // Log de performance se demorou muito
        if (_timer.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning("[Audit] Slow Request: {Name} | Duration: {Duration}ms | User: {UserId}",
                requestName, _timer.ElapsedMilliseconds, userId);
        }

        // Enfileira auditoria apenas se passar nos filtros
        if (shouldAudit)
        {
            // Verifica se deve salvar o body (default: true, exceto se [Auditable(SaveRequestBody = false)])
            var saveRequestBody = auditableAttr?.SaveRequestBody ?? true;

            await EnqueueAuditLogAsync(
                request,
                requestName,
                category,
                userId,
                executedAt,
                _timer.ElapsedMilliseconds,
                response.Success,
                response.StatusCode,
                response.Success ? null : response.Message,
                saveRequestBody,
                cancellationToken
            );
        }

        return response;
    }

    /// <summary>
    /// Determina se o request deve ser auditado baseado nas regras:
    /// 1. Se tem [Auditable(AllowAnonymous = true)]: audita mesmo sem usuário
    /// 2. Se é anonymous sem [Auditable(AllowAnonymous = true)]: NÃO audita
    /// 3. Commands: SEMPRE auditados
    /// 4. Queries: APENAS se tiver [Auditable]
    /// </summary>
    /// <returns>Tupla com (shouldAudit, auditableAttribute para acessar configs)</returns>
    private (bool shouldAudit, AuditableAttribute? attr) ShouldAuditRequest(string requestName, string userId)
    {
        var requestType = typeof(TRequest);
        var auditableAttr = requestType.GetCustomAttribute<AuditableAttribute>();
        var isAnonymous = userId == "anonymous" || string.IsNullOrEmpty(_user.Id);

        // Regra 1: Se tem [Auditable(AllowAnonymous = true)], audita mesmo sem usuário
        if (auditableAttr != null && auditableAttr.AllowAnonymous)
        {
            return (true, auditableAttr);
        }

        // Regra 2: NUNCA auditar anonymous (exceto se AllowAnonymous = true acima)
        if (isAnonymous)
        {
            return (false, null);
        }

        // Regra 3: Commands SEMPRE são auditados
        if (requestName.EndsWith("Command"))
        {
            return (true, auditableAttr);
        }

        // Regra 4: Queries só são auditadas se tiverem [Auditable]
        if (requestName.EndsWith("Query"))
        {
            return (auditableAttr != null, auditableAttr);
        }

        // Default: não auditar (segurança)
        return (false, null);
    }

    private async Task EnqueueAuditLogAsync(
        TRequest request,
        string requestName,
        string category,
        string userId,
        DateTime executedAt,
        long durationMs,
        bool success,
        int statusCode,
        string? errorMessage,
        bool saveRequestBody,
        CancellationToken cancellationToken)
    {
        try
        {
            // Serializa e criptografa o request para auditoria (se permitido)
            string? requestBodyEncrypted = null;
            string? encryptionKeyId = null;

            // Só salva o body se:
            // 1. saveRequestBody = true (configuração do [Auditable])
            // 2. Tem JTI para criptografar (usuário autenticado com token)
            if (saveRequestBody && !string.IsNullOrEmpty(_user.Jti))
            {
                try
                {
                    var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });

                    // Usa JTI do ICurrentUser como chave de criptografia
                    requestBodyEncrypted = EncryptWithAes(requestJson, _user.Jti, _user.X_Tenant_ID.ToString());
                    encryptionKeyId = _user.Jti;
                }
                catch
                {
                    // Se falhar a criptografia, não salva o body (segurança)
                }
            }

            // Obtém connection string do cache para passar ao job Hangfire
            // O job roda em background sem HttpContext, então precisa da connection string
            string? connectionString = null;
            if (_user.X_Tenant_ID != Guid.Empty)
            {
                connectionString = await _tenantCacheService.GetConnectionStringAsync(_user.X_Tenant_ID, cancellationToken);
            }

            var entry = new AuditLogEntry(
                userId,
                _user.Name,
                _user.Email,
                _user.X_Tenant_ID,
                requestName,
                category,
                _user.HttpMethod,
                _user.Endpoint,
                executedAt,
                durationMs,
                success,
                statusCode,
                errorMessage,
                requestBodyEncrypted,
                encryptionKeyId,
                _user.IpAddress,
                _user.UserAgent,
                connectionString
            );

            _auditService.EnqueueAuditLog(entry);
        }
        catch (Exception ex)
        {
            // Auditoria nunca deve quebrar a request
            _logger.LogError(ex, "[Audit] Failed to enqueue audit log for {Name}", requestName);
        }
    }

    /// <summary>
    /// Extrai a categoria do nome do request (ex: CreateProductCommand -> Products)
    /// </summary>
    private static string ExtractCategory(string requestName)
    {
        // Remove sufixos comuns
        var name = requestName
            .Replace("Command", "")
            .Replace("Query", "")
            .Replace("Handler", "");

        // Tenta extrair a entidade (ex: CreateProduct -> Product -> Products)
        var verbs = new[] { "Create", "Update", "Delete", "Get", "GetAll", "List", "Search", "Sync", "Approve", "Reject", "Cancel", "Send", "Resend", "Generate", "Import", "Export" };
        foreach (var verb in verbs)
        {
            if (name.StartsWith(verb))
            {
                var entity = name.Substring(verb.Length);
                if (!string.IsNullOrEmpty(entity))
                {
                    // Pluraliza simples
                    return entity.EndsWith("s") ? entity : entity + "s";
                }
            }
        }

        return "General";
    }

    /// <summary>
    /// Criptografa dados usando AES-256-CBC.
    /// A chave é derivada do JTI + TenantId usando PBKDF2.
    /// Retorna: Base64(IV + CipherText)
    /// </summary>
    private static string EncryptWithAes(string plainText, string jti, string tenantId)
    {
        // Deriva chave de 256 bits usando PBKDF2
        var salt = Encoding.UTF8.GetBytes($"audit_{tenantId}");
        using var keyDerivation = new Rfc2898DeriveBytes(jti, salt, 10000, HashAlgorithmName.SHA256);
        var key = keyDerivation.GetBytes(32); // 256 bits

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Concatena IV + CipherText
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Descriptografa dados criptografados com EncryptWithAes.
    /// Usado para auditoria manual quando necessário.
    /// </summary>
    public static string DecryptWithAes(string encryptedBase64, string jti, string tenantId)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedBase64);

        // Deriva a mesma chave
        var salt = Encoding.UTF8.GetBytes($"audit_{tenantId}");
        using var keyDerivation = new Rfc2898DeriveBytes(jti, salt, 10000, HashAlgorithmName.SHA256);
        var key = keyDerivation.GetBytes(32);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extrai IV (primeiros 16 bytes)
        var iv = new byte[16];
        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, 16);
        aes.IV = iv;

        // Extrai CipherText (resto)
        var cipherBytes = new byte[encryptedBytes.Length - 16];
        Buffer.BlockCopy(encryptedBytes, 16, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
