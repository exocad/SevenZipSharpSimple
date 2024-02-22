using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SevenZip.Specialized;

/// <summary>
/// The <see cref="InMemoryArchiveRepacker"/> allows to transfer files from one archive to another
/// by storing the extracted data in memory. This requires huge amount of heap memory for larger
/// archives, but may be more efficient that storing the entire archive to disk first.
/// 
/// If there is a performance gain varies depending on the use case and should be measured for
/// each case individually.
/// </summary>
public sealed class InMemoryArchiveRepacker
{
    private readonly ArchiveReader _reader;
    private readonly ArchiveWriter _writer;
    private readonly bool _useNativeStreamPool;
    private readonly int _capacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryArchiveRepacker"/> class.
    /// </summary>
    /// <param name="reader">
    /// The archive to read data from.
    /// </param>
    /// <param name="writer">
    /// The archive to write the extracted data to. This can either be a new or an existing
    /// archive.
    /// </param>
    /// <param name="flags">
    /// Optional flags to configure the repacker. See <see cref="RepackerFlags"/> for details.
    /// </param>
    /// <param name="capacity">
    /// The maximum capacity that can be used to store the extracted data. Once this limit
    /// is reached, the yet extracted data will be written to the target archive and the
    /// memory will be reused for the next bulk of streams.
    /// </param>
    public InMemoryArchiveRepacker(ArchiveReader reader, ArchiveWriter writer, RepackerFlags flags = RepackerFlags.None, int capacity = 0)
    {
        _reader = reader;
        _writer = writer;
        _capacity = GetStreamPoolCapacity(reader, capacity);
        _useNativeStreamPool = (flags & RepackerFlags.UseNativeMemory) != 0;
    }

    /// <summary>
    /// Gets the maximum capacity for the internal memory reserved for the streams.
    /// </summary>
    /// <remarks>
    /// The limit is documented in the <a href="https://learn.microsoft.com/en-us/dotnet/api/system.array?view=net-8.0&redirectedfrom=MSDN#remarks">
    /// Array Class - Remarks</a> section.
    /// </remarks>
    public const int MaxCapacity = 0x7FFFFFC7;

    /// <summary>
    /// Starts the repacking process and copies files from the source archive to the
    /// target archive.
    /// </summary>
    /// <param name="predicate">
    /// A custom filter that can be used to determine the entries to copy.
    /// </param>
    /// <param name="getArchivePath">
    /// A custom selector to transform the archive paths.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the uncompressed size of an entry within the archive exceeds
    /// <see cref="MaxCapacity"/>.
    /// </exception>
    public void Repack(Predicate<ArchiveEntry> predicate, Func<ArchiveEntry, string> getArchivePath)
    {
        var flags = ArchiveFlags.None;
        var indices = new Dictionary<int, (string, Stream)>();

        using var pool = CreateScopedStreamPool(_useNativeStreamPool, _capacity);
        using var transaction = new ExtractTransaction(_reader, flags);

        foreach (var entry in _reader.Entries)
        {
            if (entry.IsDirectory || predicate(entry) == false)
            {
                continue;
            }

            if (entry.UncompressedSize > MaxCapacity)
            {
                throw new InvalidOperationException($"The entry with '{entry.Path}' exceeds the stream capacity.");
            }

            var path = getArchivePath(entry);
            var size = (int)entry.UncompressedSize;

            if (size > _capacity)
            {
                indices.Add(entry.Index, (path, new MemoryStream(size)));
                continue;
            }
 
            if (pool.CanCreateStreamWithCapacity(size) == false)
            {
                RepackPendingEntries(transaction, indices, pool);
            }

            indices.Add(entry.Index, (path, pool.CreateStream(size)));
        }

        RepackPendingEntries(transaction, indices, pool);
    }

    private void RepackPendingEntries(ExtractTransaction transaction, Dictionary<int, (string, Stream)> indices, IScopedStreamPool pool)
    {
        if (indices.Count == 0)
        {
            return;
        }

        transaction.Extract(indices.Keys, index => indices[index].Item2);

        foreach (var (archivePath, stream) in indices.Values)
        {
            stream.Seek(0, SeekOrigin.Begin);

            _writer.AddFile(archivePath, stream);
        }

        _writer.Compress();
        indices.Clear();
        pool.Discard();
    }

    private static int GetStreamPoolCapacity(ArchiveReader reader, int capacity)
    {
        if (capacity > 0)
        {
            return Math.Min(capacity, MaxCapacity);
        }

        return GetUncompressedArchiveSize(reader) switch
        {
            > MaxCapacity => MaxCapacity,
            var value => (int)value
        };
    }

    private static ulong GetUncompressedArchiveSize(ArchiveReader reader)
    {
        return reader.Entries.Aggregate(0UL, (current, entry) => current + entry.UncompressedSize);
    }

    private static IScopedStreamPool CreateScopedStreamPool(bool useNativeMemory, int capacity)
    {
        return useNativeMemory
            ? new Detail.NativeStreamPool(capacity)
            : new Detail.DefaultStreamPool(capacity);
    }
}