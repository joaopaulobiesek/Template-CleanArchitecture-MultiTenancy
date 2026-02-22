namespace Template.Application.Common.Models;

public class StorageConfiguration
{
    public const string Key = "Storage";
    public string ConnectionString { get; set; }
    public string ContainerName { get; set; }
    public string TempContainerName { get; set; }
}
