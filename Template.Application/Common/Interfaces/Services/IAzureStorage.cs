using Template.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Template.Application.Domains.V1.ViewModels.Storage;

namespace Template.Application.Common.Interfaces.Services;

public interface IAzureStorage
{
    Task<ApiResponse<UploadFileVM>> UploadFile(IFormFile file, CancellationToken cancellationToken);

    /// <summary>
    /// Faz upload de bytes diretamente para o blob storage em um path específico.
    /// Usado para upload de arquivos vindos de fontes externas (ex: D4Sign).
    /// </summary>
    /// <param name="fileBytes">Bytes do arquivo</param>
    /// <param name="fileName">Nome do arquivo (ex: contrato_assinado.pdf)</param>
    /// <param name="path">Caminho no blob (ex: contracts/{id}/documents/)</param>
    /// <param name="contentType">Tipo MIME do arquivo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>URL do arquivo no blob</returns>
    Task<ApiResponse<UploadFileVM>> UploadBytes(byte[] fileBytes, string fileName, string path, string? contentType, CancellationToken cancellationToken);

    string GenerateSasToken(string fileName, int expiracaoMinutos = 5);

    /// <summary>
    /// Gera um SAS Token de leitura para um blob no container principal.
    /// Usado para gerar URLs temporárias de acesso a arquivos privados.
    /// </summary>
    string GenerateReadSasToken(string blobName, int expiracaoMinutos = 5);

    Task<ApiResponse<string>> MoveFile(string path, string fileName, CancellationToken cancellationToken);
    Task<ApiResponse<string>> DeleteFile(string fileName);
    Task<ApiResponse<byte[]>> DownloadFile(string fileName);
}