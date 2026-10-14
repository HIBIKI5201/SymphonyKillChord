#!/usr/bin/env bash
# クローン後の初期セットアップを行う（macOS / Linux 用。Windows は Setup-Project.ps1）。
#
# Unity を開く前に、次を確かめて、直せるものは直す。Setup-Project.ps1 と同じ手順にそろえる。
# - クローン先のパス（日本語・空白が無いか）とフォルダ名（SymphonyKillChord か）
# - git の設定（submodule.recurse / core.longpaths）
# - サブモジュールの取得（閲覧権限が無ければ招待を頼むよう案内する）
# - Unity エディタのバージョンと Android Build Support
# - Unity CLI（unity コマンド。無ければ公式のスクリプトで入れる）
# 必須の項目がすべて通ったら、UserSettings/KillChord/ProjectSetupState.json に記録する。
# Unity はこの記録を起動時に読み、無ければ警告ウィンドウを出す。
# 手順を増やしたら project-setup.json の SetupVersion を上げ、両方のスクリプトに足す。
#
# 使い方:
#   ./PowerShell/ProjectSetup/Setup-Project.sh          # 確認して直す
#   ./PowerShell/ProjectSetup/Setup-Project.sh -Check   # 確認だけを行い、何も変更しない
#
# macOS 標準の bash 3.2 でも動くよう、連想配列などは使わない。
set -uo pipefail

CHECK=0
for argument in "$@"; do
    case "$argument" in
        -Check | --check) CHECK=1 ;;
        *) echo "不明な引数です: $argument（使えるのは -Check だけです）" >&2; exit 2 ;;
    esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
CONFIG_PATH="$SCRIPT_DIR/project-setup.json"
STATE_PATH="$REPO_ROOT/UserSettings/KillChord/ProjectSetupState.json"
ERROR_COUNT=0

# JSON から、文字列か数値の値を1つ取り出す（このスクリプトが読む単純なファイル専用）。
json_value() {
    sed -n "s/.*\"$2\"[[:space:]]*:[[:space:]]*\"\{0,1\}\([^\",}]*\)\"\{0,1\}.*/\1/p" "$1" | head -n 1
}

# 状態: OK / Fixed（直した）/ Warn（推奨）/ Info（役職によって必要）/ Error（必須で未完了）
add_result() {
    local name="$1" status="$2" message="$3" color
    case "$status" in
        OK) color='32' ;; Fixed) color='36' ;; Warn) color='33' ;; Info) color='90' ;; *) color='31' ;;
    esac
    if [ "$status" = 'Error' ]; then ERROR_COUNT=$((ERROR_COUNT + 1)); fi
    if [ -t 1 ]; then
        printf '\033[%sm[%-5s] %s: %s\033[0m\n' "$color" "$status" "$name" "$message"
    else
        printf '[%-5s] %s: %s\n' "$status" "$name" "$message"
    fi
}

git_in_repo() {
    git -C "$REPO_ROOT" "$@"
}

SETUP_VERSION="$(json_value "$CONFIG_PATH" SetupVersion)"
EXPECTED_FOLDER_NAME="$(json_value "$CONFIG_PATH" RepositoryFolderName)"

echo "Symphony Kill Chord プロジェクトセットアップ（SetupVersion $SETUP_VERSION）"
if [ "$CHECK" -eq 1 ]; then echo '確認だけを行います（-Check）。何も変更しません。'; fi
echo "リポジトリ: $REPO_ROOT"
echo

# --- git -------------------------------------------------------------------
if ! command -v git > /dev/null 2>&1; then
    add_result 'git' 'Error' 'git が見つかりません。macOS は `xcode-select --install` で入れてから再実行してください。'
    exit 1
fi

# --- クローン先のパス ------------------------------------------------------
# Unity のビルドや一部のツールが、日本語や空白を含むパスの解決に失敗するため。
if printf '%s' "$REPO_ROOT" | LC_ALL=C grep -q '[^!-~]'; then
    add_result 'クローン先のパス' 'Warn' "日本語か空白が含まれています（$REPO_ROOT）。ビルドやツールが失敗する原因になるので、英数字だけのパスへクローンし直すことを勧めます。"
else
    add_result 'クローン先のパス' 'OK' "$REPO_ROOT"
fi

# --- クローン先のフォルダ名 ------------------------------------------------
# Unity と IDE はフォルダ名からソリューション（<フォルダ名>.slnx）を作る。
# 名前が違うと別名のソリューションができ、誤ってコミットされて二重になるため、名前を揃える。
FOLDER_NAME="$(basename "$REPO_ROOT")"
if [ "$FOLDER_NAME" = "$EXPECTED_FOLDER_NAME" ]; then
    add_result 'クローン先のフォルダ名' 'OK' "$FOLDER_NAME"
