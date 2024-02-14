# SevenZip

The SevenZip library for .NET provides class to create, modify and extract different kinds
of archives by using the native 7z library from Igor Pavlov. Additionally, it contains the
original SDK code which allows compressing and decompressing binary data using the LZMA
method.

The original SDK code can be downloaded at https://7-zip.org/sdk.html.

The declaration of the COM interfaces offered by the 7z library were made by reading the 
original definitions from the 7z repository at https://github.com/mcmilk/7-Zip/tree/master.

## License

MIT

This repository contains slightly modified code taken from the original LZMA SDK, which can be found
at https://www.7-zip.org/sdk.html. Thanks to the author for putting it into public domain and sharing
the code. The code from the SDK is located in the `SevenZip.CoreSdk` namespace.

## Features

This library allows to create, modify and extract archives the native 7z library supports. It can
operate on files an memory streams.

### Limitations

- Multi-part archives are not supported. This may be added in a future version if needed.
- Encryption is not yet fully supported. It can however be set by adding the parameters
  "he" and/or "em" (for Zip archives only) to the `CompressProperties` class.


## Supported Frameworks

This library supports .NET 4.8 and .NET 6.0 for Windows plus .NET 8.0 for Windows and Linux.

### Linux Support

The .NET 8 builds of `SevenZip` can run under Linux. The native 7z Linux
library was build from the original repository at https://github.com/mcmilk/7-Zip
by using the following commands:

```bash
git clone https://github.com/mcmilk/7-Zip.git
cd 7-zip/CPP/7zip/Bundles/Format7zF
make -f makefile.gcc
```

The different bundles and the formats they support are explained here:
https://github.com/mcmilk/7-Zip/blob/master/DOC/readme.txt.

## Native 7z Library

The native 7z library is part of this repository, but not linked directly by the `SevenZipSharpSimple`
project. This can be done by importing the `Native7zReferences.target` file in a `.csproj`, or with
the following snippet:

```xml
  <ItemGroup>
    <None Include="..\..\runtimes\win\native\7z.dll" Link="7z.dll" CopyToOutputDirectory="PreserveNewest" />
    <None Include="..\..\runtimes\linux\native\7z.so" Link="7z.so" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```

The path to the native library can however be specified when creating an `ArchiveReader` or
`ArchiveWriter`.

## Signed Assembly

The `SevenZip` assembly will automatically be signed if the keyfile at the following location is present:
* `$(SolutionDir)..\DentalCAD\snk\DentalConfigKeyPublicPrivate.snk`

Otherwise, an unsigned assembly will be generated.
