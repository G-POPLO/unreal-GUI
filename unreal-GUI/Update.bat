@echo off
setlocal enabledelayedexpansion
:: 复制download文件夹的内容到当前目录
:: xcopy /y /i /e "%~dp0download\*" "%~dp0"
:: 删除download文件夹
rmdir /s /q "%~dp0download"
:: 关闭正在运行的Unreal-GUI.exe进程
for /f "tokens=2 delims=," %%a in ('tasklist ^| find /i "Unreal-GUI.exe"') do (taskkill /f /pid %%a)
:: 更新完毕后启动Unreal-GUI.exe
:: start "" "%~dp0Unreal-GUI.exe"
endlocal