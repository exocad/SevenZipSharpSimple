using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// Native library access.
    /// </summary>
    internal static class Native
    {
        /// <summary>
        /// A delegate declaration for the native <c>CreateObject</c> function used
        /// to create a COM object.
        /// </summary>
        /// <param name="classId">The UUID of the class to create.</param>
        /// <param name="interfaceId">The UUID of the interface to create.</param>
        /// <param name="interfacePtr">The created interface or <c>null</c>, if the creation failed.</param>
        /// <returns>Zero (0), if the operation succeeded or a non-zero value indicating an error otherwise.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int CreateObjectDelegate(
            [In] ref Guid classId,
            [In] ref Guid interfaceId,
#if NET8_0_OR_GREATER
            out IntPtr interfacePtr
#else
            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.Interface)] out object interfacePtr
#endif
            );

        /// <summary>
        /// Loads a native library. If the first attempt failed, the library is searched
        /// in the directory of the current executable.
        /// </summary>
        /// <param name="name">The name or path of the library to load.</param>
        /// <returns>A pointer to the library handle.</returns>
        public static IntPtr LoadLibrary(string name)
        {
            if (NativeLibrary.TryLoad(name, out var handle))
            {
                return handle;
            }

            if (!System.IO.Path.IsPathRooted(name))
            {
                var baseDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                var path = System.IO.Path.Join(baseDirectory, name);

                if (!string.IsNullOrEmpty(baseDirectory) && NativeLibrary.TryLoad(path, out handle))
                {
                    return handle;
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Loads a function named <paramref name="name"/> from the given <paramref name="library"/>.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate declaring the function signature.</typeparam>
        /// <param name="library">The library containing the function to load.</param>
        /// <param name="name">The name of the function to load.</param>
        /// <returns>A delegate for the loaded native function.</returns>
        public static TDelegate GetExportAs<TDelegate>(IntPtr library, string name)
            where TDelegate : class
        {
            var handle = NativeLibrary.GetExport(library, name);
            var function = Marshal.GetDelegateForFunctionPointer<TDelegate>(handle);

            return function;
        }
    }
}
