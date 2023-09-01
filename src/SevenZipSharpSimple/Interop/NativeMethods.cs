using System;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// Native method declarations.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// A delegate declaration for the native <c>CreateObject</c> function used
        /// to create a COM object.
        /// </summary>
        /// <param name="classId">The UUID of the class to create.</param>
        /// <param name="interfaceId">The UUID of the interface to create.</param>
        /// <param name="interface">The created interface or <c>null</c>, if the creation failed.</param>
        /// <returns>Zero (0), if the operation succeeded or a non-zero value indicating an error otherwise.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int CreateObjectDelegate(
            [In] ref Guid classId,
            [In] ref Guid interfaceId,
            [MarshalAs(UnmanagedType.Interface)] out object @interface);
        
        /// <summary>
        /// Loads a native library.
        /// </summary>
        /// <param name="path">The path of the library to load.</param>
        /// <returns>The handle for the loaded library or <see cref="IntPtr.Zero"/>, if the operation failed.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string path);
        
        /// <summary>
        /// Gets the address of a given function.
        /// </summary>
        /// <param name="library">The library handle.</param>
        /// <param name="name">The name of the function to obtain the address for.</param>
        /// <returns>The function address or <see cref="IntPtr.Zero"/>, if the function could not be located.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetProcAddress(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string name);
    }
}
