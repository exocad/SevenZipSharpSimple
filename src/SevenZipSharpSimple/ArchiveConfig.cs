using System.Runtime.InteropServices;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// The <see cref="ArchiveConfig"/> class allows providing additional parameters for
    /// an <see cref="ArchiveReader"/>.
    /// </summary>
    public sealed class ArchiveConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveConfig"/> class. This constructor
        /// set the <see cref="Password"/> to <c>null</c> and the <see cref="NativeLibraryPath"/>
        /// to <c>7z.dll</c>.
        /// </summary>
        public ArchiveConfig() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveConfig"/> class.
        /// </summary>
        /// <param name="password">The password to set.</param>
        /// <param name="nativeLibraryPath">The native library path. If empty or <c>null</c>,
        /// <see cref="NativeLibraryPath"/> will be set to <c>7z.dll</c>.</param>
        public ArchiveConfig(string password, string nativeLibraryPath = null)
        {
            Password = password;
            NativeLibraryPath = string.IsNullOrEmpty(nativeLibraryPath) ? Default.NativeLibraryPath : nativeLibraryPath;
        }
        
        /// <summary>
        /// Gets or sets the archive password.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the native 7z library to load.
        /// </summary>
        public string NativeLibraryPath { get; set; } = GetDefault7zLibraryName();

        /// <summary>
        /// Gets the default configuration to use when no user configuration is provided.
        /// </summary>
        internal static ArchiveConfig Default { get; } = new ArchiveConfig();
        
        /// <summary>
        /// Creates a copy of the given <paramref name="config"/> to prevent that properties may be
        /// changed afterwards externally and to ensure that a native library name is set.
        /// </summary>
        /// <param name="config">The configuration to clone.</param>
        /// <returns>A copy of the given <paramref name="config"/> with a default native
        /// library path, if no path was set. Or, if <paramref name="config"/> was <c>null</c>,
        /// the <see cref="Default"/> instance.</returns>
        internal static ArchiveConfig CloneOrDefault(ArchiveConfig config)
        {
            if (config == null)
            {
                return Default;
            }
            
            return new ArchiveConfig()
            {
                NativeLibraryPath = string.IsNullOrEmpty(config.NativeLibraryPath) ? Default.NativeLibraryPath : config.NativeLibraryPath,
                Password = config.Password,
            };
        }

        private static string GetDefault7zLibraryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "7z.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "7z.so";
            }

            throw new System.PlatformNotSupportedException();
        }
    }
}
