<#
.SYNOPSIS
  実行中の自動ビルド（BuildAndRelease.yml）が Unity に使わせる CPU の割合を切り替える。

.DESCRIPTION
  セルフホストランナーの PC で実行する。実行中ジョブの Runner.Worker の PID と割合をファイルに書き、
  ワークフローが30秒ごとに読み取って、Unity とその子プロセスに反映する。
  - 反映は実行中のジョブだけ。ジョブが終わると、次のジョブには引き継がれない。
  - 手動実行で指定した割合や、自動ビルドの安全制限よりも優先する。
  - Unity の -job-worker-count は起動時に決まるため、次のプロファイル（Unity の再起動）から反映される。

.EXAMPLE
  .\PowerShell\SetBuildCpuMode.ps1 100     # 100% に切り替える
  .\PowerShell\SetBuildCpuMode.ps1 25      # 25% に切り替える
  .\PowerShell\SetBuildCpuMode.ps1 reset   # 切り替えを解除して、ジョブ開始時の設定に戻す
  .\PowerShell\SetBuildCpuMode.ps1         # 今の状態を表示する
#>
param(
    [ValidateSet("25", "50", "75", "100", "reset", "status")]
    [string]$Mode = "status"
)

$ErrorActionPreference = "Stop"
$CpuModeFile = Join-Path $env:ProgramData "SymphonyKillChord\build-cpu-mode.json"

function Show-Status {
    $Workers = @(Get-Process -Name "Runner.Worker" -ErrorAction SilentlyContinue)
    if ($Workers.Count -eq 0) {
        Write-Host "実行中のビルドはありません。"
    } else {
        Write-Host "実行中のジョブ（Runner.Worker PID）: $($Workers.Id -join ', ')"
    }

    if (-not (Test-Path -LiteralPath $CpuModeFile)) {
        Write-Host "CPU使用率の切り替え: なし（ジョブ開始時の設定で動作）"
        return
    }

    $CpuMode = Get-Content -LiteralPath $CpuModeFile -Raw -Encoding UTF8 | ConvertFrom-Json
    $IsActive = $Workers.Id -contains [int]$CpuMode.runner_worker_pid
    $State = if ($IsActive) { "適用中" } else { "無効（そのジョブは終了済み）" }
    Write-Host "CPU使用率の切り替え: $($CpuMode.percent)%（$State、設定: $($CpuMode.updated_at)）"
}

if ($Mode -eq "status") {
    Show-Status
    return
}

if ($Mode -eq "reset") {
    if (Test-Path -LiteralPath $CpuModeFile) {
        Remove-Item -LiteralPath $CpuModeFile -Force
    }
    Write-Host "切り替えを解除しました。30秒以内にジョブ開始時の設定へ戻ります。"
    return
}

$Workers = @(Get-Process -Name "Runner.Worker" -ErrorAction SilentlyContinue)
if ($Workers.Count -eq 0) {
    Write-Host "実行中のビルドがないため、切り替えませんでした。"
    exit 1
}
if ($Workers.Count -gt 1) {
    Write-Host "実行中のジョブが複数あります（PID: $($Workers.Id -join ', ')）。最も新しいジョブに適用します。"
}
$Worker = $Workers | Sort-Object StartTime -Descending | Select-Object -First 1

$CpuModeDir = Split-Path -Parent $CpuModeFile
if (-not (Test-Path -LiteralPath $CpuModeDir)) {
    New-Item -ItemType Directory -Path $CpuModeDir | Out-Null
}

$CpuMode = [ordered]@{
    percent           = [int]$Mode
    runner_worker_pid = $Worker.Id
    updated_at        = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
}
$CpuMode | ConvertTo-Json | Set-Content -LiteralPath $CpuModeFile -Encoding UTF8

$CpuCount = [Math]::Max(1, [Math]::Floor([Math]::Min(63, [Environment]::ProcessorCount) * [int]$Mode / 100))
Write-Host "CPU使用率を $Mode%（$CpuCount / $([Environment]::ProcessorCount) CPU）に切り替えました。30秒以内に反映されます。"
Write-Host "Runner.Worker PID: $($Worker.Id)"
