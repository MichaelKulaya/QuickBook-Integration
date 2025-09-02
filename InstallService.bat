@echo off
echo Installing QuickBooks ETL Service...

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

REM Get the current directory
set SERVICE_PATH=%~dp0QuickBooksETLService.exe

REM Check if the service executable exists
if not exist "%SERVICE_PATH%" (
    echo ERROR: QuickBooksETLService.exe not found in current directory
    echo Please ensure the service is built and the executable is present
    pause
    exit /b 1
)

echo Service executable found: %SERVICE_PATH%

REM Stop and delete existing service if it exists
echo Stopping existing service (if running)...
net stop "QuickBooksETLService" >nul 2>&1

echo Removing existing service (if exists)...
sc delete "QuickBooksETLService" >nul 2>&1

REM Create the service
echo Creating Windows Service...
sc create "QuickBooksETLService" binPath="%SERVICE_PATH%" start=auto
if %errorLevel% == 0 (
    echo Service created successfully
) else (
    echo ERROR: Failed to create service
    pause
    exit /b 1
)

REM Set service description
echo Setting service description...
sc description "QuickBooksETLService" "QuickBooks ETL Integration Service"

REM Start the service
echo Starting service...
sc start "QuickBooksETLService"
if %errorLevel% == 0 (
    echo Service started successfully
) else (
    echo WARNING: Service created but failed to start
    echo Check the Windows Event Log for error details
)

echo.
echo Installation completed!
echo.
echo Service Management Commands:
echo   Start:   net start "QuickBooksETLService"
echo   Stop:    net stop "QuickBooksETLService"
echo   Status:  sc query "QuickBooksETLService"
echo   Remove:  sc delete "QuickBooksETLService"
echo.
echo Logs can be found in:
echo   - Windows Event Log: Event Viewer ^> Applications and Services Logs
echo   - File Logs: C:\Logs\QuickBooksETL (if configured)
echo.
pause
