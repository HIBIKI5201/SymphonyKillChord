#!/bin/bash
# macOS: Double-click this file in Finder after cloning to set up the project (git submodules, git config, Unity check).
# Linux: run ./Setup.command from a terminal. Windows uses Setup.bat instead.
# Pass -Check to only verify without changing anything.
cd "$(dirname "$0")" || exit 1
/bin/bash ./PowerShell/ProjectSetup/Setup-Project.sh "$@"
status=$?
# Setup.bat の pause と同じく、ダブルクリックで開いたウィンドウが結果を読む前に閉じないよう待つ。
if [ -t 0 ]; then
    read -r -p 'Enter キーを押すと閉じます...' _
fi
exit $status
