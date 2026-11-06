#!/bin/bash
# macOS: Start the SymphonyKillChord build runner in this Terminal window after a pre-flight scan.
# Double-click this file in Finder. The scan itself is start-runner.ps1 (needs PowerShell 7: brew install powershell).
cd "$(dirname "$0")" || exit 1
printf '\033]0;SymphonyKillChord Build Runner - do not close / do not press Ctrl+C\007'
if ! command -v pwsh >/dev/null 2>&1; then
  echo "PowerShell 7 (pwsh) is not installed. Install it with: brew install powershell"
  read -r -p "Press Enter to close." _
  exit 1
fi
if ! pwsh -NoProfile -File ./start-runner.ps1 "$@"; then
  echo
  read -r -p "Press Enter to close." _
  exit 1
fi
./run.sh
echo
echo "The runner has stopped."
read -r -p "Press Enter to close." _
