using System;
using System.IO;

namespace Tests.Tools;

class TempFolder : IDisposable
{
    public readonly DirectoryInfo DirectoryInfo;

    public TempFolder(string? folderPrefix = null)
    {
        DirectoryInfo = Directory.CreateTempSubdirectory(folderPrefix);
    }

    public void Dispose()
    {
        RecursiveDelete(DirectoryInfo);
    }
    private void RecursiveDelete(DirectoryInfo dir)
    {
        var files = dir.GetFiles();
        var folders = dir.GetDirectories();

        foreach (var file in files)
        {
            file.Delete();
        }

        foreach (var folder in folders)
        {
            RecursiveDelete(folder);
        }

        dir.Delete();
    }
}