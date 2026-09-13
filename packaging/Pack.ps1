# Builds both patch packages without publishing them.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
foreach ($project in @('nucleus-dotnet/nucleus-dotnet/nucleus.dotnet.csproj', 'nucleus-dotnet/nucleus-dotnet-windows/NucleusDotNet.Windows.NETCore.csproj')) {
    dotnet pack (Join-Path $repository $project) -c Release -o (Join-Path $repository 'artifacts/packages') --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Packing failed: $project" }
}