using System;

namespace SevenZip;

/// <summary>
/// A default implementation of the <see cref="IArchiveWriterDelegate"/> interface which
/// accepts lambda expressions as callbacks.
/// </summary>
public sealed class ArchiveWriterDelegate : IArchiveWriterDelegate
{
    private readonly Action<int, string, Exception> _onGetStreamFailed;
    private readonly Action<int, string, OperationResult> _onCompressOperation;
    private readonly Action<ulong, ulong> _onProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveWriterDelegate"/> class.
    /// </summary>
    /// <param name="onGetStreamFailed">
    /// This callback is invoked when when input stream could not be created.
    /// The first parameter contains the archive entry index, the second one 
    /// contains the archive path and the third one the exception.
    /// </param>
    /// <param name="onCompressOperation">
    /// This callback is invoked when a compress operation completed.
    /// The first parameter contains the archive entry index, the second one 
    /// contains the archive path and the third one the <see cref="OperationResult"/>.
    /// </param>
    /// <param name="onProgress">
    /// This callback is invoked when a progress is being reported.
    /// </param>
    public ArchiveWriterDelegate(Action<int, string, Exception> onGetStreamFailed = null,
        Action<int, string, OperationResult> onCompressOperation = null,
        Action<ulong, ulong> onProgress = null)
    {
        _onGetStreamFailed = onGetStreamFailed;
        _onCompressOperation = onCompressOperation;
        _onProgress = onProgress;
    }

    internal static IArchiveWriterDelegate Default { get; } = new ArchiveWriterDelegate();

    #region IProgressDelegate
    void IProgressDelegate.OnProgress(ulong current, ulong total) =>
        _onProgress?.Invoke(current, total);
    #endregion

    #region IArchiveWriterDelegate
    void IArchiveWriterDelegate.OnGetStreamFailed(int index, string path, Exception ex) =>
        _onGetStreamFailed?.Invoke(index, path, ex);

    void IArchiveWriterDelegate.OnCompressOperation(int index, string path, OperationResult result) =>
        _onCompressOperation?.Invoke(index, path, result);
    #endregion
}
