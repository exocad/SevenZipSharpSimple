using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using SevenZipSharpSimple;

namespace SevenZipBenchmarks
{
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
            precompressedData = Archive.Compress(data);
        }

        [Benchmark]
        public void CompressSevenZip()
        {
            Archive.Compress(data);
        }

        [Benchmark]
        public void CompressSevenZipSharp()
        {
            SevenZip.SevenZipCompressor.CompressBytes(data);
        }

        [Benchmark]
        public void DecompressSevenZip()
        {
            Archive.Decompress(precompressedData);
        }

        [Benchmark]
        public void DecompressSevenZipSharp()
        {
            SevenZip.SevenZipExtractor.ExtractBytes(precompressedData);
        }

        static byte[] GenerateTestData(int length)
        {
            var randomizer = RandomNumberGenerator.Create();
            var buffer = new byte[length];

            randomizer.GetNonZeroBytes(buffer);
            return buffer;
        }
    }
}