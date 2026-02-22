namespace Template.Domain.Interfaces.Core;

/// <summary>
/// Interface para registro simples de client (RegisterCore)
/// </summary>
public interface IRegisterClient
{
    string FullName { get; }
    string DocumentNumber { get; }
    string Email { get; }
    string? Phone { get; }
}

public interface ICreateClient
{
    string FullName { get; set; }
    string DocumentNumber { get; set; }
    string Email { get; set; }
    string? Phone { get; set; }
    bool Paid { get; set; }
    string? ConnectionString { get; set; }
    string? StorageConfiguration { get; set; }
    string? SendGridConfiguration { get; set; }
    string Url { get; set; }
    string? TimeZoneId { get; set; }
    /// <summary>
    /// ID do usuário associado a este client (opcional)
    /// </summary>
    string? UserId { get; set; }
    /// <summary>
    /// Lista de IPs permitidos em formato JSON. Ex: ["192.168.1.1","10.0.0.1"]
    /// </summary>
    string? AllowedIpsJson { get; set; }
}

public interface IUpdateClient
{
    string FullName { get; set; }
    string Email { get; set; }
    string? Phone { get; set; }
    bool Paid { get; set; }
    string? ConnectionString { get; set; }
    string? StorageConfiguration { get; set; }
    string? SendGridConfiguration { get; set; }
    string? Url { get; set; }
    string? TimeZoneId { get; set; }
    /// <summary>
    /// ID do usuário associado a este client (opcional)
    /// </summary>
    string? UserId { get; set; }
    /// <summary>
    /// Lista de IPs permitidos em formato JSON. Ex: ["192.168.1.1","10.0.0.1"]
    /// </summary>
    string? AllowedIpsJson { get; set; }
}