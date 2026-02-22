namespace Template.Application.Common.Security;

/// <summary>
/// Marca um Command ou Query para auditoria com opções customizadas.
///
/// Para Queries: Por padrão NÃO são auditadas, use [Auditable] para forçar.
/// Para Commands: Por padrão SÃO auditados, use [Auditable] para customizar opções.
///
/// Opções disponíveis:
/// - Reason: Motivo da auditoria (documentação)
/// - SaveRequestBody: Se deve salvar o corpo da requisição (default: true)
/// - AllowAnonymous: Se deve auditar mesmo sem usuário logado (default: false)
/// </summary>
/// <remarks>
/// Exemplos de uso:
///
/// // Query sensível que precisa ser auditada
/// [Auditable("Acesso a dados sensíveis")]
/// public class GetAllUsersQuery { }
///
/// // Login - auditar sem usuário e sem salvar senha
/// [Auditable("Usuário logou", SaveRequestBody = false, AllowAnonymous = true)]
/// public class LoginUserCommand { }
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AuditableAttribute : Attribute
{
    /// <summary>
    /// Motivo pelo qual esta operação deve ser auditada.
    /// Útil para documentação e revisão de segurança.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Se deve salvar o corpo da requisição criptografado.
    /// Default: true
    /// Use false para operações com dados sensíveis como senhas.
    /// </summary>
    public bool SaveRequestBody { get; set; } = true;

    /// <summary>
    /// Se deve auditar mesmo quando o usuário não está autenticado.
    /// Default: false
    /// Use true para operações como Login e Register.
    /// </summary>
    public bool AllowAnonymous { get; set; } = false;

    public AuditableAttribute() { }

    public AuditableAttribute(string reason)
    {
        Reason = reason;
    }
}
