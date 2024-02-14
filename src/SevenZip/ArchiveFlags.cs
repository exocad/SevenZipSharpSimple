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
    DisposeEntryStreams = 1,
}
