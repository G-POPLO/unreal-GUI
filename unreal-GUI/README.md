# ACSync.Core

Incremental update/patch library for binary files.

## Features

- SHA256-based file comparison for accurate change detection
- Multi-threaded directory scanning
- 7z (LZMA2) compression via 7-Zip.CommandLine
- Patch creation (delta between old and new version)
- Patch application with file deletion and extension exclusion support

## API Overview

```csharp
// Scan directory and create manifest
var manifest = ManifestService.ScanDirectory(@"D:\app\v2.0");
ManifestService.SaveToFile(manifest, @"D:\app\v2.0\acsync_manifest.json");

// Compare manifests and create patch
SyncPatch.CreatePatch(@"D:\app\v1.0", @"D:\app\v2.0");

// Apply patch with exclusions
SyncPatch.ApplyPatch(@"D:\app\user", excludeExtensions: new[] { ".json", ".ini" });
```

## Key Types

### ManifestService

| Method | Description |
|---|---|
| `ScanDirectory(path)` | Scan directory, compute SHA256 for all files |
| `SaveToFile(manifest, path)` | Save manifest as JSON |
| `LoadFromFile(path)` | Load manifest from JSON |
| `LoadFromDirectory(path)` | Load manifest from directory (looks for `acsync_manifest.json`) |
| `Diff(old, new)` | Compare two manifests, returns (changed, deleted) |

### SyncPatch

| Method | Description |
|---|---|
| `CreatePatch(oldPath, newPath, outputPath?, sevenZipPath?)` | Create `acsync_patch.7z` from old→new delta |
| `ApplyPatch(targetPath, patchFile?, excludeExtensions?, sevenZipPath?)` | Apply patch to target directory |

> `sevenZipPath` — Optional. Defaults to `AppContext.BaseDirectory/7za.exe` (application root directory).

## Dependencies

- .NET 10.0+
- `7-Zip.CommandLine` (7za.exe required at runtime)
