$ErrorActionPreference = 'Stop'
Set-Location "$PSScriptRoot\.."
& .\scripts\build-release.ps1

[xml]$csproj = Get-Content .\*.csproj
$dllName = $csproj.Project.PropertyGroup.AssemblyName
$name = $csproj.Project.PropertyGroup.Product
$version = $csproj.Project.PropertyGroup.Version

if (Test-Path BepInEx) { Remove-Item -Recurse BepInEx }
mkdir -Force BepInEx\plugins
Copy-Item "bin/Release/netstandard2.1/$dllName.dll" "BepInEx/plugins"

Compress-Archive -Force -Path @(
	"manifest.json",
	"icon.png",
	"README.md",
	"CHANGELOG.md",
	"BepInEx"
) -DestinationPath "taffyko-$name-$version.zip"

Remove-Item -Recurse BepInEx