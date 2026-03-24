# スクリプトの1つ上のディレクトリ（プロジェクトルート）
$targetDir = Join-Path $PSScriptRoot ".."

# フルパス化
$targetDir = Resolve-Path $targetDir

# ディレクトリ移動
Set-Location $targetDir

# gemini コマンド実行
gemini