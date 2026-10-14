<#
.SYNOPSIS
    クローン後の初期セットアップを行う。

.DESCRIPTION
    Unity を開く前に、次を確かめて、直せるものは直す。
    - クローン先のパス（日本語・空白が無いか）とフォルダ名（SymphonyKillChord か）
    - git の設定（submodule.recurse / core.longpaths）
    - サブモジュールの取得（閲覧権限が無ければ招待を頼むよう案内する）
    - Unity エディタのバージョンと Android Build Support
    - Unity CLI（unity コマンド。無ければ公式のスクリプトで入れる）
    必須の項目がすべて通ったら、UserSettings/KillChord/ProjectSetupState.json に記録する。
    Unity はこの記録を起動時に読み、無ければ警告ウィンドウを出す。
    手順を増やしたら project-setup.json の SetupVersion を上げる。既存のメンバーにも再実行を促せる。
    macOS / Linux 用の Setup-Project.sh（入口は Setup.command）も同じ手順にそろえる。

.PARAMETER Check
    確認だけを行い、何も変更しない。

.EXAMPLE
    ./PowerShell/ProjectSetup/Setup-Project.ps1
    ./PowerShell/ProjectSetup/Setup-Project.ps1 -Check
#>
[CmdletBinding()]
param(
    [switch]$Check
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$Config = Get-Content (Join-Path $PSScriptRoot 'project-setup.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$StatePath = Join-Path $RepoRoot 'UserSettings\KillChord\ProjectSetupState.json'
$Results = New-Object System.Collections.Generic.List[object]

# 状態: OK / Fixed（直した）/ Warn（推奨）/ Info（役職によって必要）/ Error（必須で未完了）
function Add-Result([string]$Name, [string]$Status, [string]$Message) {
    $Results.Add([pscustomobject]@{ Name = $Name; Status = $Status; Message = $Message })
    $color = switch ($Status) {
        'OK' { 'Green' } 'Fixed' { 'Cyan' } 'Warn' { 'Yellow' } 'Info' { 'Gray' } default { 'Red' }
    }
    Write-Host ("[{0,-5}] {1}: {2}" -f $Status, $Name, $Message) -ForegroundColor $color
}

# git をリポジトリのルートで実行し、終了コードと出力を返す。
function Invoke-Git([string[]]$Arguments) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = & git -C $RepoRoot @Arguments 2>&1 | ForEach-Object { "$_" }
    $code = $LASTEXITCODE
    $ErrorActionPreference = $previous
    return [pscustomobject]@{ ExitCode = $code; Output = ($output -join "`n").Trim() }
}

Write-Host "Symphony Kill Chord プロジェクトセットアップ（SetupVersion $($Config.SetupVersion)）"
if ($Check) { Write-Host '確認だけを行います（-Check）。何も変更しません。' }
Write-Host "リポジトリ: $RepoRoot"
Write-Host ''

# --- git -------------------------------------------------------------------
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Add-Result 'git' 'Error' 'git が見つかりません。Git for Windows を入れてから再実行してください。'
    exit 1
}

# --- クローン先のパス ------------------------------------------------------
# Unity のビルドや一部のツールが、日本語や空白を含むパスの解決に失敗するため。
if ($RepoRoot -match '[^\x21-\x7E\\:]') {
    Add-Result 'クローン先のパス' 'Warn' "日本語か空白が含まれています（$RepoRoot）。ビルドやツールが失敗する原因になるので、英数字だけのパスへクローンし直すことを勧めます。"
}
else {
    Add-Result 'クローン先のパス' 'OK' $RepoRoot
}

# --- クローン先のフォルダ名 ------------------------------------------------
# Unity と IDE はフォルダ名からソリューション（<フォルダ名>.slnx）を作る。
# 名前が違うと別名のソリューションができ、誤ってコミットされて二重になるため、名前を揃える。
$folderName = Split-Path $RepoRoot -Leaf
$expectedFolderName = $Config.RepositoryFolderName
if ($folderName -ceq $expectedFolderName) {
    Add-Result 'クローン先のフォルダ名' 'OK' $folderName
}
else {
    Add-Result 'クローン先のフォルダ名' 'Error' "フォルダ名が '$folderName' です。Unity と IDE を閉じてから、フォルダ名を '$expectedFolderName' に変えてください（変えたあと、古い名前の .sln / .slnx は消してよい）。"
}

