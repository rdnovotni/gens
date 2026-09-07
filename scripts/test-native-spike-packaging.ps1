param([string]$PublishRoot = 'artifacts/native-spike')
$ErrorActionPreference = 'Stop'
$payloads = @{
    'win-x64' = @('SDL3.dll','libSkiaSharp.dll','libHarfBuzzSharp.dll')
    'linux-x64' = @('libSDL3.so.0','libSkiaSharp.so','libHarfBuzzSharp.so')
    'osx-arm64' = @('libSDL3.0.dylib','libSkiaSharp.dylib','libHarfBuzzSharp.dylib')
}
$missing = @()
foreach ($rid in @('win-x64','linux-x64','osx-arm64')) {
    foreach ($file in $payloads[$rid]) {
        $path = Join-Path (Join-Path $PublishRoot $rid) $file
        if (Test-Path -LiteralPath $path) {
            "$rid,$file,present,$((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash)"
        } else {
            "$rid,$file,MISSING"
            $missing += "$rid/$file"
        }
    }
}
if ($missing.Count -gt 0) { throw "Incomplete native packaging: $($missing -join ', ')" }
