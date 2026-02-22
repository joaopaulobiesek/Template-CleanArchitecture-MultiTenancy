namespace Template.Domain.Entity.Tenant.Audit;

/// <summary>
/// Registro de auditoria de todas as operacoes realizadas no sistema.
/// Armazena quem fez, o que fez, quando, e o resultado.
/// O RequestBody e criptografado para proteger dados sensiveis.
/// </summary>
public sealed class AuditLog : Entity
{
    /// <summary>
    /// ID do usuario que executou a acao
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Nome do usuario no momento da acao
    /// </summary>
    public string? UserName { get; private set; }

    /// <summary>
    /// Email do usuario no momento da acao
    /// </summary>
    public string? UserEmail { get; private set; }

    /// <summary>
    /// ID do tenant onde a acao foi executada
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Nome da acao/command executado (ex: CreateProductCommand)
    /// </summary>
    public string Action { get; private set; }

    /// <summary>
    /// Categoria da acao para agrupamento (ex: Products, Users, Auth)
    /// </summary>
    public string Category { get; private set; }

    /// <summary>
    /// Metodo HTTP da requisicao (GET, POST, PUT, DELETE)
    /// </summary>
    public string? HttpMethod { get; private set; }

    /// <summary>
    /// Endpoint/rota acessada
    /// </summary>
    public string? Endpoint { get; private set; }

    /// <summary>
    /// Data/hora de execucao
    /// </summary>
    public DateTime ExecutedAt { get; private set; }

    /// <summary>
    /// Duracao da execucao em milissegundos
    /// </summary>
    public long DurationMs { get; private set; }

    /// <summary>
    /// Indica se a operacao foi bem sucedida
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Codigo de status HTTP retornado
    /// </summary>
    public int StatusCode { get; private set; }

    /// <summary>
    /// Mensagem de erro caso a operacao tenha falhado
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Corpo da requisicao CRIPTOGRAFADO (protege dados sensiveis)
    /// </summary>
    public string? RequestBodyEncrypted { get; private set; }

    /// <summary>
    /// Chave para descriptografar o RequestBody (JTI do token usado)
    /// So o admin do sistema pode usar para descriptografar manualmente
    /// </summary>
    public string? EncryptionKeyId { get; private set; }

    /// <summary>
    /// Endereco IP do cliente
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User-Agent do cliente (browser/app)
    /// </summary>
    public string? UserAgent { get; private set; }

    // Construtor privado para EF Core
    private AuditLog() { }

    /// <summary>
    /// Cria um novo registro de auditoria
    /// </summary>
    public AuditLog(
        string userId,
        string? userName,
        string? userEmail,
        Guid tenantId,
        string action,
        string category,
        string? httpMethod,
        string? endpoint,
        DateTime executedAt,
        long durationMs,
        bool success,
        int statusCode,
        string? errorMessage,
        string? requestBodyEncrypted,
        string? encryptionKeyId,
        string? ipAddress,
        string? userAgent)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        TenantId = tenantId;
        Action = action;
        Category = category;
        HttpMethod = httpMethod;
        Endpoint = endpoint;
        ExecutedAt = executedAt;
        DurationMs = durationMs;
        Success = success;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        RequestBodyEncrypted = requestBodyEncrypted;
        EncryptionKeyId = encryptionKeyId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
