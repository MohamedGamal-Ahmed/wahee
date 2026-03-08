param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "artifacts\publish\$Version"

Write-Host "[1/3] Restoring..."
dotnet restore (Join-Path $root "Wahee.sln")

Write-Host "[2/3] Publishing..."
dotnet publish (Join-Path $root "Wahee.UI\Wahee.UI.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

$zipPath = Join-Path $root "artifacts\Wahee-$Version-$Runtime.zip"
Write-Host "[3/3] Zipping to $zipPath"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath

Write-Host "Done. Artifact: $zipPath"
