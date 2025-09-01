@echo off
echo Building MSI for QuickBooks ETL Service...

REM Ensure .NET SDK exists
dotnet --version >nul 2>&1 || (
    echo ERROR: .NET SDK not found. Install .NET 6 SDK.
    exit /b 1
)

REM Ensure WiX v4 CLI exists
where wix >nul 2>&1 || (
    echo WiX CLI not found. Installing...
    dotnet tool install --global wix || (
        echo ERROR: Failed to install WiX CLI. Install manually: dotnet tool install --global wix
        exit /b 1
    )
)

setlocal
set PATH=%USERPROFILE%\.dotnet\tools;%PATH%

REM Build service
cd QuickBooksETLService || exit /b 1
dotnet build -c Release || exit /b 1
cd ..

REM Create output dir
if not exist dist mkdir dist

REM Build MSI
cd installer || exit /b 1
wix build Product.wxs -arch x64 -o ..\dist\QuickBooksETLService.msi || (
    echo ERROR: MSI build failed.
    exit /b 1
)
cd ..

if exist dist\QuickBooksETLService.msi (
    echo SUCCESS: Built dist\QuickBooksETLService.msi
) else (
    echo ERROR: MSI not found after build.
    exit /b 1
)

endlocal