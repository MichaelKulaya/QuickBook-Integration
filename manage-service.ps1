# QuickBooks ETL Service Management Script
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("status", "start", "stop", "restart", "install", "uninstall", "logs", "config")]
    [string]$Action = "status"
)

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host "Right-click on PowerShell and select 'Run as administrator'" -ForegroundColor Yellow
    exit 1
}

$ServiceName = "QuickBooksETLService"
$ServiceDisplayName = "QuickBooks ETL Service"
$ProjectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$ServiceExePath = Join-Path $ProjectPath "QuickBooksETLService\bin\Release\net6.0-windows\QuickBooksETLService.exe"

function Get-ServiceStatus {
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            Write-Host "Service Status:" -ForegroundColor Green
            Write-Host "  Name: $($service.Name)" -ForegroundColor White
            Write-Host "  Display Name: $($service.DisplayName)" -ForegroundColor White
            Write-Host "  Status: $($service.Status)" -ForegroundColor White
            Write-Host "  Start Type: $($service.StartType)" -ForegroundColor White
            Write-Host "  Path: $($service.BinaryPathName)" -ForegroundColor White
        } else {
            Write-Host "Service '$ServiceName' is not installed" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Error checking service status: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Start-ServiceCustom {
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            if ($service.Status -eq "Running") {
                Write-Host "Service is already running" -ForegroundColor Green
            } else {
                Start-Service -Name $ServiceName
                Write-Host "Service started successfully" -ForegroundColor Green
                Start-Sleep -Seconds 2
                Get-ServiceStatus
            }
        } else {
            Write-Host "Service '$ServiceName' is not installed" -ForegroundColor Red
        }
    } catch {
        Write-Host "Error starting service: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Stop-ServiceCustom {
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            if ($service.Status -eq "Stopped") {
                Write-Host "Service is already stopped" -ForegroundColor Yellow
            } else {
                Stop-Service -Name $ServiceName -Force
                Write-Host "Service stopped successfully" -ForegroundColor Green
                Start-Sleep -Seconds 2
                Get-ServiceStatus
            }
        } else {
            Write-Host "Service '$ServiceName' is not installed" -ForegroundColor Red
        }
    } catch {
        Write-Host "Error stopping service: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Restart-ServiceCustom {
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            Write-Host "Restarting service..." -ForegroundColor Yellow
            Restart-Service -Name $ServiceName -Force
            Write-Host "Service restarted successfully" -ForegroundColor Green
            Start-Sleep -Seconds 3
            Get-ServiceStatus
        } else {
            Write-Host "Service '$ServiceName' is not installed" -ForegroundColor Red
        }
    } catch {
        Write-Host "Error restarting service: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Install-ServiceCustom {
    try {
        if (!(Test-Path $ServiceExePath)) {
            Write-Host "ERROR: Service executable not found at: $ServiceExePath" -ForegroundColor Red
            Write-Host "Please build the project first (Release configuration)" -ForegroundColor Yellow
            return
        }

        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            Write-Host "Service is already installed. Uninstalling first..." -ForegroundColor Yellow
            Uninstall-ServiceCustom
        }

        Write-Host "Installing service..." -ForegroundColor Yellow
        $null = New-Service -Name $ServiceName -BinaryPathName $ServiceExePath -DisplayName $ServiceDisplayName -StartupType Automatic -Description "Extracts invoices from QuickBooks Desktop and sends to ETL webhook"
        
        Write-Host "Starting service..." -ForegroundColor Yellow
        Start-Service -Name $ServiceName
        
        Write-Host "Service installed and started successfully!" -ForegroundColor Green
        Get-ServiceStatus
    } catch {
        Write-Host "Error installing service: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Uninstall-ServiceCustom {
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            if ($service.Status -eq "Running") {
                Write-Host "Stopping service..." -ForegroundColor Yellow
                Stop-Service -Name $ServiceName -Force
                Start-Sleep -Seconds 3
            }
            
            Write-Host "Removing service..." -ForegroundColor Yellow
            Remove-Service -Name $ServiceName
            
            Write-Host "Service uninstalled successfully!" -ForegroundColor Green
        } else {
            Write-Host "Service '$ServiceName' is not installed" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Error uninstalling service: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Show-Logs {
    try {
        $logPath = Join-Path $ProjectPath "logs"
        if (Test-Path $logPath) {
            $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending
            if ($logFiles.Count -gt 0) {
                Write-Host "Recent log files:" -ForegroundColor Green
                foreach ($file in $logFiles | Select-Object -First 5) {
                    Write-Host "  $($file.Name) - $($file.LastWriteTime)" -ForegroundColor White
                }
                
                $latestLog = $logFiles[0]
                Write-Host "`nLatest log file: $($latestLog.Name)" -ForegroundColor Green
                Write-Host "Last 20 lines:" -ForegroundColor Green
                Get-Content $latestLog.FullName -Tail 20
            } else {
                Write-Host "No log files found in: $logPath" -ForegroundColor Yellow
            }
        } else {
            Write-Host "Log directory not found: $logPath" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Error showing logs: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Show-Configuration {
    try {
        $configPath = Join-Path $ProjectPath "QuickBooksETLService\appsettings.json"
        if (Test-Path $configPath) {
            Write-Host "Current configuration:" -ForegroundColor Green
            $config = Get-Content $configPath | ConvertFrom-Json
            $config | ConvertTo-Json -Depth 10
        } else {
            Write-Host "Configuration file not found: $configPath" -ForegroundColor Red
        }
    } catch {
        Write-Host "Error showing configuration: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Main execution
Write-Host "QuickBooks ETL Service Management" -ForegroundColor Cyan
Write-Host "Action: $Action" -ForegroundColor White
Write-Host ""

switch ($Action) {
    "status" { Get-ServiceStatus }
    "start" { Start-ServiceCustom }
    "stop" { Stop-ServiceCustom }
    "restart" { Restart-ServiceCustom }
    "install" { Install-ServiceCustom }
    "uninstall" { Uninstall-ServiceCustom }
    "logs" { Show-Logs }
    "config" { Show-Configuration }
    default { 
        Write-Host "Available actions:" -ForegroundColor Yellow
        Write-Host "  status    - Show service status" -ForegroundColor White
        Write-Host "  start     - Start the service" -ForegroundColor White
        Write-Host "  stop      - Stop the service" -ForegroundColor White
        Write-Host "  restart   - Restart the service" -ForegroundColor White
        Write-Host "  install   - Install the service" -ForegroundColor White
        Write-Host "  uninstall - Uninstall the service" -ForegroundColor White
        Write-Host "  logs      - Show recent logs" -ForegroundColor White
        Write-Host "  config    - Show configuration" -ForegroundColor White
    }
} 