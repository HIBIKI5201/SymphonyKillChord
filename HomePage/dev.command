#!/bin/bash
# macOS: Double-click this file in Finder to preview the home page locally.
# Linux: run ./dev.command from a terminal. Windows uses dev.bat instead.
cd "$(dirname "$0")" || exit 1

run() {
    if ! command -v npm >/dev/null 2>&1; then
        echo 'Node.js (npm) is required. Install Node.js 22.12 or later.' >&2
        return 1
    fi

    if [ ! -f node_modules/astro/package.json ]; then
        npm ci || { echo 'npm ci failed.' >&2; return 1; }
    fi

    local astro=./node_modules/.bin/astro
    "$astro" dev --background --host 127.0.0.1 --port 4321 || {
        echo 'Astro dev server failed to start.' >&2
        return 1
    }

    local status url
    status=$("$astro" dev status --json 2>&1)
    url=$(printf '%s\n' "$status" | grep -oE 'https?://[^[:space:]"\\]+' | head -n 1)
    if [ -z "$url" ]; then
        echo "Could not determine the dev server URL: $status" >&2
        return 1
    fi

    url="${url%/}/SymphonyKillChord/home"
    echo "Opening $url"
    if command -v open >/dev/null 2>&1; then
        open "$url"
    elif command -v xdg-open >/dev/null 2>&1; then
        xdg-open "$url"
    else
        echo 'Open the URL above in your browser.'
    fi
}

run
status=$?
# dev.bat の pause と同じく、失敗したときはダブルクリックで開いたウィンドウが結果を読む前に閉じないよう待つ。
if [ $status -ne 0 ] && [ -t 0 ]; then
    read -r -p 'Enter キーを押すと閉じます...' _
fi
exit $status
