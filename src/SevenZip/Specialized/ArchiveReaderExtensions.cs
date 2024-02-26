using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenZip.Specialized;

public static class ArchiveReaderExtensions
{
    const int MaxCapacity = InMemoryArchiveRepacker.MaxCapacity;

    public static async Task BulkExtract(
        this ArchiveReader reader,
        Func<IEnumerable<(ArchiveEntry, Stream)>, Task> consumeExtractResults,
        BulkExtractOptions extractOptions = null)
    {
        var options = extractOptions ?? new BulkExtractOptions { };
        var entries = new Dictionary<int, (ArchiveEntry, Stream)>();

        using var pool = CreateScopedStreamPool(
            options.UseNativeStreamPool, 
            reader.GetStreamPoolCapacity(options.StreamPoolCapacity));

        using var transaction = new ExtractTransaction(reader, ArchiveFlags.None);

        foreach (var entry in reader.Entries)
        {
            if (options.Predicate is not null && !options.Predicate(entry))
            {
                continue;
            }

            if (entry.IsDirectory)
            {
                entries.Add(entry.Index, (entry, Stream.Null));
                continue;
            }

            if (entry.UncompressedSize > MaxCapacity)
            {
                throw new InvalidOperationException($"The entry with '{entry.Path}' exceeds the stream capacity.");
            }
            
            var size = (int)entry.UncompressedSize;
            if (size > pool.Capacity)
            {
                entries.Add(entry.Index, (entry, new MemoryStream(size)));
                continue;
            }

            if (pool.CanCreateStreamWithCapacity(size) == false)
            {
                await ProcessPendingEntries(consumeExtractResults, entries, transaction, pool);
            }

            entries.Add(entry.Index, (entry, pool.CreateStream(size)));
        }

        await ProcessPendingEntries(consumeExtractResults, entries, transaction, pool);
    }

    private static async Task ProcessPendingEntries(
        Func<IEnumerable<(ArchiveEntry, Stream)>, Task> consumeExtractResults,
        Dictionary<int, (ArchiveEntry, Stream)> entries,
        ExtractTransaction transaction,
        IScopedStreamPool pool)
    {
        if (entries.Count == 0)
        {
            return;
        }

        transaction.Extract(entries.Keys, index => entries[index].Item2);
        foreach (var (entry, stream) in entries.Values)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        await consumeExtractResults(entries.Values);

        entries.Clear();
        pool.Discard();
    }

    private static int GetStreamPoolCapacity(this ArchiveReader reader, int capacity)
    {
        if (capacity > 0)
        {
            return Math.Min(capacity, MaxCapacity);
        }

        return reader.GetUncompressedArchiveSize() switch
        {
            > MaxCapacity => MaxCapacity,
            var value => (int)value
        };
    }

    private static ulong GetUncompressedArchiveSize(this ArchiveReader reader)
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
