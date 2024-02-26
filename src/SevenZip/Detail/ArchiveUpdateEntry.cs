using System;
using System.IO;
using SevenZip.Interop;

namespace SevenZip.Detail;

/// <summary>
/// This class represents a new entry to add to an archive or an existing entry
/// that is about to be modified or deleted.
/// </summary>
sealed class ArchiveUpdateEntry
{
    private readonly bool _leaveOpen;
    private readonly string _path;
    private Stream _stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveEntry"/> class. This
    /// constructor is used to mark an existing entry for deletion by setting
    /// the <see cref="ArchivePath"/> property to <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="index">The index of the entry to remove.</param>
    /// <param name="isDirectory">Indicates whether the entry represents a directory.</param>
    public ArchiveUpdateEntry(int index, bool isDirectory)
    {
        Index = index;
        Attributes = isDirectory ? FileAttributes.Directory : FileAttributes.Normal;
        ArchivePath = string.Empty;

        _path = string.Empty;
        _stream = null;
        _leaveOpen = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveUpdateEntry"/> class. This
    /// constructor is used to add or modify an entry with the contents of a physical
    /// file located at <paramref name="path"/>.
    /// </summary>
    /// <param name="index">The index of the archive entry.</param>
    /// <param name="archivePath">The path the entry should have within the archive.</param>
    /// <param name="path">The path to the physical file to add.</param>
    public ArchiveUpdateEntry(int index, string archivePath, string path)
    {
        var fileInfo = new FileInfo(path);

        Index = index;
        ArchivePath = archivePath;
        Attributes = fileInfo.Attributes;

        Size = fileInfo.Length;
        LastAccessTime = fileInfo.LastAccessTime;
        LastWriteTime = fileInfo.LastWriteTime;
        CreationTime = fileInfo.CreationTime;

        _path = path;
        _stream = null;
        _leaveOpen = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveUpdateEntry"/> class. This
    /// constructor is used to add or modify an entry with the contents of a given stream.
    /// </summary>
    /// <param name="index">
    /// The index of the archive entry.
    /// </param>
    /// <param name="archivePath">
    /// The path the entry should have within the archive.
    /// </param>
    /// <param name="source">
    /// The stream to read the contents from.
    /// </param>
    /// <param name="leaveOpen">
    /// <c>true</c> to dispose the given stream once all data has
    /// been read. <c>false</c> to leave it open.
    /// </param>
    public ArchiveUpdateEntry(int index, string archivePath, Stream source, bool leaveOpen)
    {
        Index = index;
        Attributes = FileAttributes.Normal;
        ArchivePath = archivePath;

        Size = source.Length - source.Position;
        LastAccessTime =
        LastWriteTime =
        CreationTime = DateTime.UtcNow;

        _path = string.Empty;
        _stream = source;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// Gets the archive inde of this entry.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the file attributes of this entry.
    /// </summary>
    public FileAttributes Attributes { get; }

    /// <summary>
    /// Gets the uncompressed size of the content.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets the path within the archive.
    /// </summary>
    public string ArchivePath { get; }

    /// <summary>
    /// Gets a value indicating whether this entry is marked for deletion.
    /// </summary>
    public bool IsMarkedForDelete => string.IsNullOrEmpty(ArchivePath);

    /// <summary>
    /// Gets a value indicating whether the entry is a directory or not.
    /// </summary>
    public bool IsDirectory => (Attributes & FileAttributes.Directory) != 0;

    /// <summary>
    /// Gets the last write timestamp for the entry.
    /// </summary>
    public DateTime LastWriteTime { get; }

    /// <summary>
    /// Gets the creation timestamp for the entry.
    /// </summary>
    public DateTime CreationTime { get; }

    /// <summary>
    /// Gets the last access timestamp for the entry.
    /// </summary>
    public DateTime LastAccessTime { get; }

    /// <summary>
    /// Gets the extension of the entry's filename or <see cref="string.Empty"/>,
    /// if not present.
    /// </summary>
    /// <returns>The file extension or <see cref="string.Empty"/>.</returns>
    public string GetExtension()
    {
        var index = ArchivePath.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
        var isValidIndexPosition = index >= 0 && index + 1 < ArchivePath.Length;

        return isValidIndexPosition ? ArchivePath.Substring(index + 1) : string.Empty;
    }

    /// <summary>
    /// Creates an <see cref="ArchiveStream"/> which can be used to access the
    /// file data to compress.
    /// </summary>
    /// <returns>An <see cref="ArchiveStream"/> to read the content from.</returns>
    public ArchiveStream CreateStream()
    {
        if (TryReleaseStream(out var stream))
        {
            return new ArchiveStream(stream, _leaveOpen);
        }

        if (!string.IsNullOrEmpty(_path))
        {
            return new ArchiveStream(File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), _leaveOpen);
        }

        throw new InvalidOperationException($"The entry {ArchivePath} does not reference an existing stream or a file.");
    }

    private bool TryReleaseStream(out Stream stream)
    {
        stream = _stream;
        _stream = null;
        return stream != null;
    }
}