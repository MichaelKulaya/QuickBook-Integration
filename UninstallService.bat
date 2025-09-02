@echo off
echo Uninstalling QuickBooks ETL Service...

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as Administrator - OK
) else (
    echo ERROR: This script must be run as Administrator
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

REM Stop the service
echo Stopping service...
net stop "QuickBooksETLService" >nul 2>&1
if %errorLevel% == 0 (
    echo Service stopped successfully
) else (
    echo Service was not running or already stopped
)

REM Delete the service
echo Removing service...
sc delete "QuickBooksETLService"
if %errorLevel% == 0 (
    echo Service removed successfully
) else (
    echo ERROR: Failed to remove service
    echo The service may not exist or you may not have sufficient privileges
    pause
    exit /b 1
)

echo.
echo Uninstallation completed!
echo The QuickBooks ETL Service has been removed from Windows Services.
echo.
pause
