using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop;

/// <summary>
/// The <see cref="IInputStream"/> is used by an <see cref="IArchiveReader"/> to read the archive data.
///
/// <para>
/// When using .NET 8 or later, this interface supports the ISequentialInputStream interface, which is a separate
/// COM interface and provides the <c>Read</c> method. In earlier versions, however, the <c>Read</c> method must 
/// instead be declared here (in the correct order!) since an access violation occurs otherwise.
/// </para>
/// </summary>
/// <remarks>https://github.com/mcmilk/7-Zip/blob/826145b86107fc0a778ac673348226db180e4532/CPP/7zip/IStream.h#L97</remarks>
[Guid("23170F69-40C1-278A-0000-000300030000")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#if NET8_0_OR_GREATER
[System.Runtime.InteropServices.Marshalling.GeneratedComInterface]
unsafe partial
#else
[ComImport]
#endif
interface IInputStream  : ISequentialInputStream
{
#if !NET8_0_OR_GREATER
    /// <summary>
    /// Read up to <paramref name="size"/> bytes and copy them in the given <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The array to copy the bytes read from the stream to.</param>
    /// <param name="size">The number of bytes to read.</param>
    /// <returns>The number of bytes actually read.</returns>
    int Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, uint size);
#endif

    /// <summary>
    /// Sets the position of the stream.
    /// </summary>
    /// <param name="offset">The offset relative to the given <paramref name="origin"/>.</param>
    /// <param name="origin">The reference point from where to set the position.</param>
    /// <param name="newPositionPtr">A pointer to which to write the new stream position to.</param>
    /// <returns>HRESULT</returns>
    [PreserveSig]
    int Seek(long offset, SeekOrigin origin, IntPtr newPositionPtr);
}
