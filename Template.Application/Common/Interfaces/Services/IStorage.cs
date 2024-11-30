using Template.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Template.Application.Domains.V1.ViewModels.Storage;

namespace Template.Application.Common.Interfaces.Services;

public interface IStorage
{
    Task<ApiResponse<UploadFileVM>> UploadFile(IFormFile file, CancellationToken cancellationToken);
    Task<ApiResponse<UploadFileVM>> DeleteFile(string fileName);
    Task<ApiResponse<byte[]>> DownloadFile(string fileName);
}