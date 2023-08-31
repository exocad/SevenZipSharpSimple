using System;

namespace SevenZipSharpSimple
{
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
        /// Dispose any output stream that was created during an extract operation.
        /// </summary>
        CleanupOutputStreams = 1,
    }
}
