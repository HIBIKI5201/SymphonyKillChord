@echo off
rem Double-click to delete local branches that are already merged into origin/develop (asks before deleting).
rem Pass --dry-run to only list the candidates. The logic is scripts\git\clean-local-branches.sh,
rem run with the bash bundled in Git for Windows (the bash on PATH may be the WSL stub).
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
"%GIT_BASH%" "%~dp0scripts/git/clean-local-branches.sh" %*
pause
