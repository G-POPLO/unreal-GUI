@echo off
setlocal enabledelayedexpansion
:: ����download�ļ��е����ݵ���ǰĿ¼
xcopy /y /i /e "%~dp0download\*" "%~dp0"
:: ɾ��download�ļ���
rmdir /s /q "%~dp0download"
:: �ر��������е�Unreal-GUI.exe����
for /f "tokens=2 delims=," %%a in ('tasklist ^| find /i "Unreal-GUI.exe"') do (taskkill /f /pid %%a)
:: ������Ϻ�����Unreal-GUI.exe
start "" "%~dp0Unreal-GUI.exe"
endlocal