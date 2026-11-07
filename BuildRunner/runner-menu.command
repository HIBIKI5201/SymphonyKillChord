#!/bin/bash
# macOS: Management menu for the SymphonyKillChord self-hosted runner.
# Double-click this file in Finder. The menu itself is runner-menu.ps1 (needs PowerShell 7: brew install powershell).
cd "$(dirname "$0")" || exit 1
if ! command -v pwsh >/dev/null 2>&1; then
  echo "PowerShell 7 (pwsh) is not installed. Install it with: brew install powershell"
  read -r -p "Press Enter to close." _
  exit 1
fi
pwsh -NoProfile -File ./runner-menu.ps1 "$@"
