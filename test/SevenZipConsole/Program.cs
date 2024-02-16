using System.IO;
using SevenZip;
using SevenZip.Extensions;
using static System.Console;


var (archivePath, targetDir) = ("/mnt/t/github.com/exocad/SevenZipSharpSimple/test/test-data/archive.7z", ".");

for (var i = 0; i < args.Length - 1; i++)
{
    if (string.Equals(args[i], "--archive", System.StringComparison.OrdinalIgnoreCase))
    {
        archivePath = args[i + 1].Trim(' ', '\\');
        continue;
    }
    
    if (string.Equals(args[i], "--target", System.StringComparison.OrdinalIgnoreCase))
    {
        targetDir = args[i + 1].Trim(' ', '\\');;
    }
}

WriteLine("SevenZipSharpSimple Test Tool");

{
    var file = @"/mnt/t/github.com/exocad/SevenZipSharpSimple/test/test-data/sample.txt";
    archivePath = @"/mnt/t/github.com/exocad/SevenZipSharpSimple/test/test-data/archive-test.7z";

    using var writer = new ArchiveWriter(ArchiveFormat.SevenZip, archivePath);

    writer.AddFile("directory/sample.txt", file);
    writer.AddDirectoryRecursive("/mnt/t/dentalshare", NamingStrategy.RelativeToTopDirectoryInclusive);
    writer.Compress(new CompressProperties()
    {
        CompressionLevel = CompressionLevel.Ultra,
    });
}


if (string.IsNullOrEmpty(archivePath))
{
    WriteLine("Usage: SevenZipConsole --archive ARCHIVEPATH --target TARGETDIR");
    return;
}

WriteLine($"Archive: {archivePath}");
WriteLine($"Target : {targetDir}");

using var reader = new ArchiveReader(archivePath, @delegate: new ArchiveReaderDelegate(
    onExtractOperation: (i, e, op, result) =>
    {
        WriteLine($"[{i:0000}] {e?.Path} | {op} -> {result}");
    }));

WriteLine($"Format : {reader.Format}");
WriteLine($"Entries: {reader.Count}");
WriteLine("");

reader.ExtractAll(targetDir);

WriteLine($"Operation completed.");

return;
