using SevenZip;
using static System.Console;

var (archivePath, targetDir) = (string.Empty, string.Empty);

for (var i = 0; i < args.Length - 1; i++)
{
    if (string.Equals(args[i], "--archive", System.StringComparison.OrdinalIgnoreCase))
    {
        archivePath = args[i + 1].Trim(' ', '\\');
        continue;
    }
    
    if (string.Equals(args[i], "--target", System.StringComparison.OrdinalIgnoreCase))
    {
        targetDir = args[i + 1].Trim(' ', '\\');
    }
}

WriteLine("SevenZipSharpSimple Test Tool");


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

reader.ExtractAll(targetDir, ArchiveFlags.ApplyArchiveEntryTimestampsToFileStreams);

WriteLine($"Operation completed.");
