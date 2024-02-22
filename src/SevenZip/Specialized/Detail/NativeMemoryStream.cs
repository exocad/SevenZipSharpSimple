using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SevenZip.Specialized.Detail;

/// <summary>
/// The <see cref="NativeMemoryStream"/> class references a region of native
/// memory that can be read from or written to. The streams is limited to
/// the capacity passed at construction and cannot grow beyond that value.
/// </summary>
internal sealed class NativeMemoryStream : Stream
{
    private readonly IntPtr _pointer;
    private readonly int _capacity;
    private int _cursor;
    private int _length;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeMemoryStream"/> class.
    /// </summary>
    /// <param name="pointer">The pointer of the associated memory region.</param>
    /// <param name="capacity">The capacity of this stream.</param>
    public NativeMemoryStream(IntPtr pointer, int capacity)
    {
        _pointer = pointer;
        _capacity = capacity;
    }

    /// <inheritdoc />
    public override void Flush()
    {
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        var length = Math.Min(_capacity - _cursor, count);

        unsafe
        {
            var source = new Span<byte>(GetPointerAt(_pointer, _cursor), length);
            var target = new Span<byte>(buffer, offset, length);

            source.CopyTo(target);
            _cursor += length;
        }

        return length;
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        var length = Math.Min(_capacity - _cursor, count);

        unsafe
        {
            var source = new Span<byte>(buffer, offset, length);
            var target = new Span<byte>(GetPointerAt(_pointer, _cursor), length);

            source.CopyTo(target);
            _cursor += length;

            if (_length < _cursor)
            {
                _length = _cursor;
            }
        }
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _cursor = (int)offset;
                break;

            case SeekOrigin.End:
                _cursor = _length + (int)offset;
                break;

            case SeekOrigin.Current:
                _cursor += (int)offset;
                break;
        }

        if (_cursor < 0 || _cursor > _length)
        {
            throw new IOException("The seek operation failed.");
        }

        return _cursor;
    }

    /// <inheritdoc />
    public override void SetLength(long value)
    {
        if (value > _capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "The given length exceeds the stream capacity.");
        }

        _length = (int)value;

        if (_cursor > _length)
        {
            _cursor = _length;
        }
    }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => true;

    /// <inheritdoc />
    public override bool CanWrite => true;

    /// <inheritdoc />
    public override long Length => _length;

    /// <inheritdoc />
    public override long Position
    {
        get => _cursor;
        set
        {
            if (value > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The given position exceeds the stream length.");
            }

            _cursor = (int)value;
        }
    }

    private static unsafe void* GetPointerAt(IntPtr pointer, int offset)
    {
        return Unsafe.Add<byte>(pointer.ToPointer(), offset);
    }
}