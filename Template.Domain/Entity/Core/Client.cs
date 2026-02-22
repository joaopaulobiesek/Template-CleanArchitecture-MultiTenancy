using System.Text;
using Template.Domain.Interfaces.Core;
using Template.Domain.Validation;

namespace Template.Domain.Entity.Core;

public sealed class Client : Entity
{
    public string FullName { get; private set; }
    public string DocumentNumber { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public bool Paid { get; private set; }
    public string? ConnectionString { get; private set; }
    public string? StorageConfiguration { get; private set; }
    public string? SendGridConfiguration { get; private set; }
    public string Url { get; private set; }
    public string? TimeZoneId { get; private set; }

    /// <summary>
    /// ID do usuário associado a este client (opcional).
    /// Quando preenchido, usuários com Role.User só podem ver/editar este client se o UserId corresponder.
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Lista de IPs permitidos em formato JSON, armazenado em Base64.
    /// Formato decodificado: ["192.168.1.1","10.0.0.1"]
    /// Se null/vazio, não há restrição de IP (acesso livre).
    /// </summary>
    public string? AllowedIpsJson { get; private set; }

    public ICollection<ClientModule> Modules { get; private set; }

    public Client()
    {
        Modules = new List<ClientModule>();
    }

    public void RegisterClient(IRegisterClient c, string userId)
    {
        ValidateFullName(c.FullName);
        ValidateDocumentNumber(c.DocumentNumber);
        ValidatePhone(c.Phone);
        ValidateEmail(c.Email);

        FullName = c.FullName;
        Email = c.Email;
        UserId = userId;
        DocumentNumber = StringFormatter.FormatCpfOrCnpj(c.DocumentNumber);
        Phone = string.IsNullOrEmpty(c.Phone) ? null : StringFormatter.FormatPhoneNumber(c.Phone);
        Paid = false;
    }

    public void CreateClient(ICreateClient c)
    {
        ValidateFullName(c.FullName);
        ValidateDocumentNumber(c.DocumentNumber);
        ValidatePhone(c.Phone);
        ValidateEmail(c.Email);

        if (!string.IsNullOrEmpty(c.ConnectionString))
        {
            var bytes = Encoding.UTF8.GetBytes(c.ConnectionString);
            var connectionString = Convert.ToBase64String(bytes);
            ConnectionString = connectionString;
        }

        if (!string.IsNullOrEmpty(c.StorageConfiguration))
        {
            var bytes = Encoding.UTF8.GetBytes(c.StorageConfiguration);
            var storageConfig = Convert.ToBase64String(bytes);
            StorageConfiguration = storageConfig;
        }

        if (!string.IsNullOrEmpty(c.SendGridConfiguration))
        {
            var bytes = Encoding.UTF8.GetBytes(c.SendGridConfiguration);
            var sendGridConfig = Convert.ToBase64String(bytes);
            SendGridConfiguration = sendGridConfig;
        }

        FullName = c.FullName;
        Email = c.Email;
        Paid = c.Paid;
        Url = c.Url;
        TimeZoneId = c.TimeZoneId;
        UserId = c.UserId;
        DocumentNumber = StringFormatter.FormatCpfOrCnpj(c.DocumentNumber);
        Phone = string.IsNullOrEmpty(c.Phone) ? null : StringFormatter.FormatPhoneNumber(c.Phone);

        if (!string.IsNullOrEmpty(c.AllowedIpsJson))
        {
            var bytes = Encoding.UTF8.GetBytes(c.AllowedIpsJson);
            AllowedIpsJson = Convert.ToBase64String(bytes);
        }
        else
        {
            AllowedIpsJson = null;
        }
    }

    public void UpdateClient(IUpdateClient c)
    {
        DomainExceptionValidation.When(!Active, "Cannot update a deleted client.");
        ValidateFullName(c.FullName);
        ValidatePhone(c.Phone);
        ValidateEmail(c.Email);

        if (!string.IsNullOrEmpty(c.ConnectionString))
        {
            var bytes = Encoding.UTF8.GetBytes(c.ConnectionString);
            var connectionString = Convert.ToBase64String(bytes);
            ConnectionString = connectionString;
        }

        if (!string.IsNullOrEmpty(c.StorageConfiguration))
        {
            var bytes = Encoding.UTF8.GetBytes(c.StorageConfiguration);
            var storageConfig = Convert.ToBase64String(bytes);
            StorageConfiguration = storageConfig;
        }

        if (!string.IsNullOrEmpty(c.SendGridConfiguration))
        {
            var bytes = Encoding.UTF8.GetBytes(c.SendGridConfiguration);
            var sendGridConfig = Convert.ToBase64String(bytes);
            SendGridConfiguration = sendGridConfig;
        }

        FullName = c.FullName;
        Email = c.Email;
        Paid = c.Paid;
        if (!string.IsNullOrEmpty(c.Url))
            Url = c.Url;
        TimeZoneId = c.TimeZoneId;
        if (!string.IsNullOrEmpty(c.UserId))
            UserId = c.UserId;
        Phone = string.IsNullOrEmpty(c.Phone) ? null : StringFormatter.FormatPhoneNumber(c.Phone);

        if (!string.IsNullOrEmpty(c.AllowedIpsJson))
        {
            var bytes = Encoding.UTF8.GetBytes(c.AllowedIpsJson);
            AllowedIpsJson = Convert.ToBase64String(bytes);
        }
        else
        {
            AllowedIpsJson = null;
        }
    }

    public void DeleteClient()
        => Inactivate();

    private void ValidateFullName(string fullName)
    {
        DomainExceptionValidation.ValidateRequiredString(fullName, "Full name is required.");
        DomainExceptionValidation.ValidateMaxLength(fullName, 100, "Full name can have a maximum of 100 characters.");
    }

    private void ValidateDocumentNumber(string documentNumber)
    {
        DomainExceptionValidation.ValidateRequiredString(documentNumber, "Document number is required.");
        DomainExceptionValidation.ValidateMaxLength(documentNumber, 20, "Document number can have a maximum of 20 characters.");
        DomainExceptionValidation.ValidateFormat(StringFormatter.IsValidCpfOrCnpj, documentNumber, "Invalid CPF or CNPJ.");
    }

    private void ValidatePhone(string? phone)
    {
        DomainExceptionValidation.ValidateMaxLength(phone, 20, "Phone number can have a maximum of 20 characters.");
        DomainExceptionValidation.ValidateFormat(StringFormatter.IsValidPhoneNumber, phone, "Invalid phone number.");
    }

    private void ValidateEmail(string? email)
    {
        DomainExceptionValidation.ValidateMaxLength(email, 254, "Email can have a maximum of 254 characters.");
        DomainExceptionValidation.ValidateEmailFormat(email, "Invalid email format.");
    }

}