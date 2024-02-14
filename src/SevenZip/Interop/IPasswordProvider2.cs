﻿using System.Runtime.InteropServices;

namespace SevenZip.Interop;

/// <summary>
/// This interface is used by the <see cref="IArchiveReader"/> or <see cref="IArchiveWriter"/> when
/// an archive is protected with a password.
/// </summary>
/// <remarks>
/// https://github.com/mcmilk/7-Zip/blob/826145b86107fc0a778ac673348226db180e4532/CPP/7zip/IPassword.h#L49
/// </remarks>
[Guid("23170F69-40C1-278A-0000-000500110000")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#if NET8_0_OR_GREATER
[System.Runtime.InteropServices.Marshalling.GeneratedComInterface]
partial
#else
[ComImport]
#endif
interface IPasswordProvider2
{
    /// <summary>
    /// Queries the password for the current archive.
    /// </summary>
    /// <param name="passwordIsDefined">
    /// If a password exists, this value must be set to 1. Otherwise, it must be set to zero.
    /// </param>
    /// <param name="password">
    /// If a password is set, it must be stored in this <c>out</c> parameter.
    /// </param>
    /// <returns>
    /// A value of zero (0) on success, an error code otherwise.
    /// </returns>
    [PreserveSig]
    int CryptoGetTextPassword2(ref int passwordIsDefined, [MarshalAs(UnmanagedType.BStr)] out string password);
}
