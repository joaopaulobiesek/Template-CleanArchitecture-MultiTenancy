using System.Security.Cryptography;
using System.Text;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Tenant.V1.ViewModels.Audit;
using Template.Domain.Constants;

namespace Template.Application.Domains.Tenant.V1.Audit.Queries.DecryptAuditLog;

/// <summary>
/// Query para buscar log de auditoria por ID com RequestBody descriptografado
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
public class DecryptAuditLogQuery
{
    public Guid Id { get; set; }
}

public class DecryptAuditLogQueryHandler : HandlerBase<DecryptAuditLogQuery, AuditLogVM>
{
    private readonly IAuditLogRepository _repository;

    public DecryptAuditLogQueryHandler(
        HandlerDependencies<DecryptAuditLogQuery, AuditLogVM> dependencies,
        IAuditLogRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<AuditLogVM>> RunCore(
        DecryptAuditLogQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var auditLog = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (auditLog == null)
        {
            return new ErrorResponse<AuditLogVM>("Log de auditoria não encontrado.", 404);
        }

        // Mapeia para ViewModel
        var vm = AuditLogVM.FromDomain(auditLog);

        // Descriptografa o RequestBody se disponível
        if (!string.IsNullOrEmpty(auditLog.RequestBodyEncrypted) &&
            !string.IsNullOrEmpty(auditLog.EncryptionKeyId))
        {
            try
            {
                vm.RequestBodyDecrypted = DecryptWithAes(
                    auditLog.RequestBodyEncrypted,
                    auditLog.EncryptionKeyId,
                    auditLog.TenantId.ToString()
                );
            }
            catch (Exception)
            {
                vm.RequestBodyDecrypted = "[Erro ao descriptografar]";
            }
        }

        return new SuccessResponse<AuditLogVM>("Log recuperado com sucesso.", vm);
    }

    /// <summary>
    /// Descriptografa dados usando AES-256-CBC (mesma lógica do AuditBehaviour)
    /// </summary>
    private static string DecryptWithAes(string encryptedBase64, string jti, string tenantId)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedBase64);

        var salt = Encoding.UTF8.GetBytes($"audit_{tenantId}");
        using var keyDerivation = new Rfc2898DeriveBytes(jti, salt, 10000, HashAlgorithmName.SHA256);
        var key = keyDerivation.GetBytes(32);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var iv = new byte[16];
        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, 16);
        aes.IV = iv;

        var cipherBytes = new byte[encryptedBytes.Length - 16];
        Buffer.BlockCopy(encryptedBytes, 16, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
