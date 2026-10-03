<#
.SYNOPSIS
  ランナーを起動する前に、このPCの状態をスキャンする。start-runner.bat から呼ばれる。

.DESCRIPTION
  - 起動できない問題（未セットアップ、すでに起動中）があれば終了コード 1 で止める。
  - ビルドに影響しうる問題は警告として表示し、続けるか確認する（続けないなら終了コード 1）。
  - 問題がなければ終了コード 0 で戻り、start-runner.bat がこのウィンドウでランナーを起動する。
#>
param(
    [switch]$NoPrompt
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$RunnerDir = $PSScriptRoot
$ProjectRoot = Split-Path -Parent $RunnerDir
$WorkspaceSubPath = "SymphonyKillChord\SymphonyKillChord"
$RecommendedFreeSpaceGB = 80
$CriticalFreeSpaceGB = 40

$Errors = [System.Collections.Generic.List[string]]::new()
$Warnings = [System.Collections.Generic.List[string]]::new()

function Write-Ok([string]$Message) { Write-Host "[OK] $Message" }

Write-Host ""
Write-Host "==== ランナー起動前のチェック ====" -ForegroundColor Cyan

# --- 起動できない問題 ---------------------------------------------------------
if (-not (Test-Path -LiteralPath (Join-Path $RunnerDir ".runner")) -or
    -not (Test-Path -LiteralPath (Join-Path $RunnerDir "run.cmd"))) {
    $Errors.Add("このPCはまだランナーとして登録されていません。BuildRunner\runner-menu.bat の S（セットアップ）から始めてください。")
} else {
    Write-Ok "ランナーは登録済み"
}

if (Get-Process -Name "Runner.Listener" -ErrorAction SilentlyContinue) {
    $Errors.Add("ランナーはすでに起動しています（ほかのウィンドウを確認してください）。")
} else {
    Write-Ok "ほかにランナーは起動していない"
}

# --- 作業フォルダ -------------------------------------------------------------
# ビルド先のパスが長くても、ワークフローが短い別名（<ドライブ>:\SKCBuilder\<ランナー名>）から Unity を起動する。
$WorkFolder = Join-Path $RunnerDir "_work"
$RunnerConfigFile = Join-Path $RunnerDir ".runner"
if (Test-Path -LiteralPath $RunnerConfigFile) {
    $ConfiguredWorkFolder = (Get-Content -LiteralPath $RunnerConfigFile -Raw -Encoding UTF8 | ConvertFrom-Json).workFolder
    if ($ConfiguredWorkFolder) {
        $WorkFolder = if ([System.IO.Path]::IsPathRooted($ConfiguredWorkFolder)) { $ConfiguredWorkFolder } else { Join-Path $RunnerDir $ConfiguredWorkFolder }
    }
}
$WorkProjectDir = Join-Path $WorkFolder $WorkspaceSubPath
Write-Ok "作業フォルダ $WorkFolder"

# --- 空き容量 -----------------------------------------------------------------
$Drive = (Get-Item -LiteralPath $RunnerDir).PSDrive
$FreeGB = [Math]::Round((Get-PSDrive -Name $Drive.Name).Free / 1GB, 1)
if ($FreeGB -lt $CriticalFreeSpaceGB) {
    $Warnings.Add("$($Drive.Name): ドライブの空き容量が $FreeGB GB しかありません。ビルド中に容量不足で止まる可能性が高いです（推奨 $RecommendedFreeSpaceGB GB 以上）。")
} elseif ($FreeGB -lt $RecommendedFreeSpaceGB) {
    $Warnings.Add("$($Drive.Name): ドライブの空き容量が $FreeGB GB です（推奨 $RecommendedFreeSpaceGB GB 以上）。Android のビルドで容量が足りなくなると、Library を消して作り直すため時間がかかります。")
} else {
    Write-Ok "空き容量 $FreeGB GB（$($Drive.Name): ドライブ）"
}

# --- Unity とモジュール ---------------------------------------------------------
$ProjectVersionFile = Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt"
$UnityVersion = $null
if (Test-Path -LiteralPath $ProjectVersionFile) {
    $VersionMatch = [regex]::Match((Get-Content -LiteralPath $ProjectVersionFile -Raw -Encoding UTF8), 'm_EditorVersion:\s*(\S+)')
    if ($VersionMatch.Success) { $UnityVersion = $VersionMatch.Groups[1].Value }
}
if ($UnityVersion) {
    $EditorRoots = @("C:\Program Files\Unity\Hub\Editor")
    $UnityPathsFile = Join-Path $ProjectRoot ".github\unity-paths.json"
    if (Test-Path -LiteralPath $UnityPathsFile) {
        $EditorRoots = @((Get-Content -LiteralPath $UnityPathsFile -Raw -Encoding UTF8 | ConvertFrom-Json).Windows.editorRoots)
    }
    $EditorDir = $EditorRoots | ForEach-Object { Join-Path $_ $UnityVersion } |
        Where-Object { Test-Path -LiteralPath (Join-Path $_ "Editor\Unity.exe") } | Select-Object -First 1
    if (-not $EditorDir) {
        $Warnings.Add("Unity $UnityVersion が見つかりません。ビルドは失敗します。Unity Hub でインストールしてください。")
    } else {
        $PlaybackEngines = Join-Path $EditorDir "Editor\Data\PlaybackEngines"
        $MissingModules = @(
            @{ Name = "Android Build Support"; Dir = "AndroidPlayer" },
            @{ Name = "Windows Build Support (IL2CPP)"; Dir = "windowsstandalonesupport" }
        ) | Where-Object { -not (Test-Path -LiteralPath (Join-Path $PlaybackEngines $_.Dir)) } | ForEach-Object { $_.Name }
        if ($MissingModules) {
            $Warnings.Add("Unity のモジュールが足りません: $($MissingModules -join ', ')。そのプラットフォームのビルドは失敗します。")
        } else {
            Write-Ok "Unity $UnityVersion とモジュール"
        }
    }
}

# --- 作業フォルダのプロジェクトを Unity で開いていないか ---------------------------
$UnityOnWorkProject = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -and $_.CommandLine.IndexOf($WorkProjectDir, [StringComparison]::OrdinalIgnoreCase) -ge 0 }
if ($UnityOnWorkProject) {
    $Warnings.Add("ランナーの作業フォルダのプロジェクトを Unity が開いています（PID: $(($UnityOnWorkProject.ProcessId) -join ', ')）。ビルドがプロジェクトを開けずに失敗するので、閉じてください。")
} else {
    Write-Ok "作業フォルダのプロジェクトは Unity で開かれていない"
}

