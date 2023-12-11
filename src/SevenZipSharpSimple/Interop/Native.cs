using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// Native method declarations.
    /// </summary>
    internal static class Native
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
#if NET8_0_OR_GREATER
            out IntPtr @interfacePtr
#else
            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.Interface)] out object @interface
#endif
            );

        public static IntPtr LoadLibrary(string name)
        {
            var path = System.IO.Path.Join(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), name);
            var handle = NativeLibrary.Load(path);

            return handle;
        }

        public static TDelegate GetExportAs<TDelegate>(IntPtr library, string name)
            where TDelegate : class
        {
            var handle = NativeLibrary.GetExport(library, name);
            var function = Marshal.GetDelegateForFunctionPointer<TDelegate>(handle);

            return function;
        }
    }
}
