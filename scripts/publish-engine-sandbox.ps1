param([string]$OutputDirectory = 'artifacts/engine-sandbox/win-x64')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$output = [IO.Path]::GetFullPath($OutputDirectory, $root)

dotnet publish (Join-Path $root 'tools/Gens.EngineSandbox') -c Release -r win-x64 --self-contained false -o $output
if ($LASTEXITCODE -ne 0) { throw 'Engine sandbox publish failed.' }

$required = @(
    'Gens.EngineSandbox.exe',
    'libSkiaSharp.dll',
    'libHarfBuzzSharp.dll',
    'runtimes/win-x64/native/SDL3.dll',
    'Assets/NotoSans-Regular.ttf',
    'Assets/NotoSansArabic-Regular.ttf',
    'Assets/OFL.txt'
)
foreach ($relativePath in $required) {
    $path = Join-Path $output $relativePath
    if (!(Test-Path -LiteralPath $path)) { throw "Missing native sandbox payload: $relativePath" }
}

Write-Output "Published engine sandbox with verified native/assets payloads: $output"
