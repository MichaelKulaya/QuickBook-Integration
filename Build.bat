@echo off
echo Building QuickBooks ETL Service...

REM Check if QuickBooks SDK DLL exists
if not exist "lib\Interop.QBFC13Lib.dll" (
    echo WARNING: QuickBooks SDK DLL not found!
    echo Please install QuickBooks Desktop SDK and copy Interop.QBFC13Lib.dll to lib folder
    echo The DLL is typically found in: C:\Program Files\Intuit\IDN\QBSDK13\
    echo.
    echo Continuing build anyway (will fail if QuickBooks SDK is required)...
    echo.
)

REM Check if .NET 6.0 is installed
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET 6.0 SDK is not installed or not in PATH
    echo Please install .NET 6.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET SDK found: 
dotnet --version

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore
if %errorLevel% neq 0 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)

REM Build the project
echo Building project...
dotnet build --configuration Release --no-restore
if %errorLevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

REM Publish the application
echo Publishing application...
dotnet publish --configuration Release --no-build --output ./publish
if %errorLevel% neq 0 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
echo Output files are in the ./publish directory
echo.
echo Next steps:
echo 1. Copy the QuickBooks SDK Interop.QBFC13Lib.dll to the lib folder
echo 2. Update appsettings.json with your configuration
echo 3. Run InstallService.bat as Administrator to install the service
echo.
pause
