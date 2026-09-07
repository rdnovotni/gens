param([string]$OutputDirectory = 'artifacts/native-spike/win-x64')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$cache = Join-Path $root 'artifacts/toolchain'
New-Item -ItemType Directory -Force $cache | Out-Null
$archive = Join-Path $cache 'SDL3-devel.zip'
$expected = '093821FCD2B0EAFEDC86E93713687136872A6556966DB036FEBA2672F58586ED'
if (!(Test-Path -LiteralPath $archive)) {
    Invoke-WebRequest 'https://github.com/libsdl-org/SDL/releases/download/release-3.2.22/SDL3-devel-3.2.22-VC.zip' -OutFile $archive
}
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $expected) { throw 'SDL archive SHA-256 mismatch' }
Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $cache 'sdl') -Force
$output = [IO.Path]::GetFullPath($OutputDirectory, $root)
dotnet publish (Join-Path $root 'experiments/Gens.NativeSpike') -c Release -r win-x64 --self-contained false -o $output
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
Copy-Item -LiteralPath (Join-Path $cache 'sdl/SDL3-3.2.22/lib/x64/SDL3.dll') -Destination $output
Copy-Item -LiteralPath (Join-Path $cache 'sdl/SDL3-3.2.22/LICENSE.txt') -Destination (Join-Path $output 'SDL-LICENSE.txt')
foreach ($file in 'SDL3.dll','libSkiaSharp.dll','libHarfBuzzSharp.dll') {
    $path = Join-Path $output $file
    if (!(Test-Path -LiteralPath $path)) { throw "Missing native payload: $file" }
    Get-FileHash -LiteralPath $path -Algorithm SHA256
}
