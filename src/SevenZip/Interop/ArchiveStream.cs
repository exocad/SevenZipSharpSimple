﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using SevenZip.Detail;

namespace SevenZip.Interop;

/// <summary>
/// Implementation of the <see cref="IInputStream"/> and <see cref="ISequentialOutputStream"/> interfaces which
/// wrap a given <see cref="Stream"/>.
/// </summary>
#if NET8_0_OR_GREATER
[System.Runtime.InteropServices.Marshalling.GeneratedComClass]
partial 
#endif
class ArchiveStream : IInputStream, IOutputStream, IDisposable
{
    private readonly FileUpdateInfo _updateInfo;
    private readonly bool _leaveOpen;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveStream"/> class.
    /// </summary>
    /// <param name="baseStream">
    /// The base stream to refer to.
    /// </param>
    /// <param name="leaveOpen">
    /// If <c>true</c>, the stream will be left open when this
    /// instance is being disposed. Otherwise, the <paramref name="baseStream"/> will
    /// be disposed as well.
    /// </param>
    /// <param name="updateInfo">
    /// If specified, and if the <paramref name="baseStream"/> is a <see cref="FileStream"/>
    /// providing a valid path, the properties of this update information will be applied
    /// to the target file when this instance is being disposed.
    /// 
    /// This operation may fail or not work correctly if <paramref name="leaveOpen"/> is
    /// <c>true</c>, since the stream is still in use and might prevent access or implicitly
    /// modify a file property after this instace has been disposed.
    /// </param>
    public ArchiveStream(Stream baseStream, bool leaveOpen, FileUpdateInfo updateInfo = null)
    {
        _updateInfo = updateInfo;
        _leaveOpen = leaveOpen;
        BaseStream = baseStream;
    }

    /// <summary>
    /// Gets the underlying base stream this instance operates on.
    /// </summary>
    public Stream BaseStream { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var path = GetFileStreamPathOrNull();

        if (_leaveOpen is false)
        {
            BaseStream?.Dispose();
        }

        if (_updateInfo != null && !string.IsNullOrEmpty(path))
        {
            TrySetFileTime(path, _updateInfo.CreationTime, File.SetCreationTime);
            TrySetFileTime(path, _updateInfo.LastWriteTime, File.SetLastWriteTime);
            TrySetFileTime(path, _updateInfo.LastAccessTime, File.SetLastAccessTime);
        }

        return;

        static void TrySetFileTime(string path, DateTime time, Action<string, DateTime> method)
        {
            if (time == DateTime.MinValue)
            {
                return;
            }

            try
            {
                method(path, time);
            }
            catch
            {
                // ignore
            }
        }
    }

    private string GetFileStreamPathOrNull() => BaseStream switch
    {
        FileStream fileStream => fileStream.Name,
        _ => null,
    };

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("The InputStream has already been disposed.");
        }
    }

    private int WriteCore(byte[] data, uint size)
    {
        EnsureNotDisposed();
        BaseStream.Write(data, 0, (int)size);
        return (int)size;
    }

    private int ReadCore(byte[] buffer, uint size)
    {
        EnsureNotDisposed();
        return BaseStream.Read(buffer, 0, (int)size);
    }

    private int SeekCore(long offset, SeekOrigin origin, IntPtr newPositionPtr)
    {
        EnsureNotDisposed();

        var position = BaseStream.Seek(offset, origin);

        if (newPositionPtr != IntPtr.Zero)
        {
            unsafe
            {
                System.Runtime.CompilerServices.Unsafe.Write(newPositionPtr.ToPointer(), position);
            }
        }

        return 0;
    }

    private int SetSizeCore(long size)
    {
        EnsureNotDisposed();
        BaseStream.SetLength(size);

        return 0;
    }

    /// <inheritdoc />
    int ISequentialInputStream.Read([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] byte[] buffer, uint size) => ReadCore(buffer, size);

#if !NET8_0_OR_GREATER
    int IInputStream.Read([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] byte[] buffer, uint size) => ReadCore(buffer, size);
#endif

    /// <inheritdoc />
    int IInputStream.Seek(long offset, SeekOrigin origin, IntPtr newPositionPtr) => SeekCore(offset, origin, newPositionPtr);

    /// <inheritdoc />
    int ISequentialOutputStream.Write(byte[] data, uint size) => WriteCore(data, size);

#if !NET8_0_OR_GREATER
    /// <inheritdoc />
    int IOutputStream.Write(byte[] data, uint size) => WriteCore(data, size);
#endif

    /// <inheritdoc />
    int IOutputStream.Seek(long offset, SeekOrigin origin, IntPtr newPositionPtr) => SeekCore(offset, origin, newPositionPtr);

    /// <inheritdoc />
    int IOutputStream.SetSize(long size) => SetSizeCore(size);
}