extern alias SevenZipSharp;

using System.IO;
using SevenZip;
using Xunit;

namespace SevenZipTests;

public class SevenZipVsSevenZipSharp
{
    const string Data = "This is a sample text used to test the 7zip libraries.";

    [Fact]
    public void CompressWithSZAndDecompressWithSZS()
    {
        var compressed = Lzma.Compress(EncodeString(Data));
        var decompressed = SevenZipSharp.SevenZip.SevenZipExtractor.ExtractBytes(compressed);
        var result = DecodeString(decompressed);

        Assert.Equal(Data, result);
    }

    [Fact]
    public void CompressWithSZSAndDecompressWithSZ()
    {
        var compressed = SevenZipSharp.SevenZip.SevenZipCompressor.CompressBytes(EncodeString(Data));
        var decompressed = Lzma.Decompress(compressed);
        var result = DecodeString(decompressed);

        Assert.Equal(Data, result);
    }


    static byte[] EncodeString(string value) => System.Text.Encoding.UTF8.GetBytes(value);

    static string DecodeString(byte[] value) => System.Text.Encoding.UTF8.GetString(value);
}