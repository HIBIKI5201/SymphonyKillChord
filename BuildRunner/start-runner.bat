@echo off
rem Start the SymphonyKillChord build runner in this window after a pre-flight scan.
rem The scan itself is start-runner.ps1 (this file is kept ASCII to avoid code page issues).
title SymphonyKillChord Build Runner - do not close / do not press Ctrl+C
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0start-runner.ps1" %*
if errorlevel 1 (
  echo.
  pause
  exit /b 1
)
cd /d "%~dp0"
call "%~dp0run.cmd"
echo.
echo The runner has stopped.
pause
