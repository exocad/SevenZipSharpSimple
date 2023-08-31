using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenZipSharpSimple.Interop
{
    /// <summary>
    /// Implementation of the <see cref="IInputStream"/> and <see cref="IOutputStream"/> interfaces which
    /// wrap a given <see cref="Stream"/>.
    /// </summary>
    internal sealed class ArchiveStream : IInputStream, IOutputStream, IDisposable
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

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The InputStream has already been disposed.");
            }
        }

        #region IInputStream
        /// <inheritdoc />
        int IInputStream.Read([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] byte[] buffer, uint size)
        {
            EnsureNotDisposed();

            return BaseStream.Read(buffer, 0, (int)size);
        }

        /// <inheritdoc />
        long IInputStream.Seek(long offset, SeekOrigin origin)
        {
            EnsureNotDisposed();
            return BaseStream.Seek(offset, origin);
        }
        #endregion

        #region IOutputStream
        /// <inheritdoc />
        int IOutputStream.Write(byte[] data, uint size)
        {
            EnsureNotDisposed();
            BaseStream.Write(data, 0, (int)size);
            return (int)size;
        }
        #endregion

        #region IDisposable
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
        #endregion
    }
}
