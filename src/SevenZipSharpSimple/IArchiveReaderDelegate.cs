using System;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// The <see cref="IArchiveReaderDelegate"/> can be used to receive status notifications while testing
    /// or extracting an archive.
    /// </summary>
    public interface IArchiveReaderDelegate
    {
        /// <summary>
        /// This method is called by the <see cref="ArchiveReader"/> when an output stream for an extract
        /// operation could not be obtained or opened.
        /// </summary>
        /// <param name="index">The index of the current <see cref="ArchiveEntry"/>.</param>
        /// <param name="entry">The current <see cref="ArchiveEntry"/> or <c>null</c>, if the entry does not exist.</param>
        /// <param name="ex">The exception that has been thrown.</param>
        void OnGetStreamFailed(int index, ArchiveEntry? entry, Exception ex);
        
        /// <summary>
        /// This method is called by the <see cref="ArchiveReader"/> when an extract operation for one <see cref="ArchiveEntry"/>
        /// completed or failed.
        /// </summary>
        /// <param name="index">The index of the current <see cref="ArchiveEntry"/>.</param>
        /// <param name="entry">The current <see cref="ArchiveEntry"/> or <c>null</c>, if the entry does not exist.</param>
        /// <param name="operation">The operation that was performed for the given entry.</param>
        /// <param name="result">The result of the operation.</param>
        void OnExtractOperation(int index, ArchiveEntry? entry, ExtractOperation operation, ExtractOperationResult result);
    }
}
