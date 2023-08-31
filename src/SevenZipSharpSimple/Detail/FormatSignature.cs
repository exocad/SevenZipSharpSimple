
using System.Collections.Generic;
using System.Linq;

namespace SevenZipSharpSimple.Detail
{
    /// <summary>
    /// Representation of a known archive format signature.
    /// </summary>
    /// <remarks>
    /// Most signatures can be found on the following page: https://en.wikipedia.org/wiki/List_of_file_signatures
    /// </remarks>
    internal readonly struct FormatSignature
    {
        private readonly byte[] _signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatSignature"/> struct.
        /// </summary>
        /// <param name="format">The format to register a signature for.</param>
        /// <param name="signature">The bytes indicating the given <paramref name="format"/>.</param>
        public FormatSignature(ArchiveFormat format, params byte[] signature) => (Format, _signature) = (format, signature);
        
        /// <summary>
        /// Gets the <see cref="ArchiveFormat"/> of this instance.
        /// </summary>
        public ArchiveFormat Format { get; }
        
        /// <summary>
        /// Gets the length of the format's signature, in bytes.
        /// </summary>
        public int SignatureLength => _signature.Length;

        /// <summary>
        /// Gets a static list containing all known signatures.
        /// </summary>
        public static IReadOnlyList<FormatSignature> KnownSignatures { get; } = CreateSignatureList();
        
        /// <summary>
        /// Tests if the given <paramref name="buffer"/> contains the signature of this
        /// instance at the given <paramref name="offset"/>.
        /// </summary>
        /// <param name="buffer">The buffer whose content shall be compared against the signature of this format.</param>
        /// <param name="offset">The offset within the given <paramref name="buffer"/> where the signature is expected.</param>
        /// <returns><c>true</c>, if the signature was found. Otherwise <c>false</c>.</returns>
        public bool HasSignature(byte[] buffer, int offset)
        {
            if (offset + SignatureLength >= buffer.Length)
            {
                return false;
            }

            for (var i = 0; i < SignatureLength; ++i)
            {
                if (buffer[offset + i] != _signature[i])
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Searches the given <paramref name="buffer"/> for the signature of this instance starting at <paramref name="offset"/>.
        /// </summary>
        /// <param name="buffer">The buffer to iterate over to find the format's signature.</param>
        /// <param name="offset">The offset within the given <paramref name="buffer"/> where the search starts.</param>
        /// <returns>The index where the format's signature was found or -1, if the signature was not found.</returns>
        public int FindSignature(byte[] buffer, int offset)
        {
            for (; offset + SignatureLength <= buffer.Length; ++offset)
            {
                if (HasSignature(buffer, offset))
                {
                    return offset;
                }
            }
            
            return -1;
        }

        /// <summary>
        /// Gets the <see cref="FormatSignature"/> for the given <paramref name="format"/>.
        /// </summary>
        /// <param name="format">The format to retrieve the signature for.</param>
        /// <returns>The corresponding signature.</returns>
        public static FormatSignature Get(ArchiveFormat format)
        {
            return KnownSignatures.FirstOrDefault(signature => signature.Format == format);
        }
        
        private static IReadOnlyList<FormatSignature> CreateSignatureList()
        {
            return new FormatSignature[]
            {
                new FormatSignature(ArchiveFormat.SevenZip, 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C),
                new FormatSignature(ArchiveFormat.GZip, 0x1F, 0x8B, 0x08),
                new FormatSignature(ArchiveFormat.Tar, 0x75, 0x73, 0x74, 0x61, 0x72),
                new FormatSignature(ArchiveFormat.Rar, 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00),
                new FormatSignature(ArchiveFormat.Zip, 0x50, 0x4B, 0x03, 0x04),
                new FormatSignature(ArchiveFormat.Lzma, 0x5D, 0x00, 0x00, 0x40, 0x00),
                new FormatSignature(ArchiveFormat.Lzh, 0x2D, 0x6C, 0x68),
                new FormatSignature(ArchiveFormat.Lzw, 0x1F, 0x9D, 0x90),
                new FormatSignature(ArchiveFormat.Arj, 0x60, 0xEA),
                new FormatSignature(ArchiveFormat.BZip2, 0x42, 0x5A ,0x68),
                new FormatSignature(ArchiveFormat.Cab, 0x4D, 0x53, 0x43, 0x46),
                new FormatSignature(ArchiveFormat.Chm, 0x49, 0x54, 0x53, 0x46),
                new FormatSignature(ArchiveFormat.Deb, 0x21, 0x3C, 0x61, 0x72, 0x63, 0x68, 0x3E, 0x0A, 0x64, 0x65, 0x62, 0x69, 0x61, 0x6E, 0x2D, 0x62, 0x69, 0x6E, 0x61, 0x72, 0x79),
                new FormatSignature(ArchiveFormat.Iso, 0x43, 0x44, 0x30, 0x30, 0x31),
                new FormatSignature(ArchiveFormat.Rpm, 0xED, 0xAB, 0xEE, 0xDB),
                new FormatSignature(ArchiveFormat.Wim, 0x4D, 0x53, 0x57, 0x49, 0x4D, 0x00, 0x00, 0x00),
                new FormatSignature(ArchiveFormat.Xar, 0x78, 0x61, 0x72, 0x21),
                new FormatSignature(ArchiveFormat.Hfs, 0x48, 0x2B),
                new FormatSignature(ArchiveFormat.Xz, 0xFD, 0x37, 0x7A, 0x58, 0x5A),
                new FormatSignature(ArchiveFormat.Flv, 0x46, 0x4C, 0x56),
                new FormatSignature(ArchiveFormat.Swf, 0x46, 0x57, 0x53),
                new FormatSignature(ArchiveFormat.Pe, 0x4D, 0x5A),
                new FormatSignature(ArchiveFormat.Elf, 0x7F, 0x45, 0x4C, 0x46),
                new FormatSignature(ArchiveFormat.Dmg, 0x78),
                new FormatSignature(ArchiveFormat.Vhd, 0x63, 0x6F, 0x6E, 0x65, 0x63, 0x74, 0x69, 0x78)
            };
        }
    }
}