# --- git の設定 ------------------------------------------------------------
# submodule.recurse: pull や checkout のときにサブモジュールも追従させる。
# core.longpaths: Windows で 260 文字を超えるパスを扱えるようにする。
$gitSettings = [ordered]@{ 'submodule.recurse' = 'true'; 'core.longpaths' = 'true' }
foreach ($key in $gitSettings.Keys) {
    $expected = $gitSettings[$key]
    $current = (Invoke-Git @('config', '--local', '--get', $key)).Output
    if ($current -eq $expected) {
        Add-Result "git config $key" 'OK' $expected
    }
    elseif ($Check) {
        Add-Result "git config $key" 'Error' "未設定です（現在: '$current'）。"
    }
    else {
        [void](Invoke-Git @('config', '--local', $key, $expected))
        Add-Result "git config $key" 'Fixed' "$expected に設定しました。"
    }
}

# --- サブモジュール --------------------------------------------------------
$submodulePaths = (Invoke-Git @('config', '-f', '.gitmodules', '--get-regexp', '^submodule\..*\.path$')).Output -split "`n" |
    Where-Object { $_ } | ForEach-Object { ($_ -split ' ', 2)[1] }
foreach ($path in $submodulePaths) {
    $status = (Invoke-Git @('submodule', 'status', '--', $path)).Output
    $fullPath = Join-Path $RepoRoot $path
    $hasContent = (Test-Path $fullPath) -and ((Get-ChildItem $fullPath -Force | Measure-Object).Count -gt 0)
    if ($status -and -not $status.StartsWith('-') -and $hasContent) {
        Add-Result "サブモジュール $path" 'OK' '取得済みです。'
        continue
    }
    if ($Check) {
        Add-Result "サブモジュール $path" 'Error' 'まだ取得していません。'
        continue
    }

    # 仕様書のように本体の記録を気にせず最新を読むもの（ignore = all）は、ブランチの最新を取る。
    $name = (Invoke-Git @('config', '-f', '.gitmodules', '--get-regexp', '^submodule\..*\.path$')).Output -split "`n" |
        Where-Object { $_ -match " $([regex]::Escape($path))$" } | ForEach-Object { ($_ -split ' ', 2)[0] -replace '\.path$', '' }
    $ignore = (Invoke-Git @('config', '-f', '.gitmodules', '--get', "$name.ignore")).Output
    $arguments = @('submodule', 'update', '--init', '--recursive')
    if ($ignore -eq 'all') { $arguments += @('--remote', '--depth', '1') }
    $arguments += @('--', $path)

    # 大きなサブモジュールは時間がかかるので、git の進捗表示をそのまま見せる。
    Write-Host "  $path を取得しています..."
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & git -C $RepoRoot @arguments
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $previous
    if ($exitCode -eq 0) {
        Add-Result "サブモジュール $path" 'Fixed' '取得しました。'
    }
    else {
        Add-Result "サブモジュール $path" 'Error' '取得できませんでした。上に出た git のエラーを確認してください。認証や「Repository not found」のエラーなら、非公開リポジトリの閲覧権限がありません。リードに GitHub の招待を頼んでください。'
    }
}

# --- Unity エディタ --------------------------------------------------------
$versionText = Get-Content (Join-Path $RepoRoot 'ProjectSettings\ProjectVersion.txt') -Raw
$unityVersion = [regex]::Match($versionText, 'm_EditorVersion:\s*(\S+)').Groups[1].Value
$unityRevision = [regex]::Match($versionText, 'm_EditorVersionWithRevision:\s*\S+\s*\((\w+)\)').Groups[1].Value
$hubInstallUri = "unityhub://$unityVersion/$unityRevision"

# Unity Hub の既定のインストール先と、Hub で変更したインストール先を探す。
$editorRoots = New-Object System.Collections.Generic.List[string]
$editorRoots.Add((Join-Path $env:ProgramFiles 'Unity\Hub\Editor'))
$secondaryPathFile = Join-Path $env:APPDATA 'UnityHub\secondaryInstallPath.json'
if (Test-Path $secondaryPathFile) {
    $secondary = (Get-Content $secondaryPathFile -Raw | ConvertFrom-Json)
    if ($secondary) { $editorRoots.Add($secondary) }
}
$editorPath = $editorRoots | ForEach-Object { Join-Path $_ $unityVersion } |
    Where-Object { Test-Path (Join-Path $_ 'Editor\Unity.exe') } | Select-Object -First 1

