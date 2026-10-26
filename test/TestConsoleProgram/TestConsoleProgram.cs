using System;
using System.IO;
using SevenZip;
using SevenZip.Extensions;

// A small console application which runs the main scenarios of the SevenZip
// library against the bundled native 7z binary and prints the results.
//
// It is intended as a quick manual verification tool, e.g. after updating the
// native library or when checking the library on a new platform. It is not
// part of the automated test suite. Run it with:
//
//     dotnet run --project test/TestConsoleProgram
//
// All temporary files are created next to the executable.

var failures = 0;

Check("create and extract a 7z archive", () =>
{
    var content = "This is the content of a test file.";

    File.WriteAllText(P("src-file.txt"), content);

    try
    {
        using (var writer = new ArchiveWriter(ArchiveFormat.SevenZip, P("out.7z")))
        {
            writer.AddFile("dir/file1.txt", P("src-file.txt"));
            writer.Compress();
        }

        using var reader = new ArchiveReader(P("out.7z"));

        if (reader.Entries.Count != 1)
        {
            throw new InvalidOperationException($"Expected 1 entry, got {reader.Entries.Count}.");
        }

        reader.ExtractAll(P("out-extract"));

        var extracted = File.ReadAllText(Path.Combine(P("out-extract"), "dir", "file1.txt"));

        if (extracted != content)
        {
            throw new InvalidOperationException("The extracted content does not match the original content.");
        }
    }
    finally
    {
        Cleanup(P("src-file.txt"), P("out.7z"));
        DeleteDirectory(P("out-extract"));
    }
});

Check("create an AES-256 encrypted zip (LZMA is ignored, as zip does not support it)", () =>
{
    File.WriteAllText(P("src-file.txt"), "This is the content of a test file.");

    try
    {
        using (var writer = new ArchiveWriter(ArchiveFormat.Zip, P("out-encrypted.zip"), config: new ArchiveConfig(password: "test")))
        {
            writer.AddFile("dir/file1.txt", P("src-file.txt"));
            writer.Compress(new CompressProperties()
            {
                CompressionLevel = CompressionLevel.Ultra,
                EncryptionMethod = EncryptionMethod.Aes256,
                CompressionMethod = CompressionMethod.Lzma,
            });
        }

        using var reader = new ArchiveReader(P("out-encrypted.zip"), config: new ArchiveConfig(password: "test"));

        if (reader.Entries.Count != 1)
        {
            throw new InvalidOperationException($"Expected 1 entry, got {reader.Entries.Count}.");
        }

        foreach (var entry in reader.Entries)
        {
            if (entry.Method.Contains("AES-256") is false)
            {
                throw new InvalidOperationException($"Expected the AES-256 encryption method, got '{entry.Method}'.");
            }
        }
    }
    finally
    {
        Cleanup(P("src-file.txt"), P("out-encrypted.zip"));
    }
});

Check("extracting an encrypted archive with the wrong password throws an ArchiveOperationException", () =>
{
    using var reader = new ArchiveReader(P("test-data", "archive-encrypted.zip"), config: new ArchiveConfig("wrong", ignoreOperationErrors: false));

    try
    {
        reader.ExtractAll(P("out-wrongpw-extract"));
        throw new InvalidOperationException("Expected an ArchiveOperationException to be thrown.");
    }
    catch (ArchiveOperationException ex)
    {
        Console.WriteLine($"          (as expected: {ex.Message})");
    }
    finally
    {
        DeleteDirectory(P("out-wrongpw-extract"));
    }
});

Check("extracting an archive entry with an absolute path is rejected (CVE-2024-48510)", () =>
{
    // The notation of an absolute path is platform specific, so the entry is
    // created with a path which is absolute on the current platform.
    var insecureEntry = OperatingSystem.IsWindows() ? @"C:\Windows\Temp\foo" : "/Windows/Temp/foo";

    using (var writer = new ArchiveWriter(ArchiveFormat.Zip, P("out-traversal.zip")))
    {
        writer.AddFile(insecureEntry, new MemoryStream([1, 2, 3]), leaveOpen: false);
        writer.Compress();
    }

    try
    {
        using var reader = new ArchiveReader(P("out-traversal.zip"), new RethrowingDelegate());

        try
        {
            reader.ExtractAll(P("out-traversal-extract"));
            throw new InvalidOperationException("Expected an IOException to be thrown.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"          (as expected: {ex.Message})");
        }
    }
    finally
    {
        Cleanup(P("out-traversal.zip"));
        DeleteDirectory(P("out-traversal-extract"));
    }
});

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) FAILED.");
return failures == 0 ? 0 : 1;

void Check(string name, Action check)
{
    Console.WriteLine($"- {name}");

    try
    {
        check();
        Console.WriteLine("  OK");
    }
    catch (Exception ex)
    {
        ++failures;
        Console.WriteLine($"  FAILED: {ex.GetType().Name}: {ex.Message}");
    }
}

static string P(params string[] parts) => Path.Combine([AppContext.BaseDirectory, ..parts]);

static void Cleanup(params string[] files)
{
    foreach (var file in files)
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
}

static void DeleteDirectory(string directory)
{
    if (Directory.Exists(directory))
    {
        Directory.Delete(directory, recursive: true);
    }
}

sealed class RethrowingDelegate : IArchiveReaderDelegate
{
    public void OnGetStreamFailed(IExtractContext context, int index, ArchiveEntry? entry, Exception ex) => throw ex;

    public void OnExtractOperation(IExtractContext context, int index, ArchiveEntry? entry, ExtractOperation operation, OperationResult result)
    {
    }

    public void OnProgress(IExtractContext context, ulong current, ulong total)
    {
    }

    public void OnProgressBegin(IExtractContext context)
    {
    }

    public void OnProgressEnd(IExtractContext context)
    {
    }
}
