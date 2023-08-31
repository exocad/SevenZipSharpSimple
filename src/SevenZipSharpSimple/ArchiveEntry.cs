using System;
using System.IO;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// Representation of a single entry within an archive, which is either
    /// a file or a directory.
    /// </summary>
    public struct ArchiveEntry
    {
        /// <summary>
        /// Gets the entry index within the archive. This value must be used to explicitly
        /// extract this file or directory.
        /// </summary>
        public int Index { get; internal set; }
        
        /// <summary>
        /// Gets the relative path of the entry.
        /// </summary>
        public string Path { get; internal set; }
        
        /// <summary>
        /// Gets the last write timestamp for the entry.
        /// </summary>
        public DateTime LastWriteTime { get; internal set; }
        
        /// <summary>
        /// Gets the creation timestamp for the entry.
        /// </summary>
        public DateTime CreationTime { get; internal set; }
        
        /// <summary>
        /// Gets the last access timestamp for the entry.
        /// </summary>
        public DateTime LastAccessTime { get; internal set; }
        
        /// <summary>
        /// Gets the uncompressed size of the entry.
        /// </summary>
        public ulong UncompressedSize { get; internal set; }
        
        /// <summary>
        /// Gets the CRC of the entry. This property might be zero, depending on the archive format.
        /// </summary>
        public uint Crc { get; internal set; }
        
        /// <summary>
        /// Gets additional attributes for the entry.
        /// </summary>
        public FileAttributes Attributes { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating if the entry is a directory.
        /// </summary>
        public bool IsDirectory { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating if the content is encrypted.
        /// </summary>
        public bool Encrypted { get; internal set; }
        
        /// <summary>
        /// Gets an optional comment.
        /// </summary>
        public string Comment { get; internal set; }
        
        /// <summary>
        /// Gets the compression method.
        /// </summary>
        public string Method { get; internal set; }
    }
}