if (-not $editorPath) {
    Add-Result "Unity $unityVersion" 'Error' "インストールされていません。Unity Hub で $hubInstallUri を開き、Android Build Support を付けて入れてください。"
}
else {
    Add-Result "Unity $unityVersion" 'OK' $editorPath
    # ビルドターゲットが Android なので、ビルドしない役職でも無いとプロジェクトを開いたときに警告が出る。
    if (Test-Path (Join-Path $editorPath 'Editor\Data\PlaybackEngines\AndroidPlayer')) {
        Add-Result 'Android Build Support' 'OK' '入っています。'
    }
    else {
        Add-Result 'Android Build Support' 'Error' "入っていません。Unity Hub の「インストール」で $unityVersion の歯車 →「モジュールを加える」から入れてください。"
    }
}

# --- Unity CLI --------------------------------------------------------------
# AI エージェントが Unity を操作するのに使う（エディタの操作は com.unity.pipeline 経由）。
# 公式のインストールスクリプトで、ユーザーのフォルダ（%LOCALAPPDATA%\Unity\bin）に入る。
if (Get-Command unity -ErrorAction SilentlyContinue) {
    Add-Result 'Unity CLI' 'OK' "$(& unity --version 2>$null)"
}
elseif ($Check) {
    Add-Result 'Unity CLI' 'Error' '入っていません。'
}
else {
    Write-Host '  Unity CLI を入れています...'
    try {
        $env:UNITY_CLI_CHANNEL = 'beta'
        Invoke-RestMethod 'https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1' | Invoke-Expression
        # インストーラーはユーザーの PATH に足すだけなので、このセッションでも使えるよう読み直す。
        $env:Path = [Environment]::GetEnvironmentVariable('Path', 'User') + ';' + [Environment]::GetEnvironmentVariable('Path', 'Machine')
        if (Get-Command unity -ErrorAction SilentlyContinue) {
            Add-Result 'Unity CLI' 'Fixed' "$(& unity --version 2>$null) を入れました。新しく開いたターミナルから unity コマンドを使えます。"
        }
        else {
            Add-Result 'Unity CLI' 'Error' '入れましたが unity コマンドが見つかりません。ターミナルを開き直してから、もう一度実行してください。'
        }
    }
    catch {
        Add-Result 'Unity CLI' 'Error' "入れられませんでした（$($_.Exception.Message)）。ネットワークを確かめて、もう一度実行してください。"
    }
}

# --- 役職によって必要なもの ------------------------------------------------
$sdks = if (Get-Command dotnet -ErrorAction SilentlyContinue) { & dotnet --list-sdks 2>$null } else { @() }
if ($sdks | Where-Object { $_ -match '^10\.' }) {
    Add-Result '.NET SDK 10' 'OK' 'SinfoniaOperator のツールをビルドできます。'
}
else {
    Add-Result '.NET SDK 10' 'Info' 'SinfoniaOperator のツールを自分でビルドするプログラマーだけ必要です。'
}

# --- まとめ ----------------------------------------------------------------
Write-Host ''
$errors = @($Results | Where-Object { $_.Status -eq 'Error' })
if ($errors.Count -gt 0) {
    Write-Host "未完了の必須項目が $($errors.Count) 件あります。上の [Error] の案内に従って直し、もう一度実行してください。" -ForegroundColor Red
    exit 1
}

if (-not $Check) {
    New-Item -ItemType Directory -Force (Split-Path $StatePath) | Out-Null
    $state = [ordered]@{
        SetupVersion   = [int]$Config.SetupVersion
        CompletedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        UnityVersion   = $unityVersion
    }
    $json = $state | ConvertTo-Json
    [System.IO.File]::WriteAllText($StatePath, $json, (New-Object System.Text.UTF8Encoding($false)))
}
Write-Host 'セットアップは完了しています。Unity Hub からプロジェクトを開いてください。' -ForegroundColor Green
exit 0
