@echo off
setlocal enabledelayedexpansion

xcopy /y /i /e "%~dp0download\*" "%~dp0"


rmdir /s /q "%~dp0download"

for /f "tokens=2 delims=," %%a in ('tasklist ^| find /i "unreal-GUI-download.exe"') do (
    taskkill /f /pid %%a
)

start "" "%~dp0unreal-GUI-download.exe"
endlocal