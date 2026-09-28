# Restore repository-owned UI source after external packages have been unpacked.
# Keep this entry point ASCII-only so Windows PowerShell 5.1 can read it without a BOM.
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$OutputEncoding = [Text.UTF8Encoding]::new($false)

$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$uiPathspecs = @(
    ':(glob)Assets/**/*.uxml',
    ':(glob)Assets/**/*.uxml.meta',
    ':(glob)Assets/**/*.uss',
    ':(glob)Assets/**/*.uss.meta'
)

# HEAD is the commit selected by actions/checkout, not a moving remote branch.
# Restore even paths left sparse in a reused runner workspace.
& git -C $projectRoot -c core.longpaths=true restore --source=HEAD --worktree --ignore-skip-worktree-bits -- @uiPathspecs
if ($LASTEXITCODE -ne 0) {
    throw "Failed to restore tracked UI assets from HEAD (exit $LASTEXITCODE)."
}

$trackedUiPaths = @(& git -C $projectRoot -c core.quotepath=false ls-files -- @uiPathspecs)
if ($LASTEXITCODE -ne 0 -or $trackedUiPaths.Count -eq 0) {
    throw 'Failed to enumerate tracked UI assets.'
}

foreach ($relativePath in $trackedUiPaths) {
    $fullPath = Join-Path $projectRoot $relativePath
    if (-not [IO.File]::Exists($fullPath)) {
        throw "Tracked UI asset is missing after restore: $relativePath"
    }
}

Write-Host "Restored and verified $($trackedUiPaths.Count) tracked UI source/meta files from HEAD."
