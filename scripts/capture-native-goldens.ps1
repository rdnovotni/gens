param([string]$Configuration = "Release")
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root "tests/Gens.Client.Desktop.Tests/Goldens"
New-Item -ItemType Directory -Force -Path $output | Out-Null
$screens = @("main-menu", "new-game", "settings", "credits", "roster", "character", "estate", "report", "confirmation", "wax-seal")
foreach ($screen in $screens) {
    $capture = Join-Path $output "$screen.png"
    dotnet run --project (Join-Path $root "src/Gens.Client.Desktop") --configuration $Configuration -- --renderer=software --smoke-test --screen=$screen --capture=$capture
    if ($LASTEXITCODE -ne 0) { throw "Native capture failed for $screen." }
}
