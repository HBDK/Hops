using System.Collections.Generic;

namespace Tools.Models;

public record ComposeLabels(string ConfigHash, string Image, string Project, string ComposeFilePath, string FolderPath, string serviceName, string ComposeVerison)
{
    public static string ConfigHashKey= "com.docker.compose.config-hash";
    public static string ImageKey= "com.docker.compose.image";
    public static string ProjectKey= "com.docker.compose.project";
    public static string ComposeFilePathKey= "com.docker.compose.project.config_files";
    public static string FolderPathKey= "com.docker.compose.project.working_dir";
    public static string ServiceNameKey= "com.docker.compose.service";
    public static string ComposeVersionKey= "com.docker.compose.version";

    public static ComposeLabels GetLabels(IDictionary<string, string> dict)
    {
        return new(
            ConfigHash: dict[ConfigHashKey],
            Image: dict[ImageKey],
            Project: dict[ProjectKey],
            ComposeFilePath: dict[ComposeFilePathKey],
            FolderPath: dict[FolderPathKey],
            serviceName: dict[ServiceNameKey],
            ComposeVerison: dict[ComposeFilePathKey]
        );
    }
}