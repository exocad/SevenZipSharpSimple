using System.IO;
using SevenZipSharpSimple.Compression.LZMA;

namespace SevenZipSharpSimple
{
    /// <summary>
    /// The <see cref="Archive"/> class provides a set of static methods to easily compress or decompress
    /// binary data.
    /// </summary>
    public sealed class Archive
    {
        /// <summary>
        /// Compresses the content of the <paramref name="source"/> stream and writes the compressed
        /// data to the <paramref name="target"/> stream.
        /// </summary>
        /// <param name="source">The stream containing the data to compress.</param>
        /// <param name="target">The stream to write the compressed data to.</param>
        /// <param name="progress">An option instance of the <see cref="ICodeProgress"/> interface
        /// which can be used to receive progress updates during compression.</param>
        /// <returns>The size of the compressed stream.</returns>
        public static long Compress(Stream source, Stream target, ICodeProgress progress = null)
        {
            var position = target.Position;
            var encoder = CreateEncoderWithDefaultProperties();

            WriteEncoderProperties(encoder, target);
            WriteLength64(source.Length, target);

            encoder.Code(source, target, -1L, -1L, progress);

            return target.Position - position;
        }

        /// <summary>
        /// Compresses the given <paramref name="content"/> and returns a new array containing
        /// the compressed data.
        /// </summary>
        /// <param name="content">The buffer containing the data to compress.</param>
        /// <param name="progress">An option instance of the <see cref="ICodeProgress"/> interface
        /// which can be used to receive progress updates during compression.</param>
        /// <returns>An array containing the compressed data.</returns>
        public static byte[] Compress(byte[] content, ICodeProgress progress = null)
        {
            using (var source = new MemoryStream(content))
            using (var target = new MemoryStream())
            {
                Compress(source, target, progress);
                return target.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the data from the given <paramref name="source"/> stream and writes it to the
        /// <paramref name="target"/> stream.
        /// </summary>
        /// <param name="source">The stream providing the compressed data.</param>
        /// <param name="target">The stream to write the decompressed data to.</param>
        /// <param name="progress">An option instance of the <see cref="ICodeProgress"/> interface
        /// which can be used to receive progress updates during compression.</param>
        public static void Decompress(Stream source, Stream target, ICodeProgress progress = null)
        {
            var decoder = new Decoder();
            var properties = ReadDecoderProperties(source);
            var length = ReadLength64(source);

            decoder.SetDecoderProperties(properties);
            decoder.Code(source, target, source.Length - source.Position, length, progress);
        }

        /// <summary>
        /// Decompresses the contents of the <paramref name="compressed"/> array and returns a new array
        /// containing the decompressed data.
        /// </summary>
        /// <param name="compressed">The array containing the compressed data.</param>
        /// <param name="progress">An option instance of the <see cref="ICodeProgress"/> interface
        /// which can be used to receive progress updates during compression.</param>
        /// <returns>An array containing the decompressed data.</returns>
        public static byte[] Decompress(byte[] compressed, ICodeProgress progress = null)
        {
            using (var source = new MemoryStream(compressed))
            using (var target = new MemoryStream())
            {
                Decompress(source, target, progress);
                return target.ToArray();
            }
        }

        internal const int DictionarySize = 4194304;

        internal static byte[] ReadDecoderProperties(Stream source)
        {
            var buffer = new byte[5];

            ReadExactly(source, buffer, 0, buffer.Length);

            return buffer;
        }

        internal static long ReadLength64(Stream source)
        {
            var length = 0L;

            for (var i = 0; i < 8; ++i)
            {
                var value = source.ReadByte();

                if (value < 0)
                {
                    throw new EndOfStreamException("The source stream did not contain the required number of bytes.");
                }

                length |= ((long)((byte)value) << (i * 8));
            }

            return length;
        }

        internal static void ReadExactly(Stream stream, byte[] buffer, int offset, int length)
        {
            var cursor = 0;

            while (cursor < length)
            {
                var result = stream.Read(buffer, offset, length - cursor);

                if (result == 0)
                {
                    throw new EndOfStreamException("The source stream did not contain the required number of bytes.");
                }

                cursor += result;
                offset += result;
            }
        }

        internal static Encoder CreateEncoderWithDefaultProperties()
        {
            var encoder = new Encoder();
            encoder.SetCoderProperty(CoderPropID.DictionarySize, DictionarySize);
            encoder.SetCoderProperty(CoderPropID.PosStateBits, 2);
            encoder.SetCoderProperty(CoderPropID.LitContextBits, 3);
            encoder.SetCoderProperty(CoderPropID.LitPosBits, 0);
            encoder.SetCoderProperty(CoderPropID.Algorithm, 2);
            encoder.SetCoderProperty(CoderPropID.NumFastBytes, 256);
            encoder.SetCoderProperty(CoderPropID.MatchFinder, "bt4");
            encoder.SetCoderProperty(CoderPropID.EndMarker, false);

            return encoder;
        }

        internal static void WriteEncoderProperties(Encoder encoder, Stream target)
        {
            encoder.WriteCoderProperties(target);
        }

        internal static void WriteLength64(long length, Stream target)
        {
            for (var i = 0; i < 8; ++i)
            {
                target.WriteByte((byte) (length >> (8 * i)));
            }
        }
    }
}
