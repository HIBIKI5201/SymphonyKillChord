@echo off
rem Double-click this file after cloning to set up the project (git submodules, git config, Unity check).
rem Pass -Check to only verify without changing anything.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0PowerShell\ProjectSetup\Setup-Project.ps1" %*
pause
