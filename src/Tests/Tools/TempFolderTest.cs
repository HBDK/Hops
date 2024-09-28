using FluentAssertions;
using System.IO;

namespace Tests.Tools;

public class TempFolderTest
{
    [Fact]
    public void ShouldCreateAndDisposeFolder()
    {
        DirectoryInfo dir;
        using (var folder = new TempFolder())
        {
            dir = folder.DirectoryInfo;
            dir.Exists.Should().BeTrue();
            Directory.Exists(dir.FullName).Should().BeTrue();
        }

        Directory.Exists(dir.FullName).Should().BeFalse();
        dir.Exists.Should().BeFalse();
    }
}