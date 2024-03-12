using System;

namespace SevenZip.Specialized;

/// <summary>
/// The <see cref="BulkExtractOptions"/> can be used to configure the behavior
/// of the <see cref="ArchiveReaderExtensions.BulkExtract"/> extension method.
/// </summary>
public sealed class BulkExtractOptions
{
    /// <summary>
    /// Gets the predicate to determine whether an <see cref="ArchiveEntry"/>
    /// shall be extracted. If set to <c>null</c>, all entries will be
    /// extracted.
    /// </summary>
    public Func<ArchiveEntry, bool> Predicate { get; init; }

    /// <summary>
    /// Gets a value indicating whether the stream pool used during extraction
    /// shall use native or managed memory.
    /// </summary>
    public bool UseNativeStreamPool { get; init; }

    /// <summary>
    /// Gets the capacity for the internal stream pool. If set to zero, the
    /// capacity will be determined automatically based on the uncompressed
    /// archive size.
    /// </summary>
    public int StreamPoolCapacity { get; init; }
}
