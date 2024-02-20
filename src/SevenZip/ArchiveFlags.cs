using System;

namespace SevenZip;

/// <summary>
/// Flags used to configure the behavior of <see cref="ArchiveReader"/> operations.
/// </summary>
[Flags]
public enum ArchiveFlags
{
    /// <summary>
    /// No flags at all, use the default behavior.
    /// </summary>
    None = 0,
        
    /// <summary>
    /// Dispose any stream that was created during an extract operation.
    /// </summary>
    CloseArchiveEntryStreamAfterExtraction = 1 << 0,

    /// <summary>
    /// Apply the original timestamps stored in the archive to extracted
    /// files. This flag requires that <see cref="CloseArchiveEntryStreamAfterExtraction"/>
    /// is set and that the associated stream is a <see cref="System.IO.FileStream"/>.
    /// </summary>
    ApplyArchiveEntryTimestampsToFileStreams = 1 << 1,
}
