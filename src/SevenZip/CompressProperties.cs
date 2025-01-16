using System;
using System.Collections.Generic;
using SevenZip.Detail;
using SevenZip.Interop;

namespace SevenZip;

/// <summary>
/// Optional settings that may be used to configure the compression operation.
/// 
/// <para>
/// An overview of possible options can be found on the following page: https://7zip.bugaco.com/7zip/MANUAL/cmdline/switches/method.htm.
/// </para>
/// </summary>
public sealed class CompressProperties
{
    /// <summary>
    /// Gets the <see cref="SevenZip.CompressionLevel"/> to use or <c>null</c>, to use the default.
    /// </summary>
    public CompressionLevel? CompressionLevel { get; init; }

    /// <summary>
    /// Gets the <see cref="SevenZip.CompressionMethod"/> to use or <c>null</c>, to use the default.
    /// </summary>
    public CompressionMethod? CompressionMethod { get; init; }

    /// <summary>
    /// Gets the <see cref="SevenZip.EncryptionMethod"/> to use. This property is only
    /// recognized when creating Zip archives.
    /// </summary>
    public EncryptionMethod? EncryptionMethod { get; init; }

    /// <summary>
    /// Gets a dictionary which may contain format-specific parameters.
    /// </summary>
    public IDictionary<string, string> Parameters { get; init; }

    /// <summary>
    /// Applies the <see cref="CompressionLevel"/>, <see cref="CompressionMethod"/> and
    /// <see cref="Parameters"/> to the given <see cref="IArchiveWriter"/>, if it
    /// supports the <see cref="IArchiveProperties"/> interface.
    /// 
    /// Invalid properties will just be skipped.
    /// </summary>
    /// <param name="writer">The writer to assign the properties to.</param>
    /// <param name="format">The <see cref="ArchiveFormat"/> being used.</param>
    internal void Apply(IArchiveWriter writer, ArchiveFormat format)
    {
        var setter = ComCast.As<IArchiveWriter, IArchiveProperties>(writer);
        if (setter == null || format == ArchiveFormat.Tar)
        {
            return;
        }

        var count = 3 + (Parameters?.Count ?? 0);
        var index = 0;
        var names = new IntPtr[count];
        var values = new Union[count];
        try
        {
            if (Parameters != null)
            {
                foreach (var pair in Parameters)
                {
                    names[index] = StringMarshal.ManagedStringToBinaryString(pair.Key);

                    if (uint.TryParse(pair.Value, out var value))
                    {
                        values[index] = Union.Create(value);
                    }
                    else
                    {
                        values[index] = Union.Create(pair.Value);
                    }

                    index++;
                }
            }

            if (EncryptionMethod is { } encryption && format == ArchiveFormat.Zip)
            {
                names[index] = StringMarshal.ManagedStringToBinaryString("em");
                values[index] = Union.Create(EncryptionMethodToString(encryption));
                index++;
            }

            if (CompressionMethod is { } method and not SevenZip.CompressionMethod.Default &&
                CanUseCompressionMethod(method, format))
            {
                names[index] = StringMarshal.ManagedStringToBinaryString(format == ArchiveFormat.Zip ? "m" : "0");
                values[index] = Union.Create(CompressionMethodToString(method));
                index++;
            }

            if (CompressionLevel is { } level)
            {
                names[index] = StringMarshal.ManagedStringToBinaryString("x");
                values[index] = Union.Create(CompressionLevelToUInt32(level));
                index++;
            }

            using var namesHandle = new GcHandleGuard(names);
            using var valuesHandle = new GcHandleGuard(values);

            setter.SetProperties(namesHandle.Pointer, valuesHandle.Pointer, index);
        }
        finally
        {
            Array.ForEach(names, StringMarshal.BinaryStringFree);
            Array.ForEach(values, value => value.Free());
        }
    }

    private static string EncryptionMethodToString(EncryptionMethod method) => method switch
    {
        SevenZip.EncryptionMethod.ZipCrypto => "ZipCrypto",
        SevenZip.EncryptionMethod.Aes128 => "AES128",
        SevenZip.EncryptionMethod.Aes192 => "AES192",
        SevenZip.EncryptionMethod.Aes256 => "AES256",
        _ => string.Empty,
    };

    private static uint CompressionLevelToUInt32(CompressionLevel level) => level switch
    {
        SevenZip.CompressionLevel.None => 0u,
        SevenZip.CompressionLevel.Fast => 1u,
        SevenZip.CompressionLevel.Low => 3u,
        SevenZip.CompressionLevel.Normal => 5u,
        SevenZip.CompressionLevel.High => 7u,
        SevenZip.CompressionLevel.Ultra => 9u,
        _ => 5u,
    };

    private static bool CanUseCompressionMethod(CompressionMethod method, ArchiveFormat format)
    {
        if (method == SevenZip.CompressionMethod.Default)
        {
            return true;
        }

        return format switch
        {
            ArchiveFormat.GZip => method == SevenZip.CompressionMethod.Deflate,
            ArchiveFormat.BZip2 => method == SevenZip.CompressionMethod.BZip2,
            ArchiveFormat.SevenZip => method is 
                not SevenZip.CompressionMethod.Deflate and
                not SevenZip.CompressionMethod.Deflate64,
            ArchiveFormat.Tar => method == SevenZip.CompressionMethod.Copy,
            ArchiveFormat.Zip => method != SevenZip.CompressionMethod.Lzma2,
            _ => true
        };
    }

    private static string CompressionMethodToString(CompressionMethod method) => method switch
    {
        SevenZip.CompressionMethod.BZip2 => "BZip2",
        SevenZip.CompressionMethod.Copy => "Copy",
        SevenZip.CompressionMethod.Deflate => "Deflate",
        SevenZip.CompressionMethod.Deflate64 => "Deflate64",
        SevenZip.CompressionMethod.Lzma => "LZMA",
        SevenZip.CompressionMethod.Lzma2 => "LZMA2",
        SevenZip.CompressionMethod.Ppmd => "PPMd",
        _ => "",
    };
}
