# Nucleus patch packages

- `distrohelena.nucleus.net` **1.0.4**: .NET 9 and .NET Framework 4.7.2.
- `distrohelena.nucleus.net.windows` **1.0.3**: .NET 9 Windows; depends on core 1.0.4.

Includes upstream console/menu and Windows helper work through `ecf0c9a`, plus the local directory-filter copy overload from `67d0213`.

Restores core package metadata after the upstream project rename, preserves the existing NucleusDotNet assembly name and .NET Framework assets, updates the Windows project reference, and builds the Windows helper as a library. Framework compatibility uses RuntimeInformation for Windows detection and an explicit System.Text.Json dependency for net472.

From the repository root:

```powershell
./packaging/Pack.ps1
./packaging/Verify-Packages.ps1
```

Packages are written to `artifacts/packages`. Verification restores only NuGet packages into a generated consumer and runs JSON round-trip, filtered copy, and basic terminal checks on both core target frameworks. It also loads the Windows library under .NET 9 Windows. This does not exercise interactive WinForms behavior.

Upload core 1.0.4 to nuget.org first, followed by Windows 1.0.3. These artifacts are prepared for manual upload; nothing is pushed or published by the scripts.
