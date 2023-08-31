using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip;
using SevenZipSharpSimple;
using Xunit;

namespace SevenZipTests
{
    public class ArchiveReaderTests
    {
        [Fact]
        public void Extract7zArchive_SevenZipSharp()
        {
            SevenZipExtractor.SetLibraryPath("./7z.dll");
            
            using (var reader = new SevenZipExtractor("test-data/archive.7z"))
            {
                reader.ExtractArchive("result-7z-7zs");

                Directory.Delete("result-7z-7zs", recursive: true);
            }
        }
        
        [Fact]
        public void ExtractZipArchive_SevenZipSharp()
        {
            SevenZipExtractor.SetLibraryPath("./7z.dll");
            
            using (var reader = new SevenZipExtractor("test-data/archive.zip"))
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
                var result = reader.ExtractAll("result-7z-7zs-simple");

                Directory.Delete("result-7z-7zs-simple", recursive: true);
                if (result != SevenZipSharpSimple.OperationResult.Ok)
                {
                    throw new System.Exception("Failed to extract archive.");
                }
            }
        }
        
        [Fact]
        public void ExtractZipArchive_SevenZipSharpSimple()
        {
            using (var reader = new ArchiveReader("test-data/archive.zip"))
            {
                var result = reader.ExtractAll("result-zip-7zs-simple");

                Directory.Delete("result-zip-7zs-simple", recursive: true);
                if (result != SevenZipSharpSimple.OperationResult.Ok)
                {
                    throw new System.Exception("Failed to extract archive.");
                }
            }
        }
    }
}
