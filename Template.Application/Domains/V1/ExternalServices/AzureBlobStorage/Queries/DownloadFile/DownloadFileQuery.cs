using Template.Application.Common.Security;
using Template.Domain.Constants;

namespace Template.Application.Domains.V1.ExternalServices.AzureBlobStorage.Queries.DownloadFile;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = Policies.CanView)]
public class DownloadFileQuery
{
    public string FileName { get; set; }
}