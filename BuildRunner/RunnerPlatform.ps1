<#
  BuildRunner のスクリプトが共通で使う、OS ごとの違い（Windows / macOS）をまとめたもの。
  setup-runner.ps1 / start-runner.ps1 / runner-menu.ps1 から dot-source（. .\RunnerPlatform.ps1）で読み込む。
  Windows PowerShell 5.1 と PowerShell 7（pwsh）の両方で動くように書く。5.1 には $IsMacOS がないので、
  そのときは Windows として扱う。
#>

$RunnerIsMac = ($IsMacOS -eq $true)
$RunnerOsName = if ($RunnerIsMac) { "macOS" } else { "Windows" }

# ランナー本体のスクリプト名（Windows は .cmd、macOS は .sh）
function Get-RunnerScriptName([string]$BaseName) {
    if ($RunnerIsMac) { return "$BaseName.sh" }
    return "$BaseName.cmd"
}

# ランナーを起動する入口（ダブルクリックで開くファイル）
function Get-RunnerEntryName([string]$BaseName) {
    if ($RunnerIsMac) { return "$BaseName.command" }
    return "$BaseName.bat"
}

# 入口のファイルを新しいウィンドウで開く。macOS は「ターミナル」で開く。
function Start-RunnerEntryWindow([string]$EntryPath) {
    if ($RunnerIsMac) {
        & open -a Terminal $EntryPath
    } else {
        Start-Process -FilePath $EntryPath -WorkingDirectory (Split-Path -Parent $EntryPath)
    }
}

# このOSが担当するビルドと、必要な Unity のモジュール。
# Dir は PlaybackEngines フォルダの中のフォルダ名。Il2Cpp は Variations の中に IL2CPP 版があるかも確かめる。
function Get-RunnerRequiredModules {
    if ($RunnerIsMac) {
        return @(
            @{ Name = "iOS Build Support"; Dir = "iOSSupport" },
            @{ Name = "Mac Build Support (IL2CPP)"; Dir = "MacStandaloneSupport"; Il2Cpp = $true }
        )
    }
    return @(
        @{ Name = "Android Build Support"; Dir = "AndroidPlayer" },
        @{ Name = "Windows Build Support (IL2CPP)"; Dir = "windowsstandalonesupport" }
    )
}

# Unity Hub のエディターの置き場所を .github/unity-paths.json から読む。'~' はホームに展開する。
function Get-RunnerEditorRoots([string]$ProjectRoot) {
    $Roots = if ($RunnerIsMac) { @("/Applications/Unity/Hub/Editor") } else { @("C:\Program Files\Unity\Hub\Editor") }
    $UnityPathsFile = Join-Path $ProjectRoot ".github/unity-paths.json"
    if (Test-Path -LiteralPath $UnityPathsFile) {
        $UnityPaths = Get-Content -LiteralPath $UnityPathsFile -Raw -Encoding UTF8 | ConvertFrom-Json
        $OsPaths = if ($RunnerIsMac) { $UnityPaths.macOS } else { $UnityPaths.Windows }
        if ($OsPaths) { $Roots = @($OsPaths.editorRoots) }
    }
    return @($Roots | ForEach-Object { $_ -replace '^~', $HOME })
}

# 指定バージョンの Unity のフォルダ（<ルート>/<バージョン>）を返す。見つからなければ $null。
function Find-RunnerEditorDir([string[]]$EditorRoots, [string]$UnityVersion) {
    $Executable = if ($RunnerIsMac) { "Unity.app/Contents/MacOS/Unity" } else { "Editor\Unity.exe" }
    return $EditorRoots | ForEach-Object { Join-Path $_ $UnityVersion } |
        Where-Object { Test-Path -LiteralPath (Join-Path $_ $Executable) } | Select-Object -First 1
}

