# Exercises installed package assets on both supported core frameworks.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$consumer = Join-Path $repository ('artifacts/consumer-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $consumer | Out-Null
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFrameworks>net9.0-windows;net472</TargetFrameworks><LangVersion>latest</LangVersion>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects><GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
  </PropertyGroup>
  <PropertyGroup Condition="'`$(TargetFramework)' == 'net9.0-windows'"><UseWindowsForms>true</UseWindowsForms></PropertyGroup>
  <ItemGroup><PackageReference Include="distrohelena.nucleus.net" Version="1.0.4" /></ItemGroup>
  <ItemGroup Condition="'`$(TargetFramework)' == 'net9.0-windows'"><PackageReference Include="distrohelena.nucleus.net.windows" Version="1.0.3" /></ItemGroup>
</Project>
"@
[IO.File]::WriteAllText((Join-Path $consumer 'Smoke.csproj'), $project)
$program = @"
using System;
using System.IO;
using Nucleus;

public class Program {
    public static void Main() {
        var value = new SmokePayload { Name = "package", Count = 42 };
        var result = new SmokePayload();
        ApplicationUtil.PopulateObjectWithArgument(result, ApplicationUtil.GetObjectAsArgument(value));
        if (result.Name != value.Name || result.Count != value.Count) throw new Exception("JSON round trip failed.");
        var environment = Nucleus.ConsoleEngine.TerminalEnvironment.Detect(Nucleus.ConsoleEngine.RendererPreference.Basic);
        if (environment.UseAnsiSequences) throw new Exception("Basic renderer unexpectedly enables ANSI.");
        string root = Path.Combine(AppContext.BaseDirectory, "copy-fixture-" + Guid.NewGuid().ToString("N"));
        DirectoryInfo source = Directory.CreateDirectory(Path.Combine(root, "source"));
        DirectoryInfo target = Directory.CreateDirectory(Path.Combine(root, "target"));
        try {
            File.WriteAllText(Path.Combine(source.FullName, "keep.txt"), "keep");
            Directory.CreateDirectory(Path.Combine(source.FullName, "skip"));
            File.WriteAllText(Path.Combine(source.FullName, "skip", "hidden.txt"), "skip");
            DirectoryUtil.RecursiveCopy(source, target, (file, reader, writer) => writer.Write(reader.ReadToEnd().ToUpperInvariant()), file => true, directory => directory.Name != "skip");
            if (File.ReadAllText(Path.Combine(target.FullName, "keep.txt")) != "KEEP" || Directory.Exists(Path.Combine(target.FullName, "skip"))) throw new Exception("Directory filter failed.");
        } finally { Directory.Delete(root, true); }
#if !NETFRAMEWORK
        Console.WriteLine(typeof(Nucleus.Platform.Windows.Controls.BaseForm).Assembly.GetName().Name);
#endif
        Console.WriteLine("Installed Nucleus package checks passed.");
    }
}
"@
[IO.File]::WriteAllText((Join-Path $consumer 'Program.cs'), $program)
[IO.File]::WriteAllText((Join-Path $consumer 'SmokePayload.cs'), 'public sealed class SmokePayload { public string Name { get; set; } public int Count { get; set; } }')
$feed = [System.Security.SecurityElement]::Escape((Join-Path $repository 'artifacts/packages'))
$config = '<configuration><packageSources><clear/><add key="local" value="' + $feed + '"/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>'
[IO.File]::WriteAllText((Join-Path $consumer 'NuGet.Config'), $config)
dotnet restore (Join-Path $consumer 'Smoke.csproj') --packages (Join-Path $consumer 'packages') --configfile (Join-Path $consumer 'NuGet.Config') --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Consumer restore failed.' }
dotnet build (Join-Path $consumer 'Smoke.csproj') --no-restore --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Consumer build failed.' }
dotnet (Join-Path $consumer 'bin/Debug/net9.0-windows/Smoke.dll')
if ($LASTEXITCODE -ne 0) { throw '.NET 9 Windows smoke test failed.' }
& (Join-Path $consumer 'bin/Debug/net472/Smoke.exe')
if ($LASTEXITCODE -ne 0) { throw '.NET Framework smoke test failed.' }
Write-Output "Consumer retained at $consumer"