using Template.Application.Common.Security;
using Template.Domain.Constants;

namespace Template.Application.Domains.V1.ExternalServices.AzureBlobStorage.Commands.Delete;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = Policies.CanPurge)]
public class FileDeleteCommand
{
    public string FileName { get; set; }
}