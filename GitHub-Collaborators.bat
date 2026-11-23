@echo off
rem Double-click to see which GitHub accounts are collaborators on each repository (list),
rem or pass "invite" to invite one account to all repositories (asks for the account and confirms).
rem The logic is scripts\github\collaborators.sh, run with the bash bundled in Git for Windows
rem (the bash on PATH may be the WSL stub). Needs gh (GitHub CLI) logged in with "gh auth login".
setlocal
set "GIT_BASH="
for /f "delims=" %%i in ('where git 2^>nul') do (
  if not defined GIT_BASH if exist "%%~dpi..\bin\bash.exe" set "GIT_BASH=%%~dpi..\bin\bash.exe"
  if not defined GIT_BASH if exist "%%~dpi..\..\bin\bash.exe" set "GIT_BASH=%%~dpi..\..\bin\bash.exe"
)
if not defined GIT_BASH (
  echo Git for Windows bash was not found. Install Git for Windows and make sure git is on PATH.
  pause
  exit /b 1
)
if "%~1"=="" (
  "%GIT_BASH%" "%~dp0scripts/github/collaborators.sh" list
) else (
  "%GIT_BASH%" "%~dp0scripts/github/collaborators.sh" %*
)
pause
