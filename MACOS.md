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

This repository only ships:

- `runtimes/win-x64/native/7z.dll`
- `runtimes/linux-x64/native/7z.so`

There is **no macOS build** of the native 7z library, so on macOS the
`ArchiveReader`/`ArchiveWriter` based tests cannot run. The parts of the
library that are pure C# — in particular the `Lzma` class (the managed port of
the original 7-Zip SDK code) — work natively on macOS; the `LzmaTests` pass
(2/2) on Apple Silicon.

To use the archive operations on macOS, build a macOS binary of 7-Zip (e.g.
from https://github.com/mcmilk/7-Zip for `osx-arm64`, exposing the same C API
as the `Format7zF` bundle used for `7z.so`) and pass it via
`ArchiveConfig.NativeLibraryPath`.

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

Status on macOS arm64 (2026-10-25):

| Test area                        | Result |
| -------------------------------- | ------ |
| `LzmaTests` (pure C# SDK)        | pass   |
| Archive reader/writer tests      | not runnable — no native 7z for macOS |
