extern alias SevenZipSharp;

using System.IO;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using SevenZip;

namespace SevenZipBenchmarks;

public class Program
{
    public static void Main(string[] args) => BenchmarkRunner.Run<CompressionAndDecompressionPerformance>();
}

[SimpleJob(RuntimeMoniker.Net48)]
public class CompressionAndDecompressionPerformance
{
    byte[] data;
    byte[] precompressedData;

    [Params(256, 2048)]
    public int DataLength;

    [GlobalSetup]
    public void Setup()
    {
        data = GenerateTestData(DataLength);
        precompressedData = Lzma.Compress(data);
    }

    [Benchmark]
    public void CompressData_SevenZipSharpSimple()
    {
        Lzma.Compress(data);
    }

    [Benchmark]
    public void CompressData_SevenZipSharp()
    {
        SevenZipSharp.SevenZip.SevenZipCompressor.CompressBytes(data);
    }

    [Benchmark]
    public void DecompressData_SevenZipSharpSimple()
    {
        Lzma.Decompress(precompressedData);
    }

    [Benchmark]
    public void DecompressData_SevenZipSharp()
    {
        SevenZipSharp.SevenZip.SevenZipExtractor.ExtractBytes(precompressedData);
    }
        
    [Benchmark]
    public void Extract7zArchive_SevenZipSharpSimple()
    {
        using (var reader = new ArchiveReader("test-data/archive.7z"))
        {
            reader.ExtractAll("result-7z-7zs-simple");

            Directory.Delete("result-7z-7zs-simple", recursive: true);
        }
    }
        
    [Benchmark]
    public void Extract7zArchive_SevenZipSharp()
    {
        using (var reader = new SevenZipSharp.SevenZip.SevenZipExtractor("test-data/archive.7z"))
        {
            reader.ExtractArchive("result-7z-7zs");

            Directory.Delete("result-7z-7zs", recursive: true);
        }
    }
        
    [Benchmark]
    public void ExtractZipArchive_SevenZipSharpSimple()
    {
        using (var reader = new ArchiveReader("test-data/archive.zip"))
        {
            reader.ExtractAll("result-zip-7zs-simple");

            Directory.Delete("result-zip-7zs-simple", recursive: true);
        }
    }
        
    [Benchmark]
    public void ExtractZipArchive_SevenZipSharp()
    {
        using (var reader = new SevenZipSharp.SevenZip.SevenZipExtractor("test-data/archive.zip"))
        {
            reader.ExtractArchive("result-zip-7zs");

            Directory.Delete("result-zip-7zs", recursive: true);
        }
    }

    static byte[] GenerateTestData(int length)
    {
        var randomizer = RandomNumberGenerator.Create();
        var buffer = new byte[length];

        randomizer.GetNonZeroBytes(buffer);
        return buffer;
    }
}