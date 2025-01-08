using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip;
using Xunit;

namespace SevenZipTests;

public class ArchiveWriterTests
{
    const string SampleTextFilePath = "test-file.txt";

    const string SampleBinaryFilePath = "test-file.bin";

    const string SampleTextFileContent = "This is the content of a test file.";

    static readonly byte[] SampleBinaryFileContent = [0x00, 0x01, 0x02, 0x04, 0x05, 0x06];

    [Theory]
    [InlineData(ArchiveFormat.SevenZip)]
    public void CreateArchive(ArchiveFormat format)
    {
        File.WriteAllText(SampleTextFilePath, SampleTextFileContent);

        try
        {
            using (var writer = new ArchiveWriter(format, "writer-test.archive"))
            {
                writer.AddFile("dir/file1.txt", SampleTextFilePath);
                writer.AddFile("dir/file2.bin", new MemoryStream(SampleBinaryFileContent), leaveOpen: false);
                writer.Compress();
            }

            using (var reader = new ArchiveReader("writer-test.archive"))
            {
                var entries = reader.Entries;

                Assert.Equal(2, entries.Count);

                foreach (var entry in entries)
                {
                    Assert.True(entry.UncompressedSize > 0UL);
                    Assert.True(entry.Path is "dir\\file1.txt" or "dir\\file2.bin");
                }
            }
        }
        finally
        {
            File.Delete(SampleTextFilePath);
            File.Delete("writer-test.archive");
        }

    }
}