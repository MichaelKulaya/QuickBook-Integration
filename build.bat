@echo off
echo Building QuickBooks ETL Service...

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET SDK is not installed or not in PATH
    echo Please install .NET 6.0 SDK or later
    pause
    exit /b 1
)

echo .NET SDK found. Building project...

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean QuickBooksETLService --configuration Release

REM Restore packages
echo Restoring NuGet packages...
dotnet restore QuickBooksETLService

REM Build the project
echo Building project in Release configuration...
dotnet build QuickBooksETLService --configuration Release --no-restore

if %errorLevel% equ 0 (
    echo.
    echo SUCCESS: Build completed successfully!
    echo.
    echo Output location: QuickBooksETLService\bin\Release\net6.0-windows\
    echo.
    echo Next steps:
    echo 1. Run install-service.bat as Administrator to install the service
    echo 2. Or use the PowerShell script: .\manage-service.ps1 -Action install
    echo.
) else (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above for details.
    echo.
)

pause 