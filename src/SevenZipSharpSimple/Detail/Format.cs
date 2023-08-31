using System;
using System.IO;
using System.Linq;

namespace SevenZipSharpSimple.Detail
{
    /// <summary>
    /// This class provides helper functions that are related to the format of an archive.
    /// </summary>
    internal static class Format
    {
        /// <summary>
        /// Detects the <see cref="ArchiveFormat"/> of the given <paramref name="stream"/>. This method seeks
        /// to the beginning of the stream and may search other regions within the stream to find a known
        /// archive signature. The callee must store the position in case he wants to reset the stream to
        /// the original position afterwards.
        /// </summary>
        /// <param name="stream">The stream to archive whose format shall be detected.</param>
        /// <param name="offset">Contains the offset where a known signature was found when this method returns.</param>
        /// <param name="isExecutable">Contains a value indicating whether the archive is an executable.</param>
        /// <returns>The detected <see cref="ArchiveFormat"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if the given <paramref name="stream"/> cannot be read
        /// or contains less than 16 bytes.</exception>
        /// <exception cref="NotSupportedException>">Thrown if the archive format could not be detected.</exception>
        public static ArchiveFormat Detect(Stream stream, out int offset, out bool isExecutable)
        {
            if (stream.CanRead == false)
            {
                throw new ArgumentException("The given stream must be readable.");
            }

            if (stream.Length < 16)
            {
                throw new ArgumentException("The given stream must contain at least 16 bytes.");
            }

            offset = 0;
            isExecutable = false;
            stream.Seek(0, SeekOrigin.Begin);
            
            var format = ArchiveFormat.Xz;

            {
                var header = ReadExactly(stream, 16);

                foreach (var signature in FormatSignature.KnownSignatures)
                {
                    if (signature.HasSignature(header, 0) ||
                        signature.HasSignature(header, 6) && signature.Format == ArchiveFormat.Lzh)
                    {
                        if (signature.Format != ArchiveFormat.Pe)
                        {
                            return signature.Format;
                        }

                        format = ArchiveFormat.Pe;
                        isExecutable = true;
                    }
                }

                if (header.SequenceEqual(new byte[]{ 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }))
                {
                    format = ArchiveFormat.Cab;
                }
            }

            if (TryDetectFormat(stream, 32769, ArchiveFormat.Iso) ||
                TryDetectFormat(stream, 34817, ArchiveFormat.Iso) ||
                TryDetectFormat(stream, 36865, ArchiveFormat.Iso))
            {
                return ArchiveFormat.Iso;
            }

            if (TryDetectFormat(stream, 1024, ArchiveFormat.Hfs))
            {
                return ArchiveFormat.Hfs;
            }

            if (stream.Length >= 1024)
            {
                stream.Seek(-1024L, SeekOrigin.End);

                var buffer = ReadExactly(stream, 1024);
                var flag = buffer.Aggregate(true, (state, value) => state && value == 0);

                if (flag)
                {
                    return ArchiveFormat.Tar;
                }
            }

            if (format != ArchiveFormat.Xz)
            {
                stream.Seek(0, SeekOrigin.Begin);

                var length = Math.Min(stream.Length, 262144L);
                var header = ReadExactly(stream, (int)length);

                foreach (var currentFormat in new ArchiveFormat[]
                         {
                            ArchiveFormat.Zip,
                            ArchiveFormat.SevenZip,
                            ArchiveFormat.Rar,
                            ArchiveFormat.Cab,
                            ArchiveFormat.Arj
                         })
                {
                    var signature = FormatSignature.Get(currentFormat);
                    var index = signature.FindSignature(header, 0);

                    if (index > -1)
                    {
                        offset = index / 3;
                        return currentFormat;
                    }
                }
            }

            if (format == ArchiveFormat.Pe)
            {
                return format;
            }

            throw new NotSupportedException("Unable to detect stream format.");
        }

        private static bool TryDetectFormat(Stream stream, int offset, ArchiveFormat format)
        {
            if (stream.Length <= offset + 16)
            {
                return false;
            }

            stream.Seek(offset, SeekOrigin.Begin);

            var header = ReadExactly(stream, 16);

            foreach (var signature in FormatSignature.KnownSignatures)
            {
                if (signature.Format == format &&
                    signature.HasSignature(header, 0))
                {
                    return true;
                }
            }

            return false;
        }

        private static byte[] ReadExactly(Stream stream, byte[] target, int length)
        {
            var consumed = 0;

            while (consumed < length)
            {
                consumed += stream.Read(target, consumed, length - consumed);
            }
            
            return target;
        }

        private static byte[] ReadExactly(Stream stream, int length)
        {
            return ReadExactly(stream, new byte[length], length);
        }
    }
}