# --- 電源とスリープ -------------------------------------------------------------
$Battery = Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue | Select-Object -First 1
if ($Battery -and $Battery.BatteryStatus -eq 1) {
    $Warnings.Add("バッテリーで動いています（残り $($Battery.EstimatedChargeRemaining)%）。ビルドは長時間 CPU を使うので、電源につないでください。")
}
# powercfg の表示は言語によって変わるため、値の行（AC、DC の順）だけを読む
$SleepValues = @(powercfg /q SCHEME_CURRENT SUB_SLEEP STANDBYIDLE 2> $null |
    ForEach-Object { if ($_ -match ':\s*0x([0-9a-fA-F]{8})\s*$') { [Convert]::ToInt32($Matches[1], 16) } })
if ($SleepValues.Count -ge 2) {
    $SleepSeconds = if ($Battery -and $Battery.BatteryStatus -eq 1) { $SleepValues[-1] } else { $SleepValues[-2] }
    if ($SleepSeconds -gt 0) {
        $Warnings.Add("$([Math]::Round($SleepSeconds / 60)) 分操作しないとスリープする設定です。スリープするとビルドが止まります。長いビルド（Android は最大3時間）の間は、設定 → システム → 電源 でスリープを「なし」にしてください。")
    } else {
        Write-Ok "スリープしない設定"
    }
}

# --- GitHub への接続 ------------------------------------------------------------
try {
    Invoke-WebRequest -Uri "https://api.github.com/zen" -UseBasicParsing -TimeoutSec 10 | Out-Null
    Write-Ok "GitHub に接続できる"
} catch {
    $Warnings.Add("GitHub（api.github.com）に接続できません。ネットワークを確認してください: $($_.Exception.Message)")
}

# --- 前回の止まり方 -------------------------------------------------------------
$LastLog = Get-ChildItem -Path (Join-Path $RunnerDir "_diag") -Filter "Runner_*.log" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($LastLog -and (Select-String -LiteralPath $LastLog.FullName -Pattern "Runner will be shutdown for UserCancelled" -SimpleMatch -Quiet)) {
    $Warnings.Add("前回は Ctrl+C またはウィンドウを閉じる操作でランナーが止められていました（$($LastLog.LastWriteTime.ToString('M/d HH:mm'))）。ビルド中だった場合、そのビルドはキャンセルされています。")
}

# --- 結果 ---------------------------------------------------------------------
Write-Host ""
if ($Errors.Count -gt 0) {
    foreach ($Message in $Errors) { Write-Host "[エラー] $Message" -ForegroundColor Red }
    foreach ($Message in $Warnings) { Write-Host "[警告] $Message" -ForegroundColor Yellow }
    Write-Host "ランナーを起動できません。" -ForegroundColor Red
    exit 1
}

if ($Warnings.Count -gt 0) {
    foreach ($Message in $Warnings) { Write-Host "[警告] $Message" -ForegroundColor Yellow }
    Write-Host ""
    if (-not $NoPrompt) {
        $Answer = Read-Host "警告があります。このままランナーを起動しますか？ [Y/n]"
        if ($Answer -match '^(n|no)$') {
            Write-Host "起動を取りやめました。"
            exit 1
        }
    }
} else {
    Write-Host "問題は見つかりませんでした。" -ForegroundColor Green
}

Write-Host ""
Write-Host "ランナーを起動します。このウィンドウがランナーになります。" -ForegroundColor Cyan
Write-Host "  - Ctrl+C を押したり、ウィンドウを閉じたりしないでください（実行中のビルドがキャンセルされます）。" -ForegroundColor Cyan
Write-Host "  - 操作は別のウィンドウで BuildRunner\runner-menu.bat から行えます。" -ForegroundColor Cyan
Write-Host ""
exit 0