else
    add_result 'クローン先のフォルダ名' 'Error' "フォルダ名が '$FOLDER_NAME' です。Unity と IDE を閉じてから、フォルダ名を '$EXPECTED_FOLDER_NAME' に変えてください（変えたあと、古い名前の .sln / .slnx は消してよい）。"
fi

# --- git の設定 ------------------------------------------------------------
# submodule.recurse: pull や checkout のときにサブモジュールも追従させる。
# core.longpaths: Windows で 260 文字を超えるパスを扱えるようにする（同じクローンを Windows で使う場合に備えてそろえる）。
for key in submodule.recurse core.longpaths; do
    expected='true'
    current="$(git_in_repo config --local --get "$key" 2> /dev/null)"
    if [ "$current" = "$expected" ]; then
        add_result "git config $key" 'OK' "$expected"
    elif [ "$CHECK" -eq 1 ]; then
        add_result "git config $key" 'Error' "未設定です（現在: '$current'）。"
    else
        git_in_repo config --local "$key" "$expected"
        add_result "git config $key" 'Fixed' "$expected に設定しました。"
    fi
done

# --- サブモジュール --------------------------------------------------------
# git が認証を尋ねられるよう標準入力は空けておき、一覧は別の記述子（3）から読む。
SUBMODULE_LINES="$(git_in_repo config -f .gitmodules --get-regexp '^submodule\..*\.path$')"
while read -r key path <&3; do
    [ -n "$path" ] || continue
    name="${key%.path}"
    status="$(git_in_repo submodule status -- "$path" 2> /dev/null)"
    full_path="$REPO_ROOT/$path"
    if [ -n "$status" ] && [ "${status:0:1}" != '-' ] && [ -d "$full_path" ] && [ -n "$(ls -A "$full_path" 2> /dev/null)" ]; then
        add_result "サブモジュール $path" 'OK' '取得済みです。'
        continue
    fi
    if [ "$CHECK" -eq 1 ]; then
        add_result "サブモジュール $path" 'Error' 'まだ取得していません。'
        continue
    fi

    # 仕様書のように本体の記録を気にせず最新を読むもの（ignore = all）は、ブランチの最新を取る。
    arguments=(submodule update --init --recursive)
    if [ "$(git_in_repo config -f .gitmodules --get "$name.ignore")" = 'all' ]; then
        arguments+=(--remote --depth 1)
    fi

    # 大きなサブモジュールは時間がかかるので、git の進捗表示をそのまま見せる。
    echo "  $path を取得しています..."
    if git_in_repo "${arguments[@]}" -- "$path"; then
        add_result "サブモジュール $path" 'Fixed' '取得しました。'
    else
        add_result "サブモジュール $path" 'Error' '取得できませんでした。上に出た git のエラーを確認してください。認証や「Repository not found」のエラーなら、非公開リポジトリの閲覧権限がありません。リードに GitHub の招待を頼んでください。'
    fi
done 3<<< "$SUBMODULE_LINES"

# --- Unity エディタ --------------------------------------------------------
VERSION_FILE="$REPO_ROOT/ProjectSettings/ProjectVersion.txt"
UNITY_VERSION="$(sed -n 's/^m_EditorVersion:[[:space:]]*\([^[:space:]]*\).*/\1/p' "$VERSION_FILE")"
UNITY_REVISION="$(sed -n 's/^m_EditorVersionWithRevision:[[:space:]]*[^[:space:]]*[[:space:]]*(\([[:alnum:]]*\)).*/\1/p' "$VERSION_FILE")"
HUB_INSTALL_URI="unityhub://$UNITY_VERSION/$UNITY_REVISION"

# Unity Hub の既定のインストール先と、Hub で変更したインストール先を探す。
# macOS: <ルート>/<版>/Unity.app と <ルート>/<版>/PlaybackEngines
# Linux: <ルート>/<版>/Editor/Unity と <ルート>/<版>/Editor/Data/PlaybackEngines
if [ "$(uname -s)" = 'Darwin' ]; then
    EDITOR_ROOTS=('/Applications/Unity/Hub/Editor')
    SECONDARY_PATH_FILE="$HOME/Library/Application Support/UnityHub/secondaryInstallPath.json"
    EDITOR_EXECUTABLE='Unity.app'
    ANDROID_PLAYER='PlaybackEngines/AndroidPlayer'
else
    EDITOR_ROOTS=("$HOME/Unity/Hub/Editor")
    SECONDARY_PATH_FILE="${XDG_CONFIG_HOME:-$HOME/.config}/UnityHub/secondaryInstallPath.json"
    EDITOR_EXECUTABLE='Editor/Unity'
    ANDROID_PLAYER='Editor/Data/PlaybackEngines/AndroidPlayer'
