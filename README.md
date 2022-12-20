# SevenZipSharpSimple

Simple 7z CSharp project using the example sources from https://7-zip.org/sdk.html

## Supported Frameworks

The assembly is built against .netstandard 2.0 to support both, .NET Frameworks and .NET Core.

## Versioning Scheme

The assembly version corresponds with the original SDK version.

## Signed Assembly

The `SevenZipSharpSimple` assembly will automatically be signed if the keyfile at the following location is present:
* `$(SolutionDir)..\DentalCAD\snk\DentalConfigKeyPublicPrivate.snk`

Otherwise, an unsigned assembly will be generated.
