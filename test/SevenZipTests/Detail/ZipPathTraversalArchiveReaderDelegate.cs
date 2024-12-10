using System;
using SevenZip;

namespace SevenZipTests.Detail;

internal sealed class ZipPathTraversalArchiveReaderDelegate : IArchiveReaderDelegate
{
    public void OnGetStreamFailed(int index, ArchiveEntry? entry, Exception ex)
    {
        throw ex;
    }

    public void OnExtractOperation(int index, ArchiveEntry? entry, ExtractOperation operation, OperationResult result)
    {
    }

    public void OnProgress(ulong current, ulong total)
    {
    }
}
