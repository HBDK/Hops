using System;
using System.IO;
using FluentAssertions;
using LibGit2Sharp;
using Tests.Tools;

namespace Tests.GIT;

public class FolderGitTest
{
    [Fact]
    public void ShouldCloneRepo()
    {
        // Given
        var fileName = "test.txt";
        using var sourceDir = new TempFolder($"{nameof(FolderGitTest)}-SourceDir-");
        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");

        using var sourceRepo = new Repository(Repository.Init(sourceDir.DirectoryInfo.FullName));

        using (var file = File.CreateText(sourceDir.DirectoryInfo.FullName + "/" + fileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }
        sourceRepo.Index.Add(fileName);
        sourceRepo.Index.Write();

        var signature = GetAuthor();

        sourceRepo.Commit("test1", signature, signature);
        // When
        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        distRepo.Clone(sourceDir.DirectoryInfo.FullName);
        // Then

        File.Exists(distDir.DirectoryInfo.FullName + "/" + fileName).Should().BeTrue();
    }

    [Fact]
    public void ShouldPullChanges()
    {
        // Given
        var signature = GetAuthor();
        var firstFileName = "test.txt";
        var secondFileName = "test2.txt";

        using var sourceDir = new TempFolder($"{nameof(FolderGitTest)}-SourceDir-");
        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");

        using var sourceRepo = new Repository(Repository.Init(sourceDir.DirectoryInfo.FullName));

        using (var file = File.CreateText(sourceDir.DirectoryInfo.FullName + "/" + firstFileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }
        sourceRepo.Index.Add(firstFileName);
        sourceRepo.Index.Write();


        sourceRepo.Commit("test1", signature, signature);

        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        distRepo.Clone(sourceDir.DirectoryInfo.FullName);

        using (var file = File.CreateText(sourceDir.DirectoryInfo.FullName + "/" + secondFileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }
        sourceRepo.Index.Add(secondFileName);
        sourceRepo.Index.Write();


        sourceRepo.Commit("test2", signature, signature);
        // When
        distRepo.Fetch();
        distRepo.CheckOutAndReset("origin/master");

        // Then
        File.Exists(distDir.DirectoryInfo.FullName + "/" + secondFileName).Should().BeTrue();
    }


    [Fact]
    public void ShouldRemovetrackedFilesWhenPullingChanges()
    {
        // Given
        var signature = GetAuthor();
        var firstFileName = "test.txt";
        var secondFileName = "test2.txt";

        using var sourceDir = new TempFolder($"{nameof(FolderGitTest)}-SourceDir-");
        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");

        using var sourceRepo = new Repository(Repository.Init(sourceDir.DirectoryInfo.FullName));

        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        distRepo.Clone(sourceDir.DirectoryInfo.FullName);

        using (var file = File.CreateText(sourceDir.DirectoryInfo.FullName + "/" + firstFileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }
        sourceRepo.Index.Add(firstFileName);
        sourceRepo.Index.Write();


        sourceRepo.Commit("test1", signature, signature);

        using (var file = File.CreateText(distDir.DirectoryInfo.FullName + "/" + secondFileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }

        using var distRepository = new Repository(distDir.DirectoryInfo.FullName);
        distRepository.Index.Add(secondFileName);
        distRepository.Index.Write();

        // When
        distRepo.Fetch();
        distRepo.CheckOutAndReset("origin/master");

        // Then
        File.Exists(distDir.DirectoryInfo.FullName + "/" + firstFileName).Should().BeTrue();
        File.Exists(distDir.DirectoryInfo.FullName + "/" + secondFileName).Should().BeFalse();
    }

    [Fact]
    public void ShouldRemoveUntrackedFilesWhenPullingChanges()
    {
        // Given
        var signature = GetAuthor();
        var firstFileName = "test.txt";
        var secondFileName = "test2.txt";

        using var sourceDir = new TempFolder($"{nameof(FolderGitTest)}-SourceDir-");
        using var distDir = new TempFolder($"{nameof(FolderGitTest)}-DistDir-");

        using var sourceRepo = new Repository(Repository.Init(sourceDir.DirectoryInfo.FullName));

        var distRepo = new GitRepo(distDir.DirectoryInfo.FullName);
        distRepo.Clone(sourceDir.DirectoryInfo.FullName);

        using (var file = File.CreateText(sourceDir.DirectoryInfo.FullName + "/" + firstFileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }
        sourceRepo.Index.Add(firstFileName);
        sourceRepo.Index.Write();


        sourceRepo.Commit("test1", signature, signature);

        using (var file = File.CreateText(distDir.DirectoryInfo.FullName + "/" + secondFileName))
        {
            file.WriteLineAsync("test");
            file.Close();
        }

        // When
        distRepo.Fetch();
        distRepo.CheckOutAndReset("origin/master");

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