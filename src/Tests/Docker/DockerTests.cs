using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Docker.DotNet;
using FluentAssertions;
using Tests.Tools;
using Tools;
using Tools.Models;

namespace Tests.Docker;

public class DockerTests
{
    [Fact]
    public async Task ShouldCreateContainer()
    {
        // Given
        using var folder = new TempFolder();
        var filePath = folder.DirectoryInfo.FullName + "/compose.yaml";
        CreateHelloWorldCompose(filePath);
        
        // When
        await new DockerCommandBuilder().Compose().WithFile(filePath).Up().Run();
        var labels = await GetLabelsAsync(filePath); 
        await new DockerCommandBuilder().Compose().WithFile(filePath).Down().Run();
    
        // Then
        labels.Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldRemoveOrphansAsync()
    {
        // Given
        using var folder = new TempFolder();
        var firstStack = Directory.CreateDirectory(folder.DirectoryInfo.FullName + "/" + Guid.NewGuid()).FullName + "/compose.yaml";
        var secondStack = Directory.CreateDirectory(folder.DirectoryInfo.FullName + "/" + Guid.NewGuid()).FullName + "/compose.yaml";
        CreateHelloWorldCompose(firstStack);
       
        var file = new ComposeFile(new());
        file.services.Add("hello_world1",new("hello-world"));
        file.services.Add("hello_world2",new("hello-world"));
        File.AppendAllLines(secondStack, [JsonSerializer.Serialize(file)]);
        // When
        await new DockerCommandBuilder().Compose().WithFile(firstStack).Up().Run();
        await new DockerCommandBuilder().Compose().WithFile(secondStack).Up().Run();

        var secondStackLabelsBeforeChange = await GetLabelsAsync(secondStack);

        File.Delete(secondStack);
        CreateHelloWorldCompose(secondStack);
        await new DockerCommandBuilder().Compose().WithFile(secondStack).Up().Run();

        var firstStackLabels = await GetLabelsAsync(firstStack);
        var secondStackLabels = await GetLabelsAsync(secondStack);

        await new DockerCommandBuilder().Compose().WithFile(firstStack).Down().Run();
        await new DockerCommandBuilder().Compose().WithFile(secondStack).Down().Run();
    
        // Then
        firstStackLabels.Should().HaveCount(1);
        secondStackLabelsBeforeChange.Should().HaveCount(2);
        secondStackLabels.Should().HaveCount(1);
    }

    private async Task<IEnumerable<ComposeLabels>> GetLabelsAsync(string path)
    {
        using var client = new DockerClientConfiguration().CreateClient();
        var containers = await client.Containers.ListContainersAsync(new() {All = true});
        return containers.Where(x => x.Labels.Any(y => y.Key == ComposeLabels.ComposeFilePathKey && y.Value == path)).Select(x => ComposeLabels.GetLabels(x.Labels));
    }

    private void CreateHelloWorldCompose(string filePath)
    {
        var file = new ComposeFile(new());
        file.services.Add("hello_world",new("hello-world"));
        File.AppendAllLines(filePath, [JsonSerializer.Serialize(file)]);
    }
}

public record ComposeFile(Dictionary<string, ComposeService> services);
public record ComposeService(string image);