using System;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// A default implementation of the <see cref="IArchiveReaderDelegate"/> which accepts lambda expressions
    /// as callbacks
    /// </summary>
    public sealed class ArchiveReaderDelegate : IArchiveReaderDelegate
    {
        private readonly Action<int, ArchiveEntry?, Exception> _onGetStreamFailed;
        private readonly Action<int, ArchiveEntry?, ExtractOperation, ExtractOperationResult> _onExtractOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveReaderDelegate"/> class.
        /// </summary>
        /// <param name="onGetStreamFailed">This callback is invoked when an output stream
        /// could not be obtained. May be <c>null</c>.</param>
        /// <param name="onExtractOperation">This callback is invoked when the status of
        /// an extract operation changed. May be <c>null</c>.</param>
        public ArchiveReaderDelegate(
            Action<int, ArchiveEntry?, Exception> onGetStreamFailed = null,
            Action<int, ArchiveEntry?, ExtractOperation, ExtractOperationResult> onExtractOperation = null)
        {
            _onGetStreamFailed = onGetStreamFailed;
            _onExtractOperation = onExtractOperation;
        }

        internal static IArchiveReaderDelegate Default { get; } = new ArchiveReaderDelegate();

        #region IArchiveReaderDelegate
        void IArchiveReaderDelegate.OnGetStreamFailed(int index, ArchiveEntry? entry, Exception ex)
        {
            _onGetStreamFailed?.Invoke(index, entry, ex);
        }

        void IArchiveReaderDelegate.OnExtractOperation(int index, ArchiveEntry? entry, ExtractOperation operation, ExtractOperationResult result)
        {
            _onExtractOperation?.Invoke(index, entry, operation, result);
        }
        #endregion
    }
}
