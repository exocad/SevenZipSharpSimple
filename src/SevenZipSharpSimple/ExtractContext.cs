using System;
using System.IO;
using System.Runtime.InteropServices;
using SevenZipSharpSimple.Interop;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// Default implementation of the <see cref="IArchiveExtractCallback"/> which is required to obtain
    /// output streams to write extracted content to and to receive status notifications while a decompression
    /// is in progress.
    /// </summary>
    internal sealed class ExtractContext : MarshalByRefObject, IArchiveExtractCallback, IPasswordProvider, IDisposable
    {
        private readonly CurrentEntry _currentEntry = new CurrentEntry();
        private readonly IArchiveReaderDelegate _delegate;
        private readonly Func<ArchiveEntry, Stream> _onGetStream;
        private readonly ArchiveReader _archive;
        private readonly ArchiveFlags _flags;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractContext"/> class.
        /// </summary>
        /// <param name="archive">The archive to work with.</param>
        /// <param name="delegate">The delegate to forward status and error notifications to.</param>
        /// <param name="flags">Additional flags to configure the extract operation behavior. See <see cref="ArchiveFlags"/>
        /// for details.</param>
        /// <param name="onGetStream">The callback to invoke when an output stream for an entry is needed by the
        /// decompressor.</param>
        public ExtractContext(ArchiveReader archive, IArchiveReaderDelegate @delegate, ArchiveFlags flags, Func<ArchiveEntry, Stream> onGetStream)
        {
            _flags = flags;
            _archive = archive;
            _delegate = @delegate;
            _onGetStream = onGetStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractContext"/> class. This constructor must only
        /// be used for the extraction test mode.
        /// </summary>
        internal ExtractContext() => _onGetStream = _ => null;

        /// <summary>
        /// Gets the current progress of an extract operation.
        /// </summary>
        public ulong Total { get; private set; }

        /// <summary>
        /// Closes this context and resets its state.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            _disposed = true;
            _currentEntry.Reset(ExtractOperationResult.Ok, null);
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The ExtractContext object has already been disposed.");
            }
        }

        private IOutputStream CreateOutputStream(Stream streamBase)
        {
            var leaveBaseStreamOpen = (_flags & ArchiveFlags.CleanupOutputStreams) == 0;
            var output = new ArchiveStream(streamBase, leaveBaseStreamOpen);

            _currentEntry.SetOutputStream(output);

            return output;
        }

        private ArchiveEntry? GetArchiveEntry(uint index)
        {
            if (index >= _archive.Count)
            {
                return null;
            }
            
            return _archive.Entries[(int)index];
        }

        private void SetCurrentOperation(uint index, ExtractOperation operation)
        {
            _currentEntry.SetOperation(index, operation);
        }
        
        private Stream OnGetStream(uint index, out OperationResult result)
        {
            var entry = GetArchiveEntry(index);

            result = OperationResult.Ok;
            try
            {
                return entry != null ? _onGetStream(entry.Value) : default;
            }
            catch (Exception ex)
            {
                _delegate.OnGetStreamFailed((int)index, entry, ex);
                result = OperationResult.Unavailable;
                return null;
            }
        }

        #region IArchiveExtractCallback
        void IArchiveExtractCallback.SetTotal(ulong total)
        {
            Total = total;
        }

        void IArchiveExtractCallback.SetCompleted([In] ref ulong completeValue)
        {
        }

        int IArchiveExtractCallback.GetStream(uint index, [MarshalAs(UnmanagedType.Interface)] out IOutputStream output, ExtractOperation operation)
        {
            EnsureNotDisposed();
            SetCurrentOperation(index, operation);

            switch (operation)
            {
                case ExtractOperation.Test:
                    output = null;
                    return 0;
                
                case ExtractOperation.Skip:
                    output = null;
                    return 0;

                case ExtractOperation.Extract:
                default:
                    var baseStream = OnGetStream(index, out var result);

                    output = baseStream != null ? CreateOutputStream(baseStream) : default;
                    return (int)result;
            }
        }

        void IArchiveExtractCallback.PrepareOperation(ExtractOperation operation)
        {
        }

        void IArchiveExtractCallback.SetOperationResult(ExtractOperationResult result)
        {
            _currentEntry.Reset(result, this);
        }
        #endregion

        #region IPasswordProvider
        int IPasswordProvider.CryptoGetPassword(out string password)
        {
            password = _archive?.Config?.Password;
            return 0;
        }
        #endregion

        #region CurrentEntry
        private sealed class CurrentEntry
        {
            private ArchiveStream _stream;
            private uint? _index;
            private ExtractOperation _operation;

            public void SetOperation(uint index, ExtractOperation operation) => (_index, _operation) = (index, operation);
            
            public void SetOutputStream(ArchiveStream stream) => _stream = stream;
            
            public void Reset(ExtractOperationResult result, ExtractContext context)
            {
                if (context != null && _index != null)
                {
                    context._delegate.OnExtractOperation((int)_index.Value, context.GetArchiveEntry(_index.Value), _operation, result);
                }

                _stream?.Dispose();
                _stream = null;
                _index = null;
            }
        }
        #endregion
    }
}
