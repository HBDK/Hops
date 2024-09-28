using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using LibGit2Sharp;
using Tests.Tools;

namespace Tests.GIT;

public class SshGitTest
{
    [Fact]
    public async Task ShouldCloneRepoAsync()
    {
        // Given
        var fileName = "test.txt";
        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");

        // When
        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        await distRepo.SshClone("git@github.com:HBDK/test-repo.git");
        // Then

        File.Exists(distDir.DirectoryInfo.FullName + "/hello-world/compose.yaml").Should().BeTrue();
    }

    [Fact]
    public void ShouldPullChanges()
    {
        // TODO: test
    }


    [Fact]
    public async Task ShouldRemovetrackedFilesWhenPullingChangesAsync()
    {
        // Given
        var firstFileName = "hello-world/compose.yaml";
        var secondFileName = "test2.txt";

        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");


        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        await distRepo.SshClone("git@github.com:HBDK/test-repo.git");

        using (var file = File.CreateText(distDir.DirectoryInfo.FullName + "/" + secondFileName))
        {
            await file.WriteLineAsync("test");
            file.Close();
        }

        using var distRepository = new Repository(distDir.DirectoryInfo.FullName);
        distRepository.Index.Add(secondFileName);
        distRepository.Index.Write();

        // When
        await distRepo.SshFetch();
        await distRepo.SshCheckOutAndReset("origin/master");

        // Then
        File.Exists(distDir.DirectoryInfo.FullName + "/" + firstFileName).Should().BeTrue();
        File.Exists(distDir.DirectoryInfo.FullName + "/" + secondFileName).Should().BeFalse();
    }

    [Fact]
    public async void ShouldRemoveUntrackedFilesWhenPullingChanges()
    {
        // Given
        var firstFileName = "hello-world/compose.yaml";
        var secondFileName = "test2.txt";

        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");


        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        await distRepo.SshClone("git@github.com:HBDK/test-repo.git");

        using (var file = File.CreateText(distDir.DirectoryInfo.FullName + "/" + secondFileName))
        {
            await file.WriteLineAsync("test");
            file.Close();
        }

        // When
        await distRepo.SshFetch();
        await distRepo.SshCheckOutAndReset("origin/master");

        // Then
        File.Exists(distDir.DirectoryInfo.FullName + "/" + firstFileName).Should().BeTrue();
        File.Exists(distDir.DirectoryInfo.FullName + "/" + secondFileName).Should().BeFalse();
    }

    [Fact]
    public void ShouldOverWriteChangesWhenPullingChanges()
    {
        // Given
        var signature = GetAuthor();
        var fileName = "test.txt";
        var change = "change";

        using var sourceDir = new TempFolder($"{nameof(FolderGitTest)}-SourceDir-");
        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");

        using var sourceRepo = new Repository(Repository.Init(sourceDir.DirectoryInfo.FullName));


        using (var emptyFile = File.CreateText(sourceDir.DirectoryInfo.FullName + "/" + fileName))
        {
            emptyFile.WriteLineAsync("test");
            emptyFile.Close();
        }
        sourceRepo.Index.Add(fileName);
        sourceRepo.Index.Write();


        sourceRepo.Commit("test1", signature, signature);

        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        distRepo.Clone(sourceDir.DirectoryInfo.FullName);

        File.AppendAllLines(distDir.DirectoryInfo.FullName + "/" + fileName, [change]);
        using (var changedFile = File.OpenText(distDir.DirectoryInfo.FullName + "/" + fileName))
        {
            changedFile.ReadToEnd().Should().Contain(change);
        }

        // When
        distRepo.Fetch();
        distRepo.CheckOutAndReset("origin/master");

        // Then
        File.Exists(distDir.DirectoryInfo.FullName + "/" + fileName).Should().BeTrue();
        using var file = File.OpenText(distDir.DirectoryInfo.FullName + "/" + fileName);
        file.ReadToEnd().Should().NotContain(change);
    }

    private Signature GetAuthor(DateTime? time = null, string name = "tester", string email = "tester@test.dk") => new(name, email, time ?? DateTime.Now);
}