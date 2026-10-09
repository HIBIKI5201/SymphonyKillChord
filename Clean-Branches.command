#!/bin/bash
# macOS: Finder でダブルクリックすると、develop に取り込み済みのローカルブランチを確認のうえ削除する。
# Linux: ターミナルから ./Clean-Branches.command を実行する。Windows は Clean-Branches.bat を使う。
# --dry-run を付けると削除候補の表示だけを行う。
cd "$(dirname "$0")" || exit 1
/bin/bash ./scripts/git/clean-local-branches.sh "$@"
status=$?
if [ -t 0 ]; then
    read -r -p 'Enter キーを押すと閉じます...' _
fi
exit $status
