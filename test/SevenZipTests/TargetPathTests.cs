using System.IO;
using SevenZip;
using Xunit;

namespace SevenZipTests;

public class TargetPathTests
{
    [Theory]
    [InlineData(@"C:\data\", @"C:\data\")]
    [InlineData(@"C:\data\..\somewhere\", @"somewhere\")]
    [InlineData(@"C:\data\..\somewhere\..\..\anywhere\", @"anywhere\")]
    public void TestSanitize(string archiveEntryPath, string expected)
    {
        var target = SevenZip.Detail.TargetPath.Sanitize(CreateArchiveEntry(archiveEntryPath));

        Assert.Equal(expected, target);
    }

    [Theory]
    [InlineData("C:\\Extract", "\\filename.exe")]
    [InlineData("C:\\Extract", "C:\\filename.exe")]
    [InlineData("C:\\Extract\\", "C:\\targetdir\\filename.exe")]
    public void TestGetSecureTargetPathOrThrowWithInvalidArchiveEntryPath(string baseDir, string archiveEntryPath)
    {
        Assert.ThrowsAny<IOException>(() =>
        {
            SevenZip.Detail.TargetPath.GetSecureTargetPathOrThrow(baseDir, CreateArchiveEntry(archiveEntryPath), false);
        });
    }

    [Theory]
    [InlineData("C:\\", "C:\\targetdir\\filename.exe")]
    [InlineData("C:\\", ".\\filename.exe")]
    [InlineData("C:\\", "filename.exe")]
    public void TestGetSecureTargetPathOrThrow(string baseDir, string archiveEntryPath)
    {
        var canonicalBaseDir = Path.GetFullPath(baseDir);
        var path = SevenZip.Detail.TargetPath.GetSecureTargetPathOrThrow(baseDir, CreateArchiveEntry(archiveEntryPath), false);

        Assert.StartsWith(canonicalBaseDir, path);
    }

    private static ArchiveEntry CreateArchiveEntry(string path)
    {
        return new ArchiveEntry()
        {
            Path = path,
        };
    }
}
