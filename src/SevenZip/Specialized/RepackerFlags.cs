using System;

namespace SevenZip.Specialized;

/// <summary>
/// Flags to configure the behavior of the <see cref="InMemoryArchiveRepacker"/> class.
/// </summary>
[Flags]
public enum RepackerFlags
{
    /// <summary>
    /// Specifies the default behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// Use a stream pool that refers to native memory. This may be useful for
    /// larger archives to relieve the garbage collector.
    /// </summary>
    UseNativeMemory = 1 << 0,
}
