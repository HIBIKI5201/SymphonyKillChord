$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot

if (-not (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
    throw 'Node.js (npm) is required. Install Node.js 22.12 or later.'
}

if (-not (Test-Path -LiteralPath 'node_modules/astro/package.json')) {
    & npm.cmd ci
    if ($LASTEXITCODE -ne 0) {
        throw 'npm ci failed.'
    }
}

$astro = Join-Path $PSScriptRoot 'node_modules/.bin/astro.cmd'
& $astro dev --background --host 127.0.0.1 --port 4321
if ($LASTEXITCODE -ne 0) {
    throw 'Astro dev server failed to start.'
}

$status = (& $astro dev status --json 2>&1) -join "`n"
if ($LASTEXITCODE -ne 0 -or $status -notmatch 'https?://[^\s"\\]+') {
    throw "Could not determine the dev server URL: $status"
}

$url = $Matches[0].TrimEnd('/') + '/SymphonyKillChord/home'
Write-Host "Opening $url"
Start-Process $url
