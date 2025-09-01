@echo off
echo Uninstalling QuickBooks ETL Service...

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as administrator - proceeding with uninstallation
) else (
    echo ERROR: This script must be run as Administrator
    echo Right-click on this file and select "Run as administrator"
    pause
    exit /b 1
)

REM Stop the service if it's running
echo Stopping service if running...
sc stop QuickBooksETLService >nul 2>&1
timeout /t 3 /nobreak >nul

REM Remove the service
echo Removing service...
sc delete QuickBooksETLService

if %errorLevel% == 0 (
    echo.
    echo SUCCESS: QuickBooks ETL Service has been uninstalled!
    echo.
) else (
    echo ERROR: Failed to uninstall the service
    echo The service may not have been installed or there was an error
)

pause 