using Template.Domain.Entity.Core;
using System.Text;

namespace Template.Application.Domains.Core.V1.ViewModels;

/// <summary>
/// ViewModel completo do Cliente para GetById.
/// Inclui todos os campos de configuração (descriptografados).
/// </summary>
public class ClientDetailVM
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string DocumentNumber { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public bool Paid { get; set; }
    public bool Active { get; set; }
    public string Url { get; set; }
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// Connection string do banco de dados do tenant (descriptografada)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Configuração do Azure Storage em JSON (descriptografada)
    /// </summary>
    public string? StorageConfiguration { get; set; }

    /// <summary>
    /// Configuração do SendGrid em JSON (descriptografada)
    /// </summary>
    public string? SendGridConfiguration { get; set; }

    /// <summary>
    /// Lista de IPs permitidos em formato JSON (descriptografada).
    /// Ex: ["192.168.1.1","10.0.0.1"]
    /// </summary>
    public string? AllowedIpsJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ClientDetailVM() { }

    /// <summary>
    /// Converte entidade para ViewModel.
    /// Por padrão, campos sensíveis vêm null (segurança).
    /// </summary>
    /// <param name="client">Entidade Client</param>
    /// <param name="includeSensitiveData">Se true, inclui dados sensíveis (apenas para Admin/TI)</param>
    public static ClientDetailVM FromEntity(Client? client, bool includeSensitiveData = false)
    {
        if (client == null) return new ClientDetailVM();

        return new ClientDetailVM
        {
            Id = client.Id,
            FullName = client.FullName,
            DocumentNumber = client.DocumentNumber,
            Email = client.Email,
            Phone = client.Phone,
            Paid = client.Paid,
            Active = client.Active,
            Url = client.Url,
            TimeZoneId = client.TimeZoneId,
            // Por padrão null - só popula se includeSensitiveData = true
            ConnectionString = includeSensitiveData ? DecryptBase64(client.ConnectionString) : null,
            StorageConfiguration = includeSensitiveData ? DecryptBase64(client.StorageConfiguration) : null,
            SendGridConfiguration = includeSensitiveData ? DecryptBase64(client.SendGridConfiguration) : null,
            AllowedIpsJson = DecryptBase64(client.AllowedIpsJson),
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }

    /// <summary>
    /// Descriptografa string Base64
    /// </summary>
    public static string? DecryptBase64(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return base64String; // Retorna original se falhar
        }
    }
}
