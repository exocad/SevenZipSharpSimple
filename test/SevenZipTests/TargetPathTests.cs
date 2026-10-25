using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace SevenZipTests;

[Collection("SevenZip")]
public class TargetPathTests
{
    [Theory]
    [InlineData(@"C:\data\", @"C:\data\")]
    [InlineData(@"C:\data\..\somewhere\", @"somewhere\")]
    [InlineData(@"C:\data\..\somewhere\..\..\anywhere\", @"anywhere\")]
    public void TestSanitize(string archiveEntryPath, string expected)
    {
        var target = SevenZip.Detail.TargetPath.Sanitize(archiveEntryPath);

        Assert.Equal(expected, target);
    }

    [Theory]
    [MemberData(nameof(InvalidArchiveEntryPaths))]
    public void TestGetSecureTargetPathOrThrowWithInvalidArchiveEntryPath(string baseDir, string archiveEntryPath)
    {
        Assert.ThrowsAny<IOException>(() =>
        {
            SevenZip.Detail.TargetPath.GetSecureTargetPathOrThrow(baseDir, archiveEntryPath, false);
        });
    }

    /// <summary>
    /// Archive entry paths which are absolute (rooted) and thus point outside of the
    /// given base directory. The notation of an absolute path is platform specific,
    /// e.g. <c>C:\file.txt</c> on Windows or <c>/file.txt</c> on Linux.
    /// </summary>
    public static TheoryData<string, string> InvalidArchiveEntryPaths =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new TheoryData<string, string>
            {
                ("C:\\Extract", "\\filename.exe"),
                ("C:\\Extract", "C:\\filename.exe"),
                ("C:\\Extract\\", "C:\\targetdir\\filename.exe"),
            }
            : new TheoryData<string, string>
            {
                ("/Extract", "/filename.exe"),
                ("/Extract/", "/targetdir/filename.exe"),
                ("/Extract", "/somewhere/else.txt"),
            };

    [Theory]
    [InlineData("C:\\", "C:\\targetdir\\filename.exe")]
    [InlineData("C:\\", ".\\filename.exe")]
    [InlineData("C:\\", "filename.exe")]
    [InlineData("C:\\subdir", "C:\\subdir\\targetdir\\filename.exe")]
    [InlineData("C:\\subdir", ".\\filename.exe")]
    [InlineData("C:\\subdir", "filename.exe")]
    public void TestGetSecureTargetPathOrThrow(string baseDir, string archiveEntryPath)
    {
        var canonicalBaseDir = Path.GetFullPath(baseDir);
        var path = SevenZip.Detail.TargetPath.GetSecureTargetPathOrThrow(baseDir, archiveEntryPath, false);

        Assert.StartsWith(canonicalBaseDir, path);
    }
}
