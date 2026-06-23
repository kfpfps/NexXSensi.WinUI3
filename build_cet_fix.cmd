@echo off
setlocal
set DOTNET_ROLL_FORWARD=LatestPatch
powershell -ExecutionPolicy Bypass -File "%~dp0build.ps1"
pause
