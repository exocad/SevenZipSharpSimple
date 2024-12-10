using System.IO;
using SevenZip;
using Xunit;

namespace SevenZipTests;

public class ArchiveReaderTests
{
    [Fact]
    public void ExtractZipArchiveWithInsecurePath()
    {
        Assert.Throws<IOException>(() =>
        {
            using var reader = new ArchiveReader(
                "test-data/absolute-path-traversal.zip",
                new Detail.ZipPathTraversalArchiveReaderDelegate());

            reader.ExtractAll("result-zip-path-traversal");
        });
    }

    [Fact]
    public void Extract7zArchive_SevenZipSharpSimple()
    {
        var expectedEntryCount = -1L;
        var counter = new Detail.ArchiveEntryExtractCounter();

        using(var reader = new ArchiveReader("test-data/archive.7z", counter))
        {
            expectedEntryCount = reader.Count;
            reader.ExtractAll("result-7z-7zs-simple");
        }

        Directory.Delete("result-7z-7zs-simple", recursive: true);

        Assert.Equal(expectedEntryCount, counter.Result);
    }
        
    [Fact]
    public void ExtractZipArchive_SevenZipSharpSimple()
    {
        var expectedEntryCount = -1L;
        var counter = new Detail.ArchiveEntryExtractCounter();

        using (var reader = new ArchiveReader("test-data/archive.zip", counter))
        {
            expectedEntryCount = reader.Count;
            reader.ExtractAll("result-zip-7zs-simple");
        }

        Directory.Delete("result-zip-7zs-simple", recursive: true);
 
        Assert.Equal(expectedEntryCount, counter.Result);
   }
}