namespace SevenZip;

/// <summary>
/// The <see cref="IProgressDelegate"/> is used to track the progress of an extract or compress operation. It is
/// supported by the <see cref="IArchiveReaderDelegate"/> and <see cref="IArchiveWriterDelegate"/> interfaces.
/// </summary>
public interface IProgressDelegate
{
    /// <summary>
    /// Indicates the current progress of the extract operation.
    /// </summary>
    /// <param name="current">The current progress value with a range of [0 and <paramref name="total"/>].</param>
    /// <param name="total">The value indicating progress completion.</param>
    void OnProgress(ulong current, ulong total);
}