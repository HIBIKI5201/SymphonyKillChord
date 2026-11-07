<#
.SYNOPSIS
  このPCを SymphonyKillChord の自動ビルド用セルフホストランナーにする。

.DESCRIPTION
  BuildRunner フォルダの中にランナーを作る。Windows と macOS（PowerShell 7 で実行）に対応する。
  手順は次のとおりで、各段階で状況を表示する。
    1. ビルドに必要な環境（Unity・モジュール・gh・空き容量）を確認し、足りないものは入れ方を案内する
    2. GitHub Actions のランナー本体（公式リリース）をダウンロードし、SHA-256 を照合して展開する
    3. 登録トークンを用意する（gh で発行できなければ、オーナーから受け取ったものを貼り付ける）
    4. ランナーとして登録する（作業フォルダは BuildRunner/_work）
    5. 必要ならそのまま起動する
  詳しくは README.md を参照。

.EXAMPLE
  .\BuildRunner\setup-runner.ps1                 # 対話しながらセットアップする
  .\BuildRunner\setup-runner.ps1 -Token XXXXX    # 受け取った登録トークンを渡してセットアップする
  .\BuildRunner\setup-runner.ps1 -Remove         # このPCのランナー登録を外す
  .\BuildRunner\setup-runner.ps1 -WorkDir D:\RunnerWork   # 作業フォルダを別の場所にする
  pwsh ./BuildRunner/setup-runner.ps1              # macOS で対話しながらセットアップする
#>
param(
    [string]$Token,
    [string]$RunnerName = $(if ($env:COMPUTERNAME) { $env:COMPUTERNAME } else { [Environment]::MachineName }),
    [string]$WorkDir,
    [switch]$Remove,
    [switch]$SkipChecks
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$Repo = "HIBIKI5201/SymphonyKillChord"
$RunnerDir = $PSScriptRoot
$ProjectRoot = Split-Path -Parent $RunnerDir
$MinimumFreeSpaceGB = 80
. (Join-Path $RunnerDir "RunnerPlatform.ps1")
$ConfigScript = Join-Path $RunnerDir (Get-RunnerScriptName "config")

function Write-Step([string]$Message) {
    Write-Host ""
    Write-Host "==== $Message ====" -ForegroundColor Cyan
}

function Confirm-Yes([string]$Question) {
    $Answer = Read-Host "$Question [y/N]"
    return $Answer -match '^(y|yes)$'
}

function Test-GhAvailable {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { return $false }
    & gh auth status *> $null
    return $LASTEXITCODE -eq 0
}

# gh で登録用・削除用のトークンを発行する。リポジトリの管理者でなければ失敗するので $null を返す。
function New-RunnerToken([string]$Kind) {
    if (-not (Test-GhAvailable)) { return $null }
    $Issued = & gh api -X POST "repos/$Repo/actions/runners/$Kind" --jq ".token" 2> $null
    if ($LASTEXITCODE -ne 0 -or -not $Issued) { return $null }
    return "$Issued".Trim()
}

function Read-RunnerToken([string]$Kind) {
    Write-Host "トークンを自動で発行できませんでした（gh が未ログイン、またはリポジトリの管理者ではない）。"
    Write-Host "リポジトリのオーナーに、次のコマンドでトークンを発行してもらってください（有効期限は1時間）。"
    Write-Host ""
    Write-Host "  gh api -X POST repos/$Repo/actions/runners/$Kind --jq .token" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "またはブラウザで Settings → Actions → Runners → New self-hosted runner の画面に出る --token の値でもよい。"
    $Secure = Read-Host "受け取ったトークンを貼り付けてください" -AsSecureString
    $Plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secure))
    return "$Plain".Trim()
}

# ---------------------------------------------------------------------------
# 登録を外す
# ---------------------------------------------------------------------------
if ($Remove) {
    Write-Step "このPCのランナー登録を外す"
    if (-not (Test-Path -LiteralPath (Join-Path $RunnerDir ".runner"))) {
        Write-Host "このフォルダにはランナーが登録されていません。"
        return
    }
    if (Get-Process -Name "Runner.Listener" -ErrorAction SilentlyContinue) {
        Write-Host "ランナーが起動中です。先にランナーのウィンドウを閉じてから実行してください。" -ForegroundColor Red
        exit 1
    }
    $RemoveToken = if ($Token) { $Token } else { New-RunnerToken "remove-token" }
    if (-not $RemoveToken) { $RemoveToken = Read-RunnerToken "remove-token" }
    & $ConfigScript remove --token $RemoveToken
    if ($LASTEXITCODE -ne 0) {
        Write-Host "登録の解除に失敗しました（終了コード $LASTEXITCODE）。" -ForegroundColor Red
        exit 1
    }
    Write-Host "登録を外しました。ランナー本体（bin など）を消す場合は、BuildRunner の中の管理対象外のファイルを手で削除してください。"
    return
}

# ---------------------------------------------------------------------------
# はじめに
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "SymphonyKillChord のビルド用ランナーをセットアップします。" -ForegroundColor Cyan
Write-Host @"

【ランナーとは】
  GitHub で develop へのマージなどがあったときに、このPCが代わりに Unity のビルドを
  実行してリリースを作る仕組みです。ランナーを起動している間だけ、このPCがビルドを引き受けます。
  ビルド中は CPU を多く使います（自動ビルドは CPU の 50%。実行中に管理メニューで変更できる）。

【このスクリプトがやること】
  1. ビルドに必要なもの（Unity とモジュール / gh / 空き容量）がそろっているか確認し、足りなければ入れ方を案内する
  2. GitHub 公式のランナー本体をダウンロードし、改ざんされていないか（SHA-256）を確かめて展開する
  3. 登録トークン（このPCをランナーとして登録するための1時間だけ有効な鍵）を用意する
  4. ランナーとして登録する（ランナー名はPC名 $RunnerName）
  5. ランナーを起動する

  途中で止めたいときは Ctrl+C で中断できます（何度でもやり直せます）。
"@
if (-not (Confirm-Yes "始めますか？")) { return }

# winget（macOS は Homebrew）でのインストールを提案する。入れた直後は PATH が反映されないことがあるので、再起動を案内する。
function Install-WithWinget([string]$Name, [string]$WingetId, [string]$BrewName) {
    if ($RunnerIsMac) {
        if (-not (Get-Command brew -ErrorAction SilentlyContinue)) {
            Write-Host "  Homebrew が使えないため、自動でインストールできません。$Name を手動でインストールしてください。"
            return $false
        }
        if (-not (Confirm-Yes "  $Name を Homebrew でインストールしますか？")) { return $false }
        & brew install $BrewName
        return $true
    }
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Write-Host "  winget が使えないため、自動でインストールできません。$Name を手動でインストールしてください。"
        return $false
    }
    if (-not (Confirm-Yes "  $Name を winget でインストールしますか？")) { return $false }
    & winget install --id $WingetId -e --source winget --accept-package-agreements --accept-source-agreements
    $env:Path = [Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [Environment]::GetEnvironmentVariable("Path", "User")
    return $true
}

# ---------------------------------------------------------------------------
# 1. 環境の確認
# ---------------------------------------------------------------------------
Write-Step "1/5 ビルドに必要な環境を確認する"
$Problems = @()

# git: リポジトリをクローンしている前提なので、コマンドが使えるかだけ確かめる
if (Get-Command git -ErrorAction SilentlyContinue) {
    Write-Host "[OK] git"
} else {
    $Problems += "git コマンドが見つかりません（PATH が通っているか確認してください）。"
}

# gh: 登録トークンの発行や、管理メニューでのビルド状況の表示に使う（なくてもよい）
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "[--] gh（GitHub CLI）が見つかりません。なくてもセットアップはできますが、管理メニューでビルド状況を見るのに使います。"
    Install-WithWinget "GitHub CLI" "GitHub.cli" "gh" | Out-Null
}
if ((Get-Command gh -ErrorAction SilentlyContinue) -and -not (Test-GhAvailable)) {
    Write-Host "[--] gh が GitHub にログインしていません。"
    if (Confirm-Yes "  今 gh auth login でログインしますか？（ブラウザが開きます）") {
        & gh auth login --web --git-protocol https
    }
}
if (Test-GhAvailable) {
    Write-Host "[OK] gh（ログイン済み）"
}

# Unity: プロジェクトと同じバージョンと、Android / Windows 向けのモジュールが必要
$UnityVersion = $null
$UnityRevision = $null
$ProjectVersionFile = Join-Path $ProjectRoot "ProjectSettings/ProjectVersion.txt"
if (Test-Path -LiteralPath $ProjectVersionFile) {
    $VersionText = Get-Content -LiteralPath $ProjectVersionFile -Raw -Encoding UTF8
    $VersionMatch = [regex]::Match($VersionText, 'm_EditorVersionWithRevision:\s*(\S+)\s*\((\w+)\)')
    if ($VersionMatch.Success) {
        $UnityVersion = $VersionMatch.Groups[1].Value
        $UnityRevision = $VersionMatch.Groups[2].Value
    }
}
if (-not $UnityVersion) {
    $Problems += "ProjectSettings\ProjectVersion.txt から Unity のバージョンを読めませんでした。BuildRunner をリポジトリの中で実行しているか確認してください。"
} else {
    $EditorRoots = Get-RunnerEditorRoots $ProjectRoot
    $EditorDir = Find-RunnerEditorDir $EditorRoots $UnityVersion
    $UnityHubLink = "unityhub://$UnityVersion/$UnityRevision"
    $RequiredModules = @(Get-RunnerRequiredModules)
    if (-not $EditorDir) {
        Write-Host "[NG] Unity $UnityVersion が見つかりません（探した場所: $($EditorRoots -join ', ')）。" -ForegroundColor Yellow
        Write-Host "  Unity Hub でこのバージョンを入れるときは、モジュールで次の2つにチェックを入れてください:"
        $RequiredModules | ForEach-Object { Write-Host "    - $($_.Name)" }
        if (-not $RunnerIsMac) {
            Write-Host "    （Android Build Support は OpenJDK、Android SDK & NDK Tools も含める）"
        }
        if (Confirm-Yes "  Unity Hub でこのバージョンのインストール画面を開きますか？（Unity Hub が必要）") {
            if ($RunnerIsMac) { & open $UnityHubLink } else { Start-Process $UnityHubLink }
        }
        $Problems += "Unity $UnityVersion をインストールしてから、このスクリプトをもう一度実行してください。"
    } else {
        Write-Host "[OK] Unity $UnityVersion ($EditorDir)"
        $MissingModules = @(Get-RunnerMissingModules $EditorDir)
        foreach ($Module in $RequiredModules) {
            if ($MissingModules -contains $Module.Name) {
                Write-Host "[NG] Unity のモジュール「$($Module.Name)」がありません。" -ForegroundColor Yellow
                $Problems += "Unity Hub → インストール → $UnityVersion の歯車 → 「モジュールを加える」から「$($Module.Name)」を追加してください。"
            } else {
                Write-Host "[OK] $($Module.Name)"
            }
        }
    }
}

# Xcode: macOS と iOS のビルド（IL2CPP のコンパイル）に、Xcode とそのコマンドラインツールが要る
if ($RunnerIsMac) {
    & xcode-select -p *> $null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Xcode のコマンドラインツール"
    } else {
        Write-Host "[NG] Xcode のコマンドラインツールが見つかりません。" -ForegroundColor Yellow
        $Problems += "App Store から Xcode を入れて一度起動し、ターミナルで xcode-select --install を実行してください。"
    }
}

# 空き容量: Library とビルド成果物で数十GB使う
$FreeSpace = Get-RunnerFreeSpace $RunnerDir
$FreeGB = $FreeSpace.FreeGB
if ($FreeGB -lt $MinimumFreeSpaceGB) {
    Write-Host "[NG] $($FreeSpace.Label)の空き容量が $FreeGB GB です。" -ForegroundColor Yellow
    $Problems += "$($FreeSpace.Label)を $MinimumFreeSpaceGB GB 以上空けてください（Unity の Library とビルド成果物で数十GB使う）。"
} else {
    Write-Host "[OK] 空き容量 $FreeGB GB（$($FreeSpace.Label)）"
}

Write-Host ""
Write-Host "※ Unity のライセンスは自動で確認できません。Unity Hub にサインインし、" -ForegroundColor Yellow
Write-Host "   「ライセンス」に有効なライセンス（Personal でよい）があることを確かめてください。" -ForegroundColor Yellow

if ($Problems.Count -gt 0) {
    Write-Host ""
    Write-Host "まだ準備が足りないものがあります:" -ForegroundColor Yellow
    $Problems | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    if (-not $SkipChecks -and -not (Confirm-Yes "このまま続けますか？（登録はできるが、ビルドは失敗する）")) {
        Write-Host "準備ができたら、もう一度このスクリプトを実行してください。"
        exit 1
    }
}

if (Test-Path -LiteralPath (Join-Path $RunnerDir ".runner")) {
    Write-Host ""
    Write-Host "このフォルダはすでにランナーとして登録されています。作り直す場合は、先に -Remove で登録を外してください。" -ForegroundColor Yellow
    exit 1
}

# ---------------------------------------------------------------------------
# 2. ランナー本体のダウンロードと照合
# ---------------------------------------------------------------------------
Write-Step "2/5 ランナー本体をダウンロードしてハッシュ値を照合する"
$ListenerName = if ($RunnerIsMac) { "Runner.Listener" } else { "Runner.Listener.exe" }
if (Test-Path -LiteralPath (Join-Path (Join-Path $RunnerDir "bin") $ListenerName)) {
    Write-Host "ランナー本体はすでに展開済みのため、ダウンロードを省略します。"
} else {
    $Release = Invoke-RestMethod -Uri "https://api.github.com/repos/actions/runner/releases/latest" -Headers @{ "User-Agent" = "SymphonyKillChord-setup-runner" }
    $Version = "$($Release.tag_name)".TrimStart("v")
    # ランナー本体は OS と CPU ごとに別のファイル（Windows は zip、macOS は tar.gz）
    if ($RunnerIsMac) {
        $Arch = "$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)"
        $Platform = if ($Arch -eq "Arm64") { "osx-arm64" } else { "osx-x64" }
        $AssetName = "actions-runner-$Platform-$Version.tar.gz"
    } else {
        $Platform = "win-x64"
        $AssetName = "actions-runner-$Platform-$Version.zip"
    }
    $Asset = $Release.assets | Where-Object { $_.name -eq $AssetName } | Select-Object -First 1
    $HashMatch = [regex]::Match("$($Release.body)", "<!-- BEGIN SHA $Platform -->([0-9a-fA-F]{64})<!-- END SHA $Platform -->")
    if (-not $Asset -or -not $HashMatch.Success) {
        Write-Host "リリース $($Release.tag_name) から $AssetName またはそのハッシュ値を見つけられませんでした。" -ForegroundColor Red
        exit 1
    }
    $ExpectedHash = $HashMatch.Groups[1].Value.ToLowerInvariant()
    $ZipPath = Join-Path ([System.IO.Path]::GetTempPath()) $AssetName
    Write-Host "ダウンロード中: $AssetName（$([Math]::Round($Asset.size / 1MB)) MB）"
    Invoke-WebRequest -Uri $Asset.browser_download_url -OutFile $ZipPath -UseBasicParsing
    $ActualHash = (Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($ActualHash -ne $ExpectedHash) {
        Remove-Item -LiteralPath $ZipPath -Force
        Write-Host "ハッシュ値が一致しません。ダウンロードしたファイルは削除しました。" -ForegroundColor Red
        Write-Host "  期待値: $ExpectedHash"
        Write-Host "  実際  : $ActualHash"
        exit 1
    }
    Write-Host "[OK] SHA-256 が一致しました（$ExpectedHash）"
    if ($RunnerIsMac) {
        & tar -xzf $ZipPath -C $RunnerDir
        if ($LASTEXITCODE -ne 0) {
            Write-Host "展開に失敗しました（終了コード $LASTEXITCODE）。" -ForegroundColor Red
            exit 1
        }
    } else {
        Expand-Archive -LiteralPath $ZipPath -DestinationPath $RunnerDir -Force
    }
    Remove-Item -LiteralPath $ZipPath -Force
    Write-Host "[OK] $RunnerDir に展開しました（ランナー $($Release.tag_name)）"
}

# ---------------------------------------------------------------------------
# 3. 登録トークン
# ---------------------------------------------------------------------------
Write-Step "3/5 登録トークンを用意する"
if (-not $Token) {
    $Token = New-RunnerToken "registration-token"
    if ($Token) {
        Write-Host "[OK] gh で登録トークンを発行しました。"
    } else {
        $Token = Read-RunnerToken "registration-token"
    }
}
if (-not $Token) {
    Write-Host "登録トークンがないため中止します。" -ForegroundColor Red
    exit 1
}

# ---------------------------------------------------------------------------
# 4. 登録
# ---------------------------------------------------------------------------
Write-Step "4/5 ランナーとして登録する"
# ビルド先のパスが長くなっても、ワークフローが短い別名（ジャンクション）から Unity を起動するので、
# 作業フォルダの場所は気にしなくてよい。
if (-not $WorkDir) { $WorkDir = "_work" }
$WorkFolderPath = if ([System.IO.Path]::IsPathRooted($WorkDir)) { $WorkDir } else { Join-Path $RunnerDir $WorkDir }
Write-Host "リポジトリ: $Repo"
Write-Host "ランナー名: $RunnerName"
Write-Host "作業フォルダ: $WorkFolderPath"
Push-Location $RunnerDir
try {
    # ラベルは既定（self-hosted / Windows または macOS / X64 または ARM64）のまま。
    # ワークフローは runs-on: [self-hosted, windows] と [self-hosted, macOS] で、OS ごとに担当するビルドを割り当てる。
    & $ConfigScript --unattended --url "https://github.com/$Repo" --token $Token --name $RunnerName --work $WorkDir --replace
    if ($LASTEXITCODE -ne 0) {
        Write-Host "登録に失敗しました（終了コード $LASTEXITCODE）。トークンの期限（1時間）が切れていないか確認してください。" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}
Write-Host "[OK] 登録しました。"

# ---------------------------------------------------------------------------
# 5. 起動
# ---------------------------------------------------------------------------
Write-Step "5/5 起動"
Write-Host "次回からは BuildRunner の $(Get-RunnerEntryName "start-runner") で起動し、操作は $(Get-RunnerEntryName "runner-menu")（管理メニュー）から行えます。"
if ($RunnerIsMac) {
    Write-Host "macOS ランナーにビルドを割り当てるには、リポジトリ変数 MACOS_RUNNER_ENABLED を true にしてもらってください（README.md「OSごとの担当」）。" -ForegroundColor Yellow
}
if (Confirm-Yes "今すぐランナーを起動しますか？（別ウィンドウで開く）") {
    Start-RunnerEntryWindow (Join-Path $RunnerDir (Get-RunnerEntryName "start-runner"))
    Write-Host "別ウィンドウで起動前チェックを行い、ランナーを起動します。そのウィンドウでは Ctrl+C を押さず、閉じないでください（ランナーが止まり、実行中のビルドがキャンセルされる）。"
}