# 足りない Unity のモジュール名を返す。
# Windows は <バージョン>\Editor\Data\PlaybackEngines、macOS は <バージョン>/PlaybackEngines（Unity Hub で足したもの）と
# <バージョン>/Unity.app/Contents/PlaybackEngines（エディターに入っているもの）を探す。
function Get-RunnerMissingModules([string]$EditorDir) {
    $EngineRoots = if ($RunnerIsMac) {
        @((Join-Path $EditorDir "PlaybackEngines"), (Join-Path $EditorDir "Unity.app/Contents/PlaybackEngines"))
    } else {
        @(Join-Path $EditorDir "Editor\Data\PlaybackEngines")
    }
    $Missing = @()
    foreach ($Module in Get-RunnerRequiredModules) {
        $ModuleDir = $EngineRoots | ForEach-Object { Join-Path $_ $Module.Dir } |
            Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
        if ($ModuleDir -and $Module.Il2Cpp) {
            $Il2CppVariation = Get-ChildItem -LiteralPath (Join-Path $ModuleDir "Variations") -Directory -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -match 'il2cpp' } | Select-Object -First 1
            if (-not $Il2CppVariation) { $ModuleDir = $null }
        }
        if (-not $ModuleDir) { $Missing += $Module.Name }
    }
    return $Missing
}

# 指定フォルダがあるドライブ（macOS はボリューム）の名前と空き容量（GB）
function Get-RunnerFreeSpace([string]$Path) {
    $Drive = (Get-Item -LiteralPath $Path).PSDrive
    $Label = if ($RunnerIsMac) { "ディスク" } else { "$($Drive.Name): ドライブ" }
    return @{ Label = $Label; FreeGB = [Math]::Round((Get-PSDrive -Name $Drive.Name).Free / 1GB, 1) }
}

# 作業フォルダのプロジェクトを開いている Unity のプロセス ID
function Get-RunnerUnityOnProject([string]$ProjectDir) {
    if ($RunnerIsMac) {
        return @(& ps -axo "pid=,command=" | Where-Object {
            $_ -match 'Unity\.app/Contents/MacOS/Unity' -and $_.IndexOf($ProjectDir, [StringComparison]::OrdinalIgnoreCase) -ge 0
        } | ForEach-Object { ($_.Trim() -split '\s+', 2)[0] })
    }
    return @(Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine.IndexOf($ProjectDir, [StringComparison]::OrdinalIgnoreCase) -ge 0 } |
        ForEach-Object { $_.ProcessId })
}

# バッテリーで動いていれば残量（%）を、電源につながっていれば $null を返す
function Get-RunnerBatteryPercent {
    if ($RunnerIsMac) {
        $Battery = (& pmset -g batt 2> $null) -join "`n"
        if ($Battery -notmatch "'Battery Power'") { return $null }
        if ($Battery -match '(\d+)%') { return [int]$Matches[1] }
        return 0
    }
    $Battery = Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($Battery -and $Battery.BatteryStatus -eq 1) { return [int]$Battery.EstimatedChargeRemaining }
    return $null
}

# 操作しないとスリープするまでの分数（0 はスリープしない）。読めなければ $null。
function Get-RunnerSleepMinutes([bool]$OnBattery) {
    if ($RunnerIsMac) {
        # pmset -g は今の電源（電源アダプタ / バッテリー）での設定を表示する。値は分。
        $Line = @(& pmset -g 2> $null) | Where-Object { $_ -match '^\s*sleep\s+(\d+)' } | Select-Object -First 1
        if ($Line -and $Line -match '^\s*sleep\s+(\d+)') { return [int]$Matches[1] }
        return $null
    }
    # powercfg の表示は言語によって変わるため、値の行（AC、DC の順）だけを読む
    $SleepValues = @(powercfg /q SCHEME_CURRENT SUB_SLEEP STANDBYIDLE 2> $null |
        ForEach-Object { if ($_ -match ':\s*0x([0-9a-fA-F]{8})\s*$') { [Convert]::ToInt32($Matches[1], 16) } })
    if ($SleepValues.Count -lt 2) { return $null }
    $SleepSeconds = if ($OnBattery) { $SleepValues[-1] } else { $SleepValues[-2] }
    return [Math]::Round($SleepSeconds / 60)
}
