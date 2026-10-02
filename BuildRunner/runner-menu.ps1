<#
  SymphonyKillChord セルフホストランナーの管理メニュー。
  runner-menu.bat から起動する。説明は同じフォルダの README.md を参照。

  引数で直接実行もできる:
    runner-menu.bat status
    runner-menu.bat start
    runner-menu.bat cpu 100      (25 / 50 / 75 / 100 / reset / status)
    runner-menu.bat runs
    runner-menu.bat watch
    runner-menu.bat log
    runner-menu.bat setup        (setup-runner.ps1 を実行する)
#>
param(
    [string]$Command,
    [string]$Argument
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$RunnerDir = $PSScriptRoot
$Repo = "HIBIKI5201/SymphonyKillChord"
$CpuModeScript = Join-Path $RunnerDir "SetBuildCpuMode.ps1"
$SetupScript = Join-Path $RunnerDir "setup-runner.ps1"

function Test-RunnerConfigured {
    return Test-Path -LiteralPath (Join-Path $RunnerDir ".runner")
}

function Get-RunnerState {
    $Listener = @(Get-Process -Name "Runner.Listener" -ErrorAction SilentlyContinue)
    $Worker = @(Get-Process -Name "Runner.Worker" -ErrorAction SilentlyContinue)
    if (-not (Test-RunnerConfigured)) { return "未セットアップ" }
    if ($Listener.Count -eq 0) { return "停止中" }
    if ($Worker.Count -gt 0) { return "ビルド実行中" }
    return "待機中"
}

function Show-Status {
    Write-Host ""
    Write-Host "ランナー: $(Get-RunnerState)"
    & $CpuModeScript
    Write-Host ""
    Show-Runs
}

function Start-Runner {
    if (-not (Test-RunnerConfigured)) {
        Write-Host "このPCはまだランナーとして登録されていません。メニューの S（セットアップ）から始めてください。"
        return
    }
    if ((Get-RunnerState) -ne "停止中") {
        Write-Host "ランナーはすでに起動しています。"
        return
    }
    # 別ウィンドウで起動する。そのウィンドウで Ctrl+C を押したり閉じたりするとランナーが止まる。
    Start-Process -FilePath (Join-Path $RunnerDir "run.cmd") -WorkingDirectory $RunnerDir
    Write-Host "ランナーを別ウィンドウで起動しました。そのウィンドウでは Ctrl+C を押さず、閉じないでください。"
}

function Invoke-Setup {
    & $SetupScript
}

function Set-CpuMode([string]$Mode) {
    if ($Mode) {
        & $CpuModeScript $Mode
    } else {
        & $CpuModeScript
    }
}

function Show-Runs {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        Write-Host "gh コマンドが見つかりません。"
        return
    }
    Write-Host "最近のビルド:"
    & gh run list --workflow BuildAndRelease.yml --limit 5 --repo $Repo
}

function Watch-Run {
    $RunId = & gh run list --workflow BuildAndRelease.yml --limit 20 --repo $Repo --json databaseId,status --jq '[.[] | select(.status == "in_progress")][0].databaseId'
    if (-not $RunId) {
        Write-Host "実行中のビルドはありません。"
        return
    }
    & gh run watch $RunId --repo $Repo
}

function Show-RunnerLog {
    $Log = Get-ChildItem -Path (Join-Path $RunnerDir "_diag") -Filter "Runner_*.log" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $Log) {
        Write-Host "診断ログがありません。"
        return
    }
    Write-Host "== $($Log.Name)（最後の20行）"
    Get-Content -LiteralPath $Log.FullName -Tail 20 -Encoding UTF8
}

function Invoke-MenuCommand([string]$Name, [string]$Value) {
    switch ($Name) {
        "status" { Show-Status }
        "start"  { Start-Runner }
        "cpu"    { Set-CpuMode $Value }
        "runs"   { Show-Runs }
        "watch"  { Watch-Run }
        "log"    { Show-RunnerLog }
        "setup"  { Invoke-Setup }
        default  { Write-Host "不明なコマンドです: $Name" }
    }
}

if ($Command) {
    Invoke-MenuCommand $Command $Argument
    return
}

while ($true) {
    Write-Host ""
    Write-Host "===== SymphonyKillChord ランナー管理（ランナー: $(Get-RunnerState)） ====="
    Write-Host " 1. 状態を見る（ランナー・CPU切り替え・最近のビルド）"
    Write-Host " 2. ランナーを起動する（別ウィンドウ）"
    Write-Host " 3. CPU 25%     4. CPU 50%     5. CPU 75%     6. CPU 100%"
    Write-Host " 7. CPU の切り替えを解除する"
    Write-Host " 8. 実行中のビルドを見守る（Ctrl+C で見守りだけ終了）"
    Write-Host " 9. ランナーの診断ログを見る"
    Write-Host " S. セットアップ（このPCをランナーにする）"
    Write-Host " 0. 終了"
    $Choice = Read-Host "番号"
    switch ($Choice) {
        "1" { Show-Status }
        "2" { Start-Runner }
        "3" { Set-CpuMode "25" }
        "4" { Set-CpuMode "50" }
        "5" { Set-CpuMode "75" }
        "6" { Set-CpuMode "100" }
        "7" { Set-CpuMode "reset" }
        "8" { Watch-Run }
        "9" { Show-RunnerLog }
        "s" { Invoke-Setup }
        "0" { return }
        default { Write-Host "0〜9 または S を入力してください。" }
    }
}