fi
if [ -f "$SECONDARY_PATH_FILE" ]; then
    # 中身は JSON の文字列1つ（例: "/Volumes/Data/Unity"）。
    secondary="$(sed -e 's/^[[:space:]]*"//' -e 's/"[[:space:]]*$//' "$SECONDARY_PATH_FILE")"
    if [ -n "$secondary" ]; then EDITOR_ROOTS+=("$secondary"); fi
fi
EDITOR_PATH=''
for root in "${EDITOR_ROOTS[@]}"; do
    if [ -e "$root/$UNITY_VERSION/$EDITOR_EXECUTABLE" ]; then
        EDITOR_PATH="$root/$UNITY_VERSION"
        break
    fi
done

if [ -z "$EDITOR_PATH" ]; then
    add_result "Unity $UNITY_VERSION" 'Error' "インストールされていません。Unity Hub で $HUB_INSTALL_URI を開き、Android Build Support を付けて入れてください。"
else
    add_result "Unity $UNITY_VERSION" 'OK' "$EDITOR_PATH"
    # ビルドターゲットが Android なので、ビルドしない役職でも無いとプロジェクトを開いたときに警告が出る。
    if [ -d "$EDITOR_PATH/$ANDROID_PLAYER" ]; then
        add_result 'Android Build Support' 'OK' '入っています。'
    else
        add_result 'Android Build Support' 'Error' "入っていません。Unity Hub の「インストール」で $UNITY_VERSION の歯車 →「モジュールを加える」から入れてください。"
    fi
fi

# --- Unity CLI --------------------------------------------------------------
# AI エージェントが Unity を操作するのに使う（エディタの操作は com.unity.pipeline 経由）。
# 公式のインストールスクリプトで、ユーザーのフォルダ（macOS: ~/.unity/bin、Linux: ~/.local/bin）に入る。
find_unity_cli() {
    command -v unity 2> /dev/null && return 0
    for candidate in "$HOME/.unity/bin/unity" "$HOME/.local/bin/unity"; do
        if [ -x "$candidate" ]; then echo "$candidate"; return 0; fi
    done
    return 1
}
if UNITY_CLI="$(find_unity_cli)"; then
    add_result 'Unity CLI' 'OK' "$("$UNITY_CLI" --version 2> /dev/null)"
elif [ "$CHECK" -eq 1 ]; then
    add_result 'Unity CLI' 'Error' '入っていません。'
else
    echo '  Unity CLI を入れています...'
    if curl -fsSL 'https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh' | UNITY_CLI_CHANNEL='beta' bash; then
        if UNITY_CLI="$(find_unity_cli)"; then
            add_result 'Unity CLI' 'Fixed' "$("$UNITY_CLI" --version 2> /dev/null) を入れました。新しく開いたターミナルから unity コマンドを使えます。"
        else
            add_result 'Unity CLI' 'Error' '入れましたが unity コマンドが見つかりません。ターミナルを開き直してから、もう一度実行してください。'
        fi
    else
        add_result 'Unity CLI' 'Error' '入れられませんでした。ネットワークを確かめて、もう一度実行してください。'
    fi
fi

# --- 役職によって必要なもの ------------------------------------------------
# SinfoniaOperator/run-tool.sh と同じく、公式インストールスクリプトの既定の配置先も探す。
DOTNET_CMD="$(command -v dotnet 2> /dev/null)"
for candidate in "${DOTNET_ROOT:-}/dotnet" "$HOME/.dotnet/dotnet" /usr/local/share/dotnet/dotnet; do
    if [ -z "$DOTNET_CMD" ] && [ -x "$candidate" ]; then DOTNET_CMD="$candidate"; fi
done
if [ -n "$DOTNET_CMD" ] && "$DOTNET_CMD" --list-sdks 2> /dev/null | grep -q '^10\.'; then
    add_result '.NET SDK 10' 'OK' 'SinfoniaOperator のツールを実行できます（./SinfoniaOperator/run-tool.sh）。'
else
    add_result '.NET SDK 10' 'Info' 'SinfoniaOperator のツール（Notion・Discord の書き出しなど）を使う人だけ必要です。macOS / Linux では exe が動かないため、ソースから実行します。'
fi

# --- まとめ ----------------------------------------------------------------
echo
if [ "$ERROR_COUNT" -gt 0 ]; then
    echo "未完了の必須項目が $ERROR_COUNT 件あります。上の [Error] の案内に従って直し、もう一度実行してください。"
    exit 1
fi

if [ "$CHECK" -eq 0 ]; then
    mkdir -p "$(dirname "$STATE_PATH")"
    printf '{\n    "SetupVersion": %s,\n    "CompletedAtUtc": "%s",\n    "UnityVersion": "%s"\n}' \
        "$SETUP_VERSION" "$(date -u '+%Y-%m-%dT%H:%M:%SZ')" "$UNITY_VERSION" > "$STATE_PATH"
fi
echo 'セットアップは完了しています。Unity Hub からプロジェクトを開いてください。'
exit 0
