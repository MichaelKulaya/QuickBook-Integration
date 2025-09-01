@echo off
echo Installing QuickBooks ETL Service...

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as administrator - proceeding with installation
) else (
    echo ERROR: This script must be run as Administrator
    echo Right-click on this file and select "Run as administrator"
    pause
    exit /b 1
)

REM Get the current directory
set "CURRENT_DIR=%~dp0"
set "SERVICE_PATH=%CURRENT_DIR%QuickBooksETLService\bin\Release\net6.0-windows7.0\QuickBooksETLService.exe"

REM Check if the executable exists
if not exist "%SERVICE_PATH%" (
    echo ERROR: Service executable not found at: %SERVICE_PATH%
    echo Please build the project first (Release configuration)
    pause
    exit /b 1
)

REM Stop and remove existing service if it exists
echo Stopping existing service if running...
sc stop QuickBooksETLService >nul 2>&1
timeout /t 2 /nobreak >nul

echo Removing existing service if exists...
sc delete QuickBooksETLService >nul 2>&1
timeout /t 2 /nobreak >nul

REM Install the service
echo Installing service...
sc create QuickBooksETLService binPath= "%SERVICE_PATH%" DisplayName= "QuickBooks ETL Service" start= auto

if %errorLevel% == 0 (
    echo Setting service description...
    sc description QuickBooksETLService "Extracts invoices from QuickBooks Desktop and sends to ETL webhook"
    
    echo Starting service...
    sc start QuickBooksETLService
    
    if %errorLevel% == 0 (
        echo.
        echo SUCCESS: QuickBooks ETL Service has been installed and started!
        echo.
        echo Service Information:
        echo - Name: QuickBooksETLService
        echo - Display Name: QuickBooks ETL Service
        echo - Status: Running
        echo - Startup: Automatic
        echo.
        echo You can manage the service using:
        echo - Services.msc (Windows Services console)
        echo - sc start/stop/query QuickBooksETLService (command line)
        echo.
    ) else (
        echo ERROR: Failed to start the service
        echo Check the Windows Event Log for details
    )
) else (
    echo ERROR: Failed to install the service
    echo Check the Windows Event Log for details
)

pause 