using System;
using System.IO;
using SevenZip.Interop;

namespace SevenZip;

/// <summary>
/// Default implementation of the <see cref="IArchiveExtractCallback"/> interface, which is required to obtain
/// output streams to write extracted content to and to receive status notifications while a decompression
/// is in progress.
/// </summary>
#if NET8_0_OR_GREATER
[System.Runtime.InteropServices.Marshalling.GeneratedComClass]
#endif
internal sealed partial class ExtractContext : MarshalByRefObject, IArchiveExtractCallback, IPasswordProvider, IDisposable
{
    private readonly CurrentEntry _state = new();
    private readonly IArchiveReaderDelegate _delegate;
    private readonly Func<ArchiveEntry, Stream> _onGetStream;
    private readonly ArchiveReader _reader;
    private readonly ArchiveFlags _flags;
    private ulong _total;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractContext"/> class.
    /// </summary>
    /// <param name="reader">The archive to work with.</param>
    /// <param name="delegate">The delegate to forward status and error notifications to.</param>
    /// <param name="flags">Additional flags to configure the extract operation behavior. See <see cref="ArchiveFlags"/>
    /// for details.</param>
    /// <param name="onGetStream">The callback to invoke when an output stream for an entry is needed by the
    /// decoder.</param>
    public ExtractContext(ArchiveReader reader, IArchiveReaderDelegate @delegate, ArchiveFlags flags, Func<ArchiveEntry, Stream> onGetStream)
    {
        _flags = flags;
        _reader = reader;
        _delegate = @delegate;
        _onGetStream = onGetStream;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractContext"/> class. This constructor must only
    /// be used for the extraction test mode.
    /// </summary>
    public ExtractContext() => _onGetStream = _ => null;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
            
        _disposed = true;
        _state.Reset(OperationResult.Ok, null);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("The ExtractContext object has already been disposed.");
        }
    }

    private ArchiveEntry? GetArchiveEntry(uint index)
    {
        if (index >= _reader.Count)
        {
            return null;
        }
            
        return _reader.Entries[(int)index];
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
        _total = total;
    }

    void IArchiveExtractCallback.SetCompleted(ref ulong completeValue)
    {
        _delegate?.OnProgress(completeValue, _total);
    }

    int IArchiveExtractCallback.GetStream(uint index, out ISequentialOutputStream output, ExtractOperation operation)
    {
        EnsureNotDisposed();

        var result = OperationResult.Ok;
        var stream = default(ArchiveStream);

        switch (operation)
        {
            case ExtractOperation.Test:
                output = null;
                break;
                
            case ExtractOperation.Skip:
                output = null;
                break;

            case ExtractOperation.Extract:
            default:
                var leaveOpen = (_flags & ArchiveFlags.DisposeEntryStreams) == 0;

                output = stream = OnGetStream(index, out result) switch
                {
                    { } baseStream => new ArchiveStream(baseStream, leaveOpen),
                    _ => null,
                };
                break;
        }

        _state.Set(index, operation, stream);
        return (int)result;
    }

    void IArchiveExtractCallback.PrepareOperation(ExtractOperation operation)
    {
    }

    void IArchiveExtractCallback.SetOperationResult(OperationResult result)
    {
        _state.Reset(result, this);
    }
    #endregion

    #region IPasswordProvider
    int IPasswordProvider.CryptoGetPassword(out string password)
    {
        EnsureNotDisposed();

        password = _reader?.Config?.Password;
        return 0;
    }
    #endregion

    #region CurrentEntry
    private sealed class CurrentEntry
    {
        private ArchiveStream _stream;
        private uint? _index;
        private ExtractOperation _operation;

        public void Set(uint index, ExtractOperation operation, ArchiveStream stream) =>
            (_index, _operation, _stream) = (index, operation, stream);
           
        public void Reset(OperationResult result, ExtractContext context)
        {
            if (context != null && _index != null)
            {
                context._delegate.OnExtractOperation((int)_index.Value, context.GetArchiveEntry(_index.Value), _operation, result);
            }

            _operation = ExtractOperation.Skip;
            _stream?.Dispose();
            _stream = null;
            _index = null;
        }
    }
    #endregion
}