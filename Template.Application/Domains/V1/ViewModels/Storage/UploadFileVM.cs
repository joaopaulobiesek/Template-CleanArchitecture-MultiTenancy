﻿namespace Template.Application.Domains.V1.ViewModels.Storage;

public class UploadFileVM
{
    public string FileName { get; set; }
    public string Path { get; set; }

    public UploadFileVM(string fileName, string path)
    {
        FileName = fileName;
        Path = path;
    }
}
