@echo off
rem Management menu for the SymphonyKillChord self-hosted runner.
rem Double-click this file. The menu itself is runner-menu.ps1 (kept ASCII here to avoid code page issues).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0runner-menu.ps1" %*
