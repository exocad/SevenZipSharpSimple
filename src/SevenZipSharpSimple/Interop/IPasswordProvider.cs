using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// This interface is used by the <see cref="IArchiveReader"/> when an archive is protected or
    /// encrypted with a password.
    /// </summary>
    /// <remarks>
    /// https://github.com/mcmilk/7-Zip/blob/826145b86107fc0a778ac673348226db180e4532/CPP/7zip/IPassword.h#L17
    /// </remarks>
    [Guid("23170F69-40C1-278A-0000-000500100000")]
#if NET8_0_OR_GREATER
    [System.Runtime.InteropServices.Marshalling.GeneratedComInterface]
    unsafe partial
#else
    [ComImport]
#endif
    interface IPasswordProvider
    {
        /// <summary>
        /// Queries the password for the current archive.
        /// </summary>
        /// <param name="password">If a password is set, it must be stored in this <c>out</c> parameter.</param>
        /// <returns>Zero (0), if a password was provided. Non-Zero, if an error occurred.</returns>
        [PreserveSig]
        int CryptoGetPassword([MarshalAs(UnmanagedType.BStr)] out string password);
    }
}