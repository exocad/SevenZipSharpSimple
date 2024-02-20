using System;
using System.IO;
using SevenZip.Interop;

namespace SevenZip.Detail;

/// <summary>
/// The <see cref="ArchiveEntryReader"/> opens an existing archive and obtains all
/// existing entries.
/// </summary>
sealed class ArchiveEntryReader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveEntryReader"/> class which
    /// opens an archive and reads its file entries. Afterwards, the archive is closed
    /// again immediately.
    /// </summary>
    /// <param name="reader">The archive reader instance.</param>
    /// <param name="stream">The stream representing the archive.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the archive cannot be opened.
    /// </exception>
    public ArchiveEntryReader(in IArchiveReader reader, in Stream stream)
    {
        using var guard = new OpenArchiveGuard(reader, stream);

        guard.EnsureOpened();

        var count = reader.GetNumberOfItems();
        var array = new ArchiveEntry[count];

        for (var index = 0u; index < count; ++index)
        {
            var entry = new ArchiveEntry()
            {
                Attributes = (FileAttributes)GetProperty(reader, index, ArchiveEntryProperty.Attributes, union => union.AsUInt32()),
                Comment = GetProperty(reader, index, ArchiveEntryProperty.Comment, union => union.AsString()),
                Crc = GetProperty(reader, index, ArchiveEntryProperty.Crc, union => union.AsUInt32()),
                CreationTime = GetProperty(reader, index, ArchiveEntryProperty.CreationTime, union => union.AsDateTime()),
                Encrypted = GetProperty(reader, index, ArchiveEntryProperty.Encrypted, union => union.AsBool()),
                Path = GetProperty(reader, index, ArchiveEntryProperty.Path, union => union.AsString()),
                Index = (int)index,
                IsDirectory = GetProperty(reader, index, ArchiveEntryProperty.IsDirectory, union => union.AsBool()),
                LastAccessTime = GetProperty(reader, index, ArchiveEntryProperty.LastAccessTime, union => union.AsDateTime()),
                LastWriteTime = GetProperty(reader, index, ArchiveEntryProperty.LastWriteTime, union => union.AsDateTime()),
                Method = GetProperty(reader, index, ArchiveEntryProperty.Method, union => union.AsString()),
                UncompressedSize = GetProperty(reader, index, ArchiveEntryProperty.Size, union => union.AsUInt64())
            };

            array[index] = entry;
        }

        Entries = array;
    }

    public ArchiveEntry[] Entries { get; }

    private static T GetProperty<T>(IArchiveReader reader, uint index, ArchiveEntryProperty property, Func<Union, T> convert)
    {
        var union = default(Union);

        reader.GetProperty(index, property, ref union);
        return convert(union);
    }
}
