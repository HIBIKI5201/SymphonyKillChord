@echo off
rem Double-click this file to preview the home page locally.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0dev.ps1"
if errorlevel 1 pause
