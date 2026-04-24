@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: 等待Unreal-GUI.exe进程完全关闭
echo 等待程序关闭...
:waitloop
tasklist /FI "IMAGENAME eq Unreal-GUI.exe" 2>NUL | find /I /N "Unreal-GUI.exe">NUL
if "%ERRORLEVEL%"=="0" (
    timeout /t 1 /nobreak >nul
    goto waitloop
)

:: 创建排除文件列表（排除Update.bat自身，避免被覆盖）
echo Update.bat > "%~dp0exclude.txt"

:: 复制download文件夹的内容到当前目录（排除Update.bat）
echo 正在复制更新文件...
xcopy /y /i /e /EXCLUDE:"%~dp0exclude.txt" "%~dp0download\*" "%~dp0"

:: 删除排除文件列表
del "%~dp0exclude.txt"

:: 删除download文件夹
echo 清理临时文件...
rmdir /s /q "%~dp0download"

:: 更新完成后启动Unreal-GUI.exe
echo 启动程序...
start "" "%~dp0Unreal-GUI.exe"

endlocal