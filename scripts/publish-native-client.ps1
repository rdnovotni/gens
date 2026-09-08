param(
    [ValidateSet("win-x64", "linux-x64", "osx-arm64")][string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputRoot = "artifacts/native"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$publishDirectory = Join-Path $repositoryRoot (Join-Path $OutputRoot $Runtime)
$resolvedOutputRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot)) + [IO.Path]::DirectorySeparatorChar
$resolvedPublishDirectory = [IO.Path]::GetFullPath($publishDirectory)
if (-not $resolvedPublishDirectory.StartsWith($resolvedOutputRoot, [StringComparison]::OrdinalIgnoreCase)) { throw "Publish directory escapes the configured artifact root." }
if (Test-Path -LiteralPath $resolvedPublishDirectory) { Remove-Item -LiteralPath $resolvedPublishDirectory -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null

$clientProject = Join-Path $repositoryRoot "src/Gens.Client.Desktop/Gens.Client.Desktop.csproj"
dotnet restore $clientProject --runtime $Runtime | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed for $Runtime." }
dotnet clean $clientProject --configuration $Configuration --runtime $Runtime | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet clean failed for $Runtime." }

dotnet publish $clientProject `
    --configuration $Configuration --runtime $Runtime --self-contained true `
    --output $publishDirectory `
    -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $Runtime." }

$entryPoint = if ($Runtime.StartsWith("win-", [StringComparison]::Ordinal)) { "Gens.Client.Desktop.exe" } else { "Gens.Client.Desktop" }
$required = @($entryPoint, "Gens.Client.Desktop.dll", "Assets/NotoSans-Regular.ttf", "Assets/Localization/en.json")
foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $publishDirectory $relative))) { throw "Package is missing $relative." }
}
if ($Runtime -eq "win-x64" -and -not (Get-ChildItem -LiteralPath $publishDirectory -Recurse -Filter SDL3.dll)) { throw "Windows package is missing SDL3.dll." }
if ($Runtime -ne "win-x64" -and (Get-ChildItem -LiteralPath $publishDirectory -Recurse -Filter SDL3.dll)) { throw "Non-Windows package contains the incompatible Windows SDL payload." }
if (Get-ChildItem -LiteralPath $publishDirectory -Recurse | Where-Object { $_.FullName -match '[\\/](Assets|Packages|ProjectSettings)[\\/]' -and $_.FullName -notmatch '[\\/]Assets[\\/](NotoSans|Localization|OFL)' }) { throw "Package unexpectedly contains Unity project content." }

$metadata = [ordered]@{
    version = "0.1.0"
    commitSha = (git -C $repositoryRoot rev-parse HEAD)
    channel = if ($env:GENS_BUILD_CHANNEL) { $env:GENS_BUILD_CHANNEL } else { "local" }
    runtime = $Runtime
    selfContained = $true
    createdUtc = [DateTimeOffset]::UtcNow.ToString("O")
    nativeAudio = "sdl3-pcm16-with-null-device-fallback"
}
$metadata | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $publishDirectory "release-metadata.json") -Encoding utf8NoBOM

if ($Runtime.StartsWith("win-", [StringComparison]::Ordinal)) {
    $archive = "$publishDirectory.zip"
    Compress-Archive -Path (Join-Path $publishDirectory "*") -DestinationPath $archive -Force
} else {
    $archive = "$publishDirectory.tar.gz"
    & tar -czf $archive -C $publishDirectory .
    if ($LASTEXITCODE -ne 0) { throw "tar failed for $Runtime." }
}
Write-Output $archive
