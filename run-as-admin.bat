@echo off
:: Check for administrative privileges
NET SESSION >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo This script requires administrator privileges.
    echo Please run as administrator.
    pause
    exit /b 1
)

:: Run the WPF application
start "" "src\Binary\net8.0-windows\NSSM.WPF.exe"
