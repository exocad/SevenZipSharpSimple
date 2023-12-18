using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SevenZipSharpSimple.Detail
{
    /// <summary>
    /// String related functions to convert native strings to managed strings.
    /// </summary>
    internal static class StringMarshal
    {
        /// <summary>
        /// Creates a managed string from the given unmanaged string located in unmanaged memory.
        /// </summary>
        /// <param name="ptr">
        /// The pointer to the first character of the binary string. The length of the string,
        /// in bytes, must be stored as 32-bit integer just before this pointer (e.g. <c>((uint*)ptr)[-1]</c>).
        /// 
        /// The per-character width depends on the current platform. 2 bytes are assumed for Windows and
        /// 4 bytes for Linux.
        /// </param>
        /// <returns>The resulting managed string.</returns>
        public static unsafe string BinaryStringToManagedString(IntPtr ptr)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Marshal.PtrToStringBSTR(ptr);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // The length of the string, in bytes, is stored one element prior to the address of 'ptr'.
                // On Linux, the per character width is 32 bits (4 bytes); so we use UTF-32 encoding
                // here.

                ref var lengthPtr = ref Unsafe.AsRef<uint>(ptr.ToPointer());
                var length = Unsafe.Subtract(ref lengthPtr, 1);
                var result = Encoding.UTF32.GetString((byte *)ptr, (int)length);

                return result;
            }

            return string.Empty;
        }
    }
}
