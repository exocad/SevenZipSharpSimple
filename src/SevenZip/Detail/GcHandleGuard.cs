using System;
using System.Runtime.InteropServices;

namespace SevenZip.Detail;

/// <summary>
/// The <see cref="GcHandleGuard"/> is a <c>ref</c> struct which allocates a
/// pinned <see cref="GCHandle"/> for a given object to obtain a non-movable
/// address that may be passed to native methods.
/// 
/// It provides a <c>Dispose</c> method to allow using the <c>using</c> pattern
/// to securely free allocated resources.
/// </summary>
internal readonly ref struct GcHandleGuard
{
    private readonly GCHandle _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="GcHandleGuard"/> struct.
    /// </summary>
    /// <param name="instance">The instance to allocate a pinned handle for.</param>
    public GcHandleGuard(object instance) => _handle = GCHandle.Alloc(instance, GCHandleType.Pinned);

    /// <summary>
    /// Gets the pinned native pointer for the associated object.
    /// </summary>
    public IntPtr Pointer => _handle.AddrOfPinnedObject();

    /// <summary>
    /// Releases the allocated handle again.
    /// </summary>
    public void Dispose() => _handle.Free();
}
