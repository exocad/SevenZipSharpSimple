using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// Implementation of the <see cref="IInputStream"/> and <see cref="ISequentialOutputStream"/> interfaces which
    /// wrap a given <see cref="Stream"/>.
    /// </summary>
#if NET8_0_OR_GREATER
    [System.Runtime.InteropServices.Marshalling.GeneratedComClass]
    partial
#endif
    class ArchiveStream : IInputStream, ISequentialOutputStream, IDisposable
    {
        private readonly bool _disposeBaseStream;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveStream"/> class.
        /// </summary>
        /// <param name="baseStream">The base stream to refer to.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream will be left open when this
        /// instance is being disposed. Otherwise, the <paramref name="baseStream"/> will
        /// be disposed as well.</param>
        public ArchiveStream(Stream baseStream, bool leaveOpen = true)
        {
            _disposeBaseStream = !leaveOpen;

            BaseStream = baseStream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveStream"/> class and updates its
        /// cursor to the given <paramref name="position"/>.
        /// </summary>
        /// <param name="baseStream">The base stream to refer to.</param>
        /// <param name="position">The position to set the <paramref name="baseStream"/> to.</param>
        /// <param name="leaveOpen">If <c>true</c>, the stream will be left open when this
        /// instance is being disposed. Otherwise, the <paramref name="baseStream"/> will
        /// be disposed as well.</param>
        public ArchiveStream(Stream baseStream, long position, bool leaveOpen = true)
            : this(baseStream, leaveOpen)
        {
            BaseStream.Seek(position, SeekOrigin.Begin);
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
            
            if (_disposeBaseStream)
            {
                BaseStream?.Dispose();
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The InputStream has already been disposed.");
            }
        }

        private int Write(byte[] data, uint size)
        {
            EnsureNotDisposed();
            BaseStream.Write(data, 0, (int)size);
            return (int)size;
        }

        private int Read(byte[] buffer, uint size)
        {
            EnsureNotDisposed();
            return BaseStream.Read(buffer, 0, (int)size);
        }

        private int Seek(long offset, SeekOrigin origin, IntPtr newPositionPtr)
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

        /// <inheritdoc />
#if NET8_0_OR_GREATER
        int ISequentialInputStream.Read([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] byte[] buffer, uint size) => Read(buffer, size);
#else
        int IInputStream.Read([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] byte[] buffer, uint size) => Read(buffer, size);
#endif

        /// <inheritdoc />
        int IInputStream.Seek(long offset, SeekOrigin origin, IntPtr newPositionPtr) => Seek(offset, origin, newPositionPtr);

        /// <inheritdoc />
        int ISequentialOutputStream.Write(byte[] data, uint size) => Write(data, size);
    }
}
