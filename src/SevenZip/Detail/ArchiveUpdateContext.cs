using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SevenZip.Detail;

/// <summary>
/// The <see cref="ArchiveUpdateContext"/> manages any changes of an archive including
/// new, modified and deleted files.
/// </summary>
sealed class ArchiveUpdateContext
{
    private readonly Dictionary<int, ArchiveUpdateEntry> _entries = [];
    private readonly List<ArchiveEntry> _existingArchiveEntries = [];
    private int _cursor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveUpdateContext"/> class.
    /// </summary>
    /// <param name="existingArchiveEntries">
    /// An array containing the entries that already exist in the target archive. This is
    /// required to manage the entry indices and track if a file is new or modified.
    /// </param>
    public ArchiveUpdateContext(in ArchiveEntry[] existingArchiveEntries) => Reset(existingArchiveEntries);

    /// <summary>
    /// Gets the total number of entries.
    /// </summary>
    public int TotalEntryCount => _cursor;

    /// <summary>
    /// Gets a value indicating whether the original archive was empty.
    /// </summary>
    public bool IsEmptyArchive => _existingArchiveEntries.Count == 0;

    /// <summary>
    /// Gets a collection containing the already existing entries.
    /// </summary>
    public IReadOnlyList<ArchiveEntry> ArchiveEntries => _existingArchiveEntries;

    /// <summary>
    /// Gets a value indicating whether the file entry at the given <paramref name="index"/> refers
    /// to a new or an existing entry.
    /// </summary>
    /// <param name="index">
    /// The entry index.
    /// </param>
    /// <returns>
    /// <c>true</c>, if the entry already exists. Otherwise, <c>false</c> is being returned.
    /// </returns>
    public bool IsNewFile(int index) => _entries.ContainsKey(index) && index >= _existingArchiveEntries.Count;

    /// <summary>
    /// Gets a value indicating whether the file entry at the given <paramref name="index"/> refers
    /// to a new or an existing entry.
    /// </summary>
    /// <param name="index">
    /// The entry index.
    /// </param>
    /// <returns>
    /// <c>true</c>, if the entry already exists. Otherwise, <c>false</c> is being returned.
    /// </returns>
    public bool IsNewFile(uint index) => IsNewFile((int)index);

    /// <summary>
    /// Gets the <see cref="ArchiveUpdateEntry"/> at the given index, if it exists.
    /// </summary>
    /// <param name="index">
    /// The index of the entry to obtain.
    /// </param>
    /// <param name="entry">
    /// The corresponding entry or <c>null</c>, if the entry
    /// does not exist.
    /// </param>
    /// <returns>
    /// <c>true</c>, if the entry could be obtained. Otherwise, <c>false</c> is being returned.
    /// </returns>
    public bool TryGetArchiveUpdateEntry(int index, out ArchiveUpdateEntry entry) =>
        _entries.TryGetValue(index, out entry);

    /// <summary>
    /// Gets the <see cref="ArchiveUpdateEntry"/> at the given index, if it exists.
    /// </summary>
    /// <param name="index">
    /// The index of the entry to obtain.
    /// </param>
    /// <param name="entry">
    /// The corresponding entry or <c>null</c>, if the entry
    /// does not exist.
    /// </param>
    /// <returns>
    /// <c>true</c>, if the entry could be obtained. Otherwise, <c>false</c> is being returned.
    /// </returns>
    public bool TryGetArchiveUpdateEntry(uint index, out ArchiveUpdateEntry entry) =>
        TryGetArchiveUpdateEntry((int)index, out entry);

    /// <summary>
    /// Resets the context to an initial state with the given entries.
    /// </summary>
    /// <param name="existingArchiveEntries">
    /// An array containing the entries that already exist in the associated archive.
    /// </param>
    public void Reset(in ArchiveEntry[] existingArchiveEntries)
    {
        _existingArchiveEntries.Clear();
        _existingArchiveEntries.Capacity = existingArchiveEntries.Length;
        _existingArchiveEntries.AddRange(existingArchiveEntries);
        _entries.Clear();
        _cursor = _existingArchiveEntries.Count;
    }

    /// <summary>
    /// Enumerates all new entries that refer to an existing entry that
    /// requires modification or is marked for deletion.
    /// </summary>
    /// <returns>
    /// A lazy collection containing all entries representing a modification
    /// or deletion.
    /// </returns>
    public IEnumerable<ArchiveUpdateEntry> EnumerateModifiedEntries()
    {
        foreach (var entry in _existingArchiveEntries)
        {
            if (_entries.TryGetValue(entry.Index, out var updateEntry))
            {
                yield return updateEntry;
            }
        }
    }

    /// <summary>
    /// Adds a new entry that refers to an existing file.
    /// </summary>
    /// <param name="archivePath">
    /// The name within the archive.
    /// </param>
    /// <param name="path">
    /// The path to the file to add.
    /// </param>
    /// <returns>
    /// The temporary index for the new file.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the arguments is <c>null</c>.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown if <paramref name="path"/> does not refer to an existing file.
    /// </exception>
    public int Add(string archivePath, string path)
    {
        EnsureArgumentNotNull(nameof(archivePath), archivePath);
        EnsureArgumentNotNull(nameof(path), path);
        EnsureFileExists(path);

        _entries[_cursor] = new ArchiveUpdateEntry(_cursor, archivePath, path);
        return _cursor++;
    }

    /// <summary>
    /// Adds a new entry that refers to a stream.
    /// </summary>
    /// <param name="archivePath">
    /// The name within the archive.
    /// </param>
    /// <param name="stream">
    /// The stream to read the contents from.
    /// </param>
    /// <param name="leaveOpen">
    /// <c>true</c> to leave the <paramref name="stream"/> open after use,
    /// <c>false</c> to dispose it.
    /// </param>
    /// <returns>
    /// The temporary index for the new file.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the arguments is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the given <paramref name="stream"/> does not support read operations.
    /// </exception>
    public int Add(string archivePath, Stream stream, bool leaveOpen)
    {
        EnsureArgumentNotNull(nameof(archivePath), archivePath);
        EnsureArgumentNotNull(nameof(stream), stream);

        if (stream.CanRead == false)
        {
            throw new ArgumentException("The given stream must support read operations.");
        }

        _entries[_cursor] = new ArchiveUpdateEntry(_cursor, archivePath, stream, leaveOpen);
        return _cursor++;
    }

    /// <summary>
    /// Marks an existing entry for deletion.
    /// </summary>
    /// <param name="index">
    /// The index of the entry to delete.
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the given <paramref name="index"/> does not refer to an existing
    /// entry.
    /// </exception>
    public void Delete(int index)
    {
        EnsureExistingArchiveEntryIndex(index);

        _entries[index] = new ArchiveUpdateEntry(index, _existingArchiveEntries[index].IsDirectory);
    }

    /// <summary>
    /// Replaces an existing entry with the contents of another file.
    /// </summary>
    /// <param name="index">
    /// The index of the entry to modify.
    /// </param>
    /// <param name="path">
    /// The path to the file to read the new contents from.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the arguments is <c>null</c>.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown if <paramref name="path"/> does not refer to an existing file.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the given <paramref name="index"/> does not refer to an existing
    /// entry.
    /// </exception>
    public void Replace(int index, string path)
    {
        EnsureExistingArchiveEntryIndex(index);
        EnsureArgumentNotNull(nameof(path), path);
        EnsureFileExists(path);

        var archivePath = _existingArchiveEntries[index].Path;

        _entries[index] = new ArchiveUpdateEntry(index, archivePath, path);
    }

    /// <summary>
    /// Replaces an existing entry with the contents of the given stream.
    /// </summary>
    /// <param name="index">
    /// The index of the entry to modify.
    /// </param>
    /// <param name="stream">
    /// The stream to read the contents from.
    /// </param>
    /// <param name="leaveOpen">
    /// <c>true</c> to leave the <paramref name="stream"/> open after use,
    /// <c>false</c> to dispose it.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the arguments is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the given <paramref name="stream"/> does not support read operations.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the given <paramref name="index"/> does not refer to an existing
    /// entry.
    /// </exception>
    public void Replace(int index, Stream stream, bool leaveOpen)
    {
        EnsureExistingArchiveEntryIndex(index);
        EnsureArgumentNotNull(nameof(stream), stream);

        if (stream.CanRead == false)
        {
            throw new ArgumentException("The given stream must support read operations.");
        }

        var archivePath = _existingArchiveEntries[index].Path;

        _entries[index] = new ArchiveUpdateEntry(index, archivePath, stream, leaveOpen);
    }

    private void EnsureExistingArchiveEntryIndex(int index)
    {
        if (_existingArchiveEntries.Any(entry => entry.Index == index))
        {
            return;
        }

        throw new IndexOutOfRangeException($"The index {index} does not refer to an existing archive entry.");
    }

    private static void EnsureFileExists(string path)
    {
            if (File.Exists(path))
            {
                return;
            }

            throw new FileNotFoundException($"The file '{path}' does not exist.");
        }

    private static void EnsureArgumentNotNull<T>(string name, T arg) where T : class
    {
        if (arg != null)
        {
            return;
        }

        throw new ArgumentNullException(name);
    }
}