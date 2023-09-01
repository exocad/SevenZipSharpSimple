using System.IO;
using SevenZipSharpSimple;
using Xunit;

namespace SevenZipTests
{
    public class SevenZipVsSevenZipSharp
    {
        const string Data = "This is a sample text used to test the 7zip libraries.";

        [Fact]
        public void CompressWithSZAndDecompressWithSZS()
        {
            var compressed = Archive.Compress(EncodeString(Data));
            var decompressed = SevenZip.SevenZipExtractor.ExtractBytes(compressed);
            var result = DecodeString(decompressed);

            Assert.Equal(Data, result);
        }

        [Fact]
        public void CompressWithSZSAndDecompressWithSZ()
        {
            var compressed = SevenZip.SevenZipCompressor.CompressBytes(EncodeString(Data));
            var decompressed = Archive.Decompress(compressed);
            var result = DecodeString(decompressed);

            Assert.Equal(Data, result);
        }


        static byte[] EncodeString(string value) => System.Text.Encoding.UTF8.GetBytes(value);

        static string DecodeString(byte[] value) => System.Text.Encoding.UTF8.GetString(value);
    }
}