#!/usr/bin/env bash
# SinfoniaOperator の CLI ツールを、実行中の OS に合わせて起動する。
#   Windows (Git Bash / MSYS / Cygwin): 配布用の <ツール名>.exe をそのまま実行する。
#   macOS / Linux: dotnet run でソースからビルドして実行する（.NET 10 SDK が必要）。
# カレントディレクトリは呼び出し元のまま引き継ぐため、相対パスの引数や設定の解決は exe 実行時と同じになる。
#
# 使い方: ./SinfoniaOperator/run-tool.sh <ツール名> [ツールの引数...]
set -euo pipefail

TOOLS="NotionMarkdownExporter NotionMarkdownWriter DiscordLogExporter"
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

usage() {
    echo "使い方: $0 <ツール名> [引数...]" >&2
    echo "ツール名: ${TOOLS// / | }" >&2
}

tool="${1:-}"
if [[ -z "$tool" || " $TOOLS " != *" $tool "* ]]; then
    usage
    exit 2
fi
shift

case "$(uname -s)" in
    MINGW* | MSYS* | CYGWIN*)
        exec "$script_dir/$tool.exe" "$@"
        ;;
esac

# PATH に無い場合は、公式インストールスクリプトの既定の配置先も探す。
dotnet_cmd="$(command -v dotnet || true)"
for candidate in "${DOTNET_ROOT:-}/dotnet" "$HOME/.dotnet/dotnet" /usr/local/share/dotnet/dotnet; do
    if [[ -z "$dotnet_cmd" && -x "$candidate" ]]; then dotnet_cmd="$candidate"; fi
done
if [[ -z "$dotnet_cmd" ]]; then
    echo "エラー: dotnet が見つかりません。.NET 10 SDK をインストールしてください: https://dotnet.microsoft.com/download" >&2
    exit 1
fi

exec "$dotnet_cmd" run --project "$script_dir/$tool/$tool.csproj" --configuration Release --verbosity quiet -- "$@"
