using System;

namespace SevenZip;

/// <summary>
/// A default implementation of the <see cref="IArchiveReaderDelegate"/> interface which
/// accepts lambda expressions as callbacks.
/// </summary>
public sealed class ArchiveReaderDelegate : IArchiveReaderDelegate
{
    private readonly Action<int, ArchiveEntry?, Exception> _onGetStreamFailed;
    private readonly Action<int, ArchiveEntry?, ExtractOperation, OperationResult> _onExtractOperation;
    private readonly Action<ulong, ulong> _onProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveReaderDelegate"/> class.
    /// </summary>
    /// <param name="onGetStreamFailed">This callback is invoked when an output stream
    /// could not be obtained. May be <c>null</c>.</param>
    /// <param name="onExtractOperation">This callback is invoked when the status of
    /// an extract operation changed. May be <c>null</c>.</param>
    /// <param name="onProgress">This callback is invoked when progress is being reported.</param>
    public ArchiveReaderDelegate(Action<int, ArchiveEntry?, Exception> onGetStreamFailed = null,
        Action<int, ArchiveEntry?, ExtractOperation, OperationResult> onExtractOperation = null,
        Action<ulong, ulong> onProgress = null)
    {
        _onGetStreamFailed = onGetStreamFailed;
        _onExtractOperation = onExtractOperation;
        _onProgress = onProgress;
    }

    internal static IArchiveReaderDelegate Default { get; } = new ArchiveReaderDelegate();

    #region IProgressDelegate
    void IProgressDelegate.OnProgress(ulong current, ulong total) =>
        _onProgress?.Invoke(current, total);
    #endregion

    #region IArchiveReaderDelegate
    void IArchiveReaderDelegate.OnGetStreamFailed(int index, ArchiveEntry? entry, Exception ex) => 
        _onGetStreamFailed?.Invoke(index, entry, ex);

    void IArchiveReaderDelegate.OnExtractOperation(int index, ArchiveEntry? entry, ExtractOperation operation, OperationResult result) =>
        _onExtractOperation?.Invoke(index, entry, operation, result);
    #endregion
}
