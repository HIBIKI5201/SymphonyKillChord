#!/bin/bash
# macOS: Finder でダブルクリックすると、リポジトリごとの GitHub コラボレーターを表で出す。
# Linux: ターミナルから ./GitHub-Collaborators.command を実行する。Windows は GitHub-Collaborators.bat を使う。
# invite を付けると、入力したアカウントを全リポジトリへ招待する（確認あり）。gh auth login 済みであること。
cd "$(dirname "$0")" || exit 1
if [ $# -eq 0 ]; then
    set -- list
fi
/bin/bash ./scripts/github/collaborators.sh "$@"
status=$?
if [ -t 0 ]; then
    read -r -p 'Enter キーを押すと閉じます...' _
fi
exit $status
