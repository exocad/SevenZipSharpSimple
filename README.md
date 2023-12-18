# SevenZipSharpSimple

Simple 7z project written in C# using the reference SDK sources from https://7-zip.org/sdk.html
and some additional classes that allow accessing some native 7z library features like
archive extraction.

The declaration of the COM interfaces offered by the 7z library were made by reading the 
original definitions from the 7z repository at https://github.com/mcmilk/7-Zip/tree/master.

## Supported Frameworks

This library supports .NET 6.0 and .NET 8.0. The latter also runs under Linux.

### Linux Support

The .NET 8 builds of `SevenZipSharpSimple` can run under Linux. The native 7z Linux
library was build from the original repository at https://github.com/mcmilk/7-Zip
by using the following commands:

```bash
git clone https://github.com/mcmilk/7-Zip.git
cd 7-zip/CPP/7zip/Bundles/Format7zF
make -f makefile.gcc
```

The different bundles and the formats they support are explained
here: https://github.com/mcmilk/7-Zip/blob/master/DOC/readme.txt.

### Native 7z Library

The native 7z library is part of this repository, but not linked directly by the `SevenZipSharpSimple`
project. This can be done by importing the `Native7zReferences.target` file in a `.csproj`, or with
the following snippet:

```xml
  <ItemGroup>
    <None Include="..\..\runtimes\win\native\7z.dll" Link="7z.dll" CopyToOutputDirectory="PreserveNewest" />
    <None Include="..\..\runtimes\linux\native\7z.so" Link="7z.so" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```

## Versioning Scheme

The assembly version corresponds with the original SDK version.

## Signed Assembly

The `SevenZipSharpSimple` assembly will automatically be signed if the keyfile at the following location is present:
* `$(SolutionDir)..\DentalCAD\snk\DentalConfigKeyPublicPrivate.snk`

Otherwise, an unsigned assembly will be generated.

## Usage

### Functionality implemented in pure .NET

Compression into and Decrompression from a binary blob can be handled with the `Archive` class.

### Functionality requiring the native 7z library

Reading files and their contents from an archive file requires the use of the `ArchiveReader` class, which
internally makes use of native methods and some COM interface types declared by 7z.
The `ArchiveReader` dynamically loads the native library at runtime to create the native objects. By default,
it assumes that the native library is located in the same directory as the main executable and named `7z.dll`
or `7z.so`. If the name or location differs, it can be set in the `ArchiveConfig` class.

## Benchmarks

To test the SevenZipSharpLibrary, a benchmark and a unit test project where 7z and zip archives are being decompressed.

The following table contains a local result of a benchmark run.


|                                Method | DataLength |       Mean |     Error |    StdDev |     Median |
|-------------------------------------- |----------- |-----------:|----------:|----------:|-----------:|
|      CompressData_SevenZipSharpSimple |        256 | 4,612.1 us | 160.73 us | 473.93 us | 4,722.3 us |
|            CompressData_SevenZipSharp |        256 | 4,607.5 us | 102.22 us | 299.80 us | 4,646.6 us |
|    DecompressData_SevenZipSharpSimple |        256 |   184.2 us |   3.67 us |  10.35 us |   183.4 us |
|          DecompressData_SevenZipSharp |        256 |   187.9 us |   5.94 us |  16.15 us |   186.7 us |
|  Extract7zArchive_SevenZipSharpSimple |        256 | 3,349.2 us | 111.43 us | 328.56 us | 3,207.7 us |
|        Extract7zArchive_SevenZipSharp |        256 | 6,083.8 us | 119.90 us | 160.07 us | 6,039.9 us |
| ExtractZipArchive_SevenZipSharpSimple |        256 | 3,285.3 us |  64.53 us |  79.25 us | 3,283.9 us |
|       ExtractZipArchive_SevenZipSharp |        256 | 5,930.9 us |  76.66 us |  59.85 us | 5,928.1 us |
|      CompressData_SevenZipSharpSimple |       2048 | 4,185.7 us |  28.65 us |  23.92 us | 4,183.2 us |
|            CompressData_SevenZipSharp |       2048 | 4,198.6 us |  18.98 us |  17.75 us | 4,202.2 us |
|    DecompressData_SevenZipSharpSimple |       2048 |   258.3 us |   3.30 us |   3.08 us |   257.6 us |
|          DecompressData_SevenZipSharp |       2048 |   258.9 us |   2.14 us |   2.00 us |   259.2 us |
|  Extract7zArchive_SevenZipSharpSimple |       2048 | 3,128.4 us |  60.04 us |  71.47 us | 3,119.6 us |
|        Extract7zArchive_SevenZipSharp |       2048 | 6,014.4 us | 118.62 us | 158.35 us | 5,939.3 us |
| ExtractZipArchive_SevenZipSharpSimple |       2048 | 3,315.3 us |  65.52 us |  82.87 us | 3,316.6 us |
|       ExtractZipArchive_SevenZipSharp |       2048 | 5,991.5 us | 118.60 us | 141.18 us | 5,947.2 us |

