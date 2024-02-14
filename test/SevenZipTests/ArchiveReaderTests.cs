extern alias SevenZipSharp;

using System.IO;
using SevenZip;
using Xunit;

namespace SevenZipTests;

public class ArchiveReaderTests
{
    [Fact]
    public void Extract7zArchive_SevenZipSharp()
    {
        SevenZipSharp.SevenZip.SevenZipExtractor.SetLibraryPath("./7z.dll");
            
        using (var reader = new SevenZipSharp.SevenZip.SevenZipExtractor("test-data/archive.7z"))
        {
            reader.ExtractArchive("result-7z-7zs");

            Directory.Delete("result-7z-7zs", recursive: true);
        }
    }
        
    [Fact]
    public void ExtractZipArchive_SevenZipSharp()
    {
        SevenZipSharp.SevenZip.SevenZipExtractor.SetLibraryPath("./7z.dll");
            
        using (var reader = new SevenZipSharp.SevenZip.SevenZipExtractor("test-data/archive.zip"))
        {
            reader.ExtractArchive("result-zip-7zs");

            Directory.Delete("result-zip-7zs", recursive: true);
        }
    }
        
    [Fact]
    public void Extract7zArchive_SevenZipSharpSimple()
    {
        using (var reader = new ArchiveReader("test-data/archive.7z"))
        {
            reader.ExtractAll("result-7z-7zs-simple");

            Directory.Delete("result-7z-7zs-simple", recursive: true);
        }
    }
        
    [Fact]
    public void ExtractZipArchive_SevenZipSharpSimple()
    {
        using (var reader = new ArchiveReader("test-data/archive.zip"))
        {
            reader.ExtractAll("result-zip-7zs-simple");

            Directory.Delete("result-zip-7zs-simple", recursive: true);
        }
    }
}