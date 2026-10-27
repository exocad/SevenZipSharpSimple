# Building and running on macOS (Apple Silicon)

This document describes how to build the `SevenZip` library for .NET 10 on macOS
with Apple Silicon (arm64), what works out of the box, and where a native
dependency is still missing. Verified with .NET SDK 10.0.401 (runtime 10.0.12)
on macOS arm64.

## Building

```bash
dotnet restore SevenZipSharpSimple.sln
dotnet build SevenZipSharpSimple.sln -f net10.0
```

- Only the `net10.0` target framework is used here. The `net48` target requires
  the .NET Framework reference assemblies, which are only available on Windows
  (or via the `Microsoft.NETFramework.ReferenceAssemblies` package), and the
  `net8.0` target requires the .NET 8 SDK or its targeting pack.
- The solution and both projects build with 0 warnings and 0 errors.
  `GeneratePackageOnBuild` produces the NuGet package when packing (see below).

### PlatformTarget / AMD64 IL

`Directory.Build.props` sets `<Platform>x64</Platform>`. The .NET SDK maps a
`Platform` of `x64` to `PlatformTarget=x64` (see `Microsoft.NET.Sdk.props`),
which makes the C# compiler stamp the IL with the AMD64 machine type
(PE machine `0x8664`). The arm64 .NET runtime refuses to load such assemblies
with `FileLoadException: The assembly architecture is not compatible with the
current process architecture`, so on Apple Silicon the built test host (a native
arm64 executable) could not even start.

`Directory.Build.props` therefore forces `PlatformTarget=anycpu` on
non-Windows platforms. This produces portable ILOnly assemblies
(PE machine `0x14C`) which load on any architecture. Windows builds are
unchanged and still produce x64-stamped assemblies, so the existing
Windows-based CI is not affected. The same issue also affected Linux arm64
(aarch64) before this change.

If you build a single project and cannot apply the fix, the equivalent
command-line workaround is `dotnet build -p:PlatformTarget=anycpu`.

## Native 7z library

The archive operations (create, modify, extract) P/Invoke into a native 7z
library whose path can be configured via `ArchiveConfig.NativeLibraryPath`
(defaulting to `7z.dll` on Windows and `7z.so` on Linux).

This repository ships:

- `runtimes/win-x64/native/7z.dll`
- `runtimes/linux-x64/native/7z.so`
- `runtimes/osx-arm64/native/lib7z.dylib` (macOS, Apple Silicon)

All of them expose the same C API entry point (`CreateObject`), which the
library resolves at runtime; a different binary can be supplied via
`ArchiveConfig.NativeLibraryPath`.

### Building `lib7z.dylib`

The macOS library is built from the official 7-Zip git repository
(https://github.com/ip7z/7zip), which succeeded the former mirror
(https://github.com/mcmilk/7-Zip), in the same way as `7z.so`, using the
`Format7zF` bundle. The library currently shipped in
`runtimes/osx-arm64/native/` was built from version 26.05 (tag `26.05`,
commit `b88c6bc09339de32f7d18bbab900686b244f42ba`).

On a macOS machine with the Xcode command line tools installed:

```bash
git clone --branch 26.05 --depth 1 https://github.com/ip7z/7zip.git
cd 7zip/CPP/7zip/Bundles/Format7zF
make -f makefile.gcc CC=clang CXX=clang++ "CFLAGS_BASE=-O2 -c -DNDEBUG -D_REENTRANT -fPIC"
# copy the result into the SevenZipSharpSimple repository:
cp _o/7z.so /path/to/SevenZipSharpSimple/runtimes/osx-arm64/native/lib7z.dylib
```

The `CFLAGS_BASE` override disables the `-Werror` flag of the Linux makefile,
which is not portable to clang on macOS.

### String marshalling

The 7z C API passes strings as BSTR-like pointers. On Windows these hold
16 bit characters, while the Linux and macOS builds of the native library
use 32 bit `wchar_t`. The managed side (`StringMarshal`) therefore treats
Linux and macOS identically: a 4 byte length prefix followed by UTF-32
characters, matching the `SysAllocString` implementation of 7-Zip for
non-Windows platforms.

## Packaging

`dotnet pack` does not support the `-f`/`--framework` switch, and packing the
`SevenZip.csproj` directly fails with `NU5019: File not found: .../README.md`,
because the project references `$(SolutionDir)README.md`, and `$(SolutionDir)`
is empty when a project is built directly. Pack through the solution instead:

```bash
dotnet pack SevenZipSharpSimple.sln -c Debug -p:TargetFrameworks=net10.0
```

This produces `artifacts/package/debug/Exocad.SevenZip.2.3.0.nupkg`.

## Running the tests

The test project uses xunit.v3 with the in-process test runner (MTP), so the
test executable is a regular apphost:

```bash
./artifacts/bin/SevenZipTests/debug_net10.0/SevenZipTests          # all tests
./artifacts/bin/SevenZipTests/debug_net10.0/SevenZipTests -class "*Lzma*"
```

Filter options: `-class "name"`, `-method "Ns.Class.Method"`,
`-namespace "name"` (wildcards supported). Note that the MTP query options
such as `--filter-method` are not wired up for this runner.

With `runtimes/osx-arm64/native/lib7z.dylib` present, the complete test suite
and the `TestConsoleProgram` verification program run on macOS arm64.
