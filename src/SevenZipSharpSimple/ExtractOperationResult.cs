namespace SevenZipSharpSimple
{
    /// <summary>
    /// Enumeration listing the supported extraction operation result types.
    /// </summary>
    public enum ExtractOperationResult
    {
        /// <summary>
        /// The operation succeeded.
        /// </summary>
        Ok,
        
        /// <summary>
        /// The archive entry is compressed with an unsupported compression method (See <see cref="ArchiveEntry.Method"/>).
        /// </summary>
        UnsupportedMethod,
        
        /// <summary>
        /// A data related error occurred, this may happen when a stream cannot be read or written.
        /// </summary>
        DataError,
        
        /// <summary>
        /// A CRC error occurred.
        /// </summary>
        CrcError
    }
}
