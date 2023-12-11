using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using SevenZipSharpSimple.Detail;
using SevenZipSharpSimple.Interop;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// The <see cref="ArchiveReader"/> allows browsing and extracting the content of an archive.
    /// It makes use of the native <c>7z</c> library, which is loaded when creating an instance
    /// of this class.
    /// </summary>
    public sealed class ArchiveReader : IDisposable
    {
        private readonly IArchiveReaderDelegate _delegate;
        private readonly IntPtr _libraryHandle;
        private readonly ArchiveFormat _format;
        private readonly IArchiveReader _archive;
        private readonly Stream _stream;
        private readonly int _offset;
        private readonly bool _disposeStreamOnDispose;
        private readonly ArchiveEntry[] _entries;
        private bool _disposed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveReader"/> class which opens the given
        /// <paramref name="path"/> to read the archive content.
        /// </summary>
        /// <param name="path">The path to the archive to load.</param>
        /// <param name="delegate">An optional implementation of the <see cref="IArchiveReaderDelegate"/>
        /// interface which is used to report the progress or errors during an extract operation.</param>
        /// <param name="config">Optional configuration parameters for this <see cref="ArchiveReader"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">Thrown if the archive-file could not be opened.</exception>
        /// <exception cref="NotSupportedException">Thrown if the archive format could not be determined.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the native library could not be loaded.</exception>
        /// <exception cref="DllNotFoundException">Thrown if the native library could not be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the archive open operation failed.</exception>
        public ArchiveReader(string path, IArchiveReaderDelegate @delegate = null, ArchiveConfig config = null)
            : this(File.OpenRead(path), leaveOpen: false, @delegate, config)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveReader"/> class which uses the given
        /// <paramref name="stream"/> to read the archive content.
        /// </summary>
        /// <param name="stream">The stream to read the archive data from.</param>
        /// <param name="leaveOpen">If set to <c>true</c>, the stream will not be disposed when this
        /// instance is being closed. Otherwise, the stream's <c>Dispose</c> method will be called.</param>
        /// <param name="delegate">An optional implementation of the <see cref="IArchiveReaderDelegate"/>
        /// interface which is used to report the progress or errors during an extract operation.</param>
        /// <param name="config">Optional configuration parameters for this <see cref="ArchiveReader"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">Thrown if the archive-file could not be opened.</exception>
        /// <exception cref="NotSupportedException">Thrown if the archive format could not be determined.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the native library could not be loaded.</exception>
        /// <exception cref="DllNotFoundException">Thrown if the native library could not be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the archive open operation failed.</exception>
        public ArchiveReader(Stream stream, bool leaveOpen, IArchiveReaderDelegate @delegate = null, ArchiveConfig config = null)
        {
            Config = ArchiveConfig.CloneOrDefault(config);

            _delegate = @delegate ?? ArchiveReaderDelegate.Default;
            _format = Detail.Format.Detect(stream, out _offset, out var isExecutable);
            _disposeStreamOnDispose = !leaveOpen;
            _libraryHandle = Native.LoadLibrary(Config.NativeLibraryPath);
            _archive = CreateArchiveReader();
            _stream = stream;
            _stream.Seek(0, SeekOrigin.Begin);

            if (_libraryHandle == IntPtr.Zero)
            {
                if (File.Exists(Config.NativeLibraryPath))
                {
                    throw new BadImageFormatException("The library could not be loaded.");
                }
                else
                {
                    throw new DllNotFoundException("The library could not be found.");
                }
            }
            
            using (var guard = new OpenArchiveGuard(_archive, _stream))
            {
                guard.EnsureOpened();
                Count = _archive.GetNumberOfItems();

                _entries = new ArchiveEntry[Count];

                for (var index = 0u; index < Count; ++index)
                {
                    var entry = new ArchiveEntry()
                    {
                        Attributes = (FileAttributes)GetProperty(index, ArchiveEntryProperty.Attributes, union => union.AsUInt32()),
                        Comment = GetProperty(index, ArchiveEntryProperty.Comment, union => union.AsString()),
                        Crc = GetProperty(index, ArchiveEntryProperty.Crc, union => union.AsUInt32()),
                        CreationTime = GetProperty(index, ArchiveEntryProperty.CreationTime, union => union.AsDateTime()),
                        Encrypted = GetProperty(index, ArchiveEntryProperty.Encrypted, union => union.AsBool()),
                        Path = GetProperty(index, ArchiveEntryProperty.Path, union => union.AsString()),
                        Index = (int)index,
                        IsDirectory = GetProperty(index, ArchiveEntryProperty.IsDirectory, union => union.AsBool()),
                        LastAccessTime = GetProperty(index, ArchiveEntryProperty.LastAccessTime, union => union.AsDateTime()),
                        LastWriteTime = GetProperty(index, ArchiveEntryProperty.LastWriteTime, union => union.AsDateTime()),
                        Method = GetProperty(index, ArchiveEntryProperty.Method, union => union.AsString()),
                        UncompressedSize = GetProperty(index, ArchiveEntryProperty.Size, union => union.AsUInt64())
                    };
                    
                    _entries[index] = entry;
                }
            }
        }

        /// <summary>
        /// Gets the number of entries this archive has.
        /// </summary>
        public long Count { get; }

        /// <summary>
        /// Gets the archive format.
        /// </summary>
        public ArchiveFormat Format => _format;
        
        /// <summary>
        /// Gets a list of all entries in this archive.
        /// </summary>
        public IReadOnlyList<ArchiveEntry> Entries => _entries;

        /// <summary>
        /// Performs a kind of dry-run for all entries to test if they can be extracted.
        /// </summary>
        /// <returns><c>true</c> if all entries can be extracted.</returns>
        public bool CanExtractEntries()
        {
            using (var guard = new OpenArchiveGuard(_archive, _stream))
            {
                guard.EnsureOpened();

                using (var context = new ExtractContext())
                {
                    return 0 == _archive.Extract(null, uint.MaxValue, 1, context);
                }
            }
        }

        /// <summary>
        /// Extracts a single entry to the given <paramref name="output"/> stream.
        /// </summary>
        /// <param name="index">The index of the <see cref="ArchiveEntry"/> to extract.</param>
        /// <param name="output">The stream to extract the entry to.</param>
        /// <param name="flags">Additional flags to configure the extraction behavior. See
        /// <see cref="ArchiveFlags"/> for details.</param>
        /// <returns>An enumerated value indicating the result of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="output"/> is
        /// <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="index"/> is
        /// out of range or if <paramref name="output"/> is not writeable.</exception>
        public OperationResult Extract(int index, Stream output, ArchiveFlags flags = ArchiveFlags.None)
        {
            EnsureArgumentNotNull(output, nameof(output));

            if (output.CanWrite is false)
            {
                throw new ArgumentException("The given output stream must be writeable.");
            }

            Stream OnGetStream(ArchiveEntry e) => e.Index == index ? output : null;

            return Extract(Enumerable.Repeat(index, 1), OnGetStream, flags);
        }

        /// <summary>
        /// Extracts the given <paramref name="indices"/> to an existing stream which is being returned
        /// by the <paramref name="onGetStream"/> callback.
        /// </summary>
        /// <param name="indices">A collection of entry indices.</param>
        /// <param name="onGetStream">A callback which is invoked to obtain the output stream for
        /// an archive entry. The stream is then used to write the uncompressed content.</param>
        /// <param name="flags">Additional flags to configure the extraction behavior. See
        /// <see cref="ArchiveFlags"/> for details.</param>
        /// <returns>An enumerated value indicating the result of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="indices"/> or
        /// <paramref name="onGetStream"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if any index is out of range.</exception>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public OperationResult Extract(IEnumerable<int> indices, Func<ArchiveEntry, Stream> onGetStream, ArchiveFlags flags = ArchiveFlags.None)
        {
            EnsureArgumentNotNull(indices, nameof(indices));
            
            return Extract(indices.Select(i => (uint)i).ToArray(), onGetStream, flags);
        }

        /// <summary>
        /// Extracts all entries to an existing stream which is being returned by the <paramref name="onGetStream"/>
        /// callback.
        /// </summary>
        /// <param name="onGetStream">A callback which is invoked to obtain the output stream for
        /// an archive entry. The stream is then used to write the uncompressed content.</param>
        /// <param name="flags">Additional flags to configure the extraction behavior. See
        /// <see cref="ArchiveFlags"/> for details.</param>
        /// <returns>An enumerated value indicating the result of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if  <paramref name="onGetStream"/> is
        /// <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if any index is out of range.</exception>
        public OperationResult ExtractAll(Func<ArchiveEntry, Stream> onGetStream, ArchiveFlags flags = ArchiveFlags.None)
        {
            return Extract(default(uint[]), onGetStream, flags);
        }
        
        /// <summary>
        /// Extracts all entries to the given <paramref name="targetDir"/>.
        /// </summary>
        /// <param name="targetDir">The directory to create the files in.</param>
        /// <param name="flags">Additional flags to configure the extraction behavior. See
        /// <see cref="ArchiveFlags"/> for details.</param>
        /// <returns>An enumerated value indicating the result of the operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="targetDir"/> is <c>null</c>
        /// or empty.</exception>
        /// <exception cref="IOException">Thrown if the given <paramref name="targetDir"/> cannot
        /// be created in case it does not yet exist.</exception>
        public OperationResult ExtractAll(string targetDir, ArchiveFlags flags = ArchiveFlags.None)
        {
            if (string.IsNullOrEmpty(targetDir))
            {
                throw new ArgumentException("The given target directory must not be null or empty.");
            }
            
            if (Directory.Exists(targetDir) is false)
            {
                Directory.CreateDirectory(targetDir);
            }
            
            Stream OnGetStream(ArchiveEntry entry)
            {
                // In case any of these operations fail they will be caught within the `IArchiveExtractCallback`
                
                var path = Path.Combine(targetDir, entry.Path);
                var directory = entry.IsDirectory ? path : Path.GetDirectoryName(path);
                
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (entry.IsDirectory)
                {
                    return null;
                }
                else
                {
                    return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                }
            }

            return Extract(default(uint[]), OnGetStream, flags | ArchiveFlags.CleanupOutputStreams);
        }

        /// <summary>
        /// Closes the archive and its associated resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_disposeStreamOnDispose)
            {
                _stream.Dispose();
            }
        }

        /// <summary>
        /// Gets the archive configuration.
        /// </summary>
        internal ArchiveConfig Config { get; }

        private void EnsureEntryIndicesAreValid(uint[] indices)
        {
            if (indices == null)
            {
                return;
            }
            
            if (indices.Any(index => index >= Count))
            {
                throw new ArgumentException("One or more of the given indices are out of range.");
            }
        }
        
        private void EnsureArgumentNotNull<T>(T argument, string name) where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }
        
        private OperationResult Extract(uint[] indices, Func<ArchiveEntry, Stream> onGetStream, ArchiveFlags flags = ArchiveFlags.None)
        {
            EnsureArgumentNotNull(onGetStream, nameof(onGetStream));
            EnsureEntryIndicesAreValid(indices);
            
            using (var guard = new OpenArchiveGuard(_archive, _stream))
            {
                guard.EnsureOpened();
                using (var context = new ExtractContext(this, _delegate, flags, onGetStream))
                {
                    var length = indices?.Length != null ? (uint)indices.Length : uint.MaxValue;
                    var result = _archive.Extract(indices, length, 0, context);

                    return (OperationResult)result;
                }
            }
        }

        private unsafe IArchiveReader CreateArchiveReader()
        {
            var createObject = Native.GetExportAs<Native.CreateObjectDelegate>(_libraryHandle, "CreateObject");
        
            var interfaceId = typeof(IArchiveReader).GUID;
            var classId = FormatHandler.KnownHandlers[_format];

            createObject(ref classId, ref interfaceId, out var @interface);

#if NET8_0_OR_GREATER
            return System.Runtime.InteropServices.Marshalling.ComInterfaceMarshaller<IArchiveReader>.ConvertToManaged(@interface.ToPointer());
#else
            return @interface as IArchiveReader;
#endif
        }
        
        private T GetProperty<T>(uint index, ArchiveEntryProperty property, Func<Union, T> convert)
        {
            var union = default(Union);
            
            _archive.GetProperty(index, property, ref union);
            return convert(union);
        }
    }
}
