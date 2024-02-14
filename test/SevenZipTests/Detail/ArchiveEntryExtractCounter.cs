using System;
using SevenZip;

namespace SevenZipTests.Detail;

internal sealed class ArchiveEntryExtractCounter : IArchiveReaderDelegate
{
    private long _counter;

    public long Result => _counter;

    void IProgressDelegate.OnProgress(ulong current, ulong total)
    {
    }

    void IArchiveReaderDelegate.OnGetStreamFailed(int index, ArchiveEntry? entry, Exception ex)
    {
        throw new ApplicationException($"Failed to create stream for index {index}.");
    }

    void IArchiveReaderDelegate.OnExtractOperation(int index, ArchiveEntry? entry, ExtractOperation operation, OperationResult result)
    {
        _counter++;
    }
}
