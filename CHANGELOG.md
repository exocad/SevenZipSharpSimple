# SevenZipSharp Changelog

## [2.0.0] - 2025-01-14

### Changed

- The major version has been changed to 2 due to changes in the public API of `ArchiveReader`, `IProgressDelete`, `IArchiveReaderDelegate` and `IArchiveWriterDelegate`
  and a behavioral change in the error handling during extract or compress operations (see `ArchiveConfig.IgnoreOperationErrors`).
- Unified and slightly changed the public interface of `ArchiveReader` so that it now has a single public `Extract`
  method while all overloads were moved an an extension class.
- By default, errors which occur during compression or extraction now cause an exception of type `ArchiveOperationException`.
  This behavior can be controlled with the `ArchiveConfig.IgnoreOperationErrors` property, which defaults to `false`.
- The delegate interfaces `IProgressDelete`, `IArchiveReaderDelegate` and `IArchiveWriterDelegate` have been
  extended with another parameter (`IExtractContext` or `ICompressContext`), which can be used to provide
  more information about the current operation.

## [1.0.3] - 2025-01-07

### Fixed

- Fixed a NullReferenceException caused by calling `ArchiveReader.CanExtractEntries`.

## [1.0.2] - 2024-12-10

### Changed

- Added a guard to prevent the [directory traversal attack (CVE-2024-48510)](https://github.com/advisories/GHSA-xhg6-9j5j-w4vf).

## [1.0.1] - 2024-11-06

### Changed

- The generated assembly now always has a strong-name.

## [1.0.0] - 2024-05-03

### Changed

- Initial release of the .NET SevenZip library.