@echo off
echo Building QuickBooks ETL Service and creating ZIP package...

REM Build the service first
call build.bat
if %errorLevel% neq 0 (
    echo ERROR: Service build failed
    pause
    exit /b 1
)

REM Create dist directory
if not exist dist mkdir dist

REM Create logs directory in dist
if not exist dist\logs mkdir dist\logs

REM Copy service files
echo Copying service files...
xcopy "QuickBooksETLService\bin\Release\net6.0-windows7.0\*" "dist\QuickBooksETLService\" /E /I /Y

REM Copy configuration files
echo Copying configuration files...
copy "QuickBooksETLService\appsettings.json" "dist\QuickBooksETLService\" /Y
copy "QuickBooksETLService\appsettings.Development.json" "dist\QuickBooksETLService\" /Y

REM Copy installation scripts
echo Copying installation scripts...
copy "install-service.bat" "dist\" /Y
copy "uninstall-service.bat" "dist\" /Y
copy "manage-service.ps1" "dist\" /Y

REM Copy documentation
echo Copying documentation...
copy "sample-invoice.json" "dist\" /Y

REM Create installation instructions
echo Creating installation instructions...
(
echo QuickBooks ETL Service - Installation Instructions
echo ================================================
echo.
echo 1. Extract this ZIP file to a folder on your Windows machine
echo 2. Open Command Prompt as Administrator
echo 3. Navigate to the extracted folder
echo 4. Run: install-service.bat
echo.
echo Alternative installation using PowerShell:
echo 1. Open PowerShell as Administrator
echo 2. Navigate to the extracted folder
echo 3. Run: .\manage-service.ps1 -Action install
echo.
echo Service Management:
echo - Check status: .\manage-service.ps1 -Action status
echo - Start service: .\manage-service.ps1 -Action start
echo - Stop service: .\manage-service.ps1 -Action stop
echo - View logs: .\manage-service.ps1 -Action logs
echo - Uninstall: .\manage-service.ps1 -Action uninstall
echo.
echo For more information, see README.md
) > "dist\INSTALL.txt"

REM Create ZIP file
echo Creating ZIP package...
powershell -Command "Compress-Archive -Path 'dist\*' -DestinationPath 'QuickBooksETLService-Install.zip' -Force"

echo.
echo SUCCESS: Created QuickBooksETLService-Install.zip
echo.
echo To install:
echo 1. Copy the ZIP to your Windows machine
echo 2. Extract and run install-service.bat as Administrator
echo.
pause 