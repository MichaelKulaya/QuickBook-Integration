# QuickBooks ETL Service

A Windows Service that integrates QuickBooks Desktop with an ETL webhook API, automatically extracting invoices and sending them to a specified endpoint.

## Features

- **Windows Service**: Runs continuously in the background on Windows 10
- **QuickBooks Integration**: Connects to QuickBooks Desktop using QBFC SDK
- **Invoice Extraction**: Automatically extracts invoice data from QuickBooks
- **Webhook Integration**: Sends invoice data via HTTP POST to configured endpoint
- **Error Handling**: Comprehensive error handling with retry logic
- **Logging**: Multiple logging options (Event Log, File, Console)
- **Configurable**: Easy configuration via appsettings.json

## Prerequisites

1. **Windows 10** with .NET 6.0 Runtime installed
2. **QuickBooks Desktop** (any supported version)
3. **QuickBooks Desktop SDK** (QBFC) - Download from Intuit Developer website
4. **Administrator privileges** for service installation

## Installation

### 1. Download and Install QuickBooks Desktop SDK

1. Visit [Intuit Developer](https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/get-started-with-quickbooks-desktop-sdk)
2. Download the QuickBooks Desktop SDK
3. Install the SDK on your system
4. Copy the `Interop.QBFC13Lib.dll` file to the `lib` folder in your project

### 2. Build the Service

```bash
# Clone or download the project files
cd QuickBooksETLService

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build --configuration Release
```

### 3. Configure the Service

Edit `appsettings.json` with your specific settings:

```json
{
  "QuickBooksETL": {
    "WebhookEndpoint": "https://etl.saris.info.tz/api/webhook/ngorongoro-quickbook",
    "PollingIntervalMinutes": 5,
    "QuickBooksAppName": "QuickBooks ETL Service",
    "QuickBooksAppID": "YourAppID",
    "QuickBooksCompanyFile": "",
    "HttpTimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 5,
    "LogToFile": true,
    "LogFilePath": "C:\\Logs\\QuickBooksETL"
  }
}
```

### 4. Install the Windows Service

#### Option A: Using sc.exe (Command Line)

```cmd
# Open Command Prompt as Administrator
sc create "QuickBooksETLService" binPath="C:\Path\To\Your\QuickBooksETLService.exe" start=auto
sc description "QuickBooksETLService" "QuickBooks ETL Integration Service"
sc start "QuickBooksETLService"
```

#### Option B: Using PowerShell

```powershell
# Open PowerShell as Administrator
New-Service -Name "QuickBooksETLService" -BinaryPathName "C:\Path\To\Your\QuickBooksETLService.exe" -StartupType Automatic -DisplayName "QuickBooks ETL Service" -Description "QuickBooks ETL Integration Service"
Start-Service -Name "QuickBooksETLService"
```

#### Option C: Using InstallUtil (if using .NET Framework)

```cmd
# Navigate to .NET Framework directory
cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319

# Install the service
InstallUtil.exe "C:\Path\To\Your\QuickBooksETLService.exe"

# Start the service
net start "QuickBooksETLService"
```

## Configuration

### appsettings.json Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `WebhookEndpoint` | URL of the ETL webhook endpoint | Required |
| `PollingIntervalMinutes` | How often to check for new invoices (minutes) | 5 |
| `QuickBooksAppName` | Name of your QuickBooks application | "QuickBooks ETL Service" |
| `QuickBooksAppID` | Your QuickBooks application ID | "YourAppID" |
| `QuickBooksCompanyFile` | Path to specific company file (optional) | "" |
| `HttpTimeoutSeconds` | HTTP request timeout | 30 |
| `MaxRetryAttempts` | Number of retry attempts for failed requests | 3 |
| `RetryDelaySeconds` | Delay between retry attempts | 5 |
| `LogToFile` | Enable file logging | true |
| `LogFilePath` | Path for log files | "C:\\Logs\\QuickBooksETL" |

## Invoice Data Format

The service sends invoice data in the following JSON format:

```json
{
  "invoiceNumber": "INV-2024-001",
  "date": "2024-01-15T10:30:00Z",
  "customer": {
    "name": "Acme Corporation",
    "id": "80000001-1234567890",
    "email": "billing@acmecorp.com",
    "phone": "+1-555-123-4567",
    "address": {
      "line1": "123 Business Street",
      "line2": "Suite 100",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA"
    }
  },
  "amount": 1500.00,
  "lineItems": [
    {
      "itemName": "Professional Services",
      "description": "Software development and consulting services",
      "quantity": 40.0,
      "rate": 25.00,
      "amount": 1000.00,
      "itemType": "Service"
    }
  ],
  "taxAmount": 120.00,
  "totalAmount": 1620.00,
  "dueDate": "2024-02-14T23:59:59Z",
  "memo": "Payment due within 30 days",
  "terms": "Net 30",
  "status": "Open",
  "createdDate": "2024-01-15T10:30:00Z",
  "modifiedDate": "2024-01-15T10:30:00Z"
}
```

## Service Management

### Start the Service
```cmd
net start "QuickBooksETLService"
```

### Stop the Service
```cmd
net stop "QuickBooksETLService"
```

### Check Service Status
```cmd
sc query "QuickBooksETLService"
```

### Uninstall the Service
```cmd
sc delete "QuickBooksETLService"
```

## Logging

The service provides multiple logging options:

1. **Windows Event Log**: View in Event Viewer under Applications and Services Logs
2. **File Logging**: Log files are created in the configured directory
3. **Console Logging**: Available when running in debug mode

### Debug Mode

To run the service in console mode for debugging:

```cmd
QuickBooksETLService.exe --console
```

## Troubleshooting

### Common Issues

1. **Service won't start**
   - Ensure QuickBooks Desktop is installed and running
   - Check that the QuickBooks SDK is properly installed
   - Verify the service has appropriate permissions

2. **Connection to QuickBooks fails**
   - Ensure QuickBooks Desktop is open
   - Check that the company file is accessible
   - Verify the QuickBooks application is registered

3. **Webhook requests fail**
   - Check network connectivity
   - Verify the webhook endpoint URL is correct
   - Check firewall settings

4. **No invoices are processed**
   - Verify the polling interval is appropriate
   - Check that there are new invoices in QuickBooks
   - Review the service logs for errors

### Log Locations

- **Event Log**: Windows Event Viewer → Applications and Services Logs → QuickBooksETLService
- **File Logs**: Configured in `LogFilePath` setting (default: `C:\Logs\QuickBooksETL`)

## Security Considerations

1. **Service Account**: Run the service under a dedicated service account with minimal privileges
2. **Network Security**: Ensure secure communication with the webhook endpoint (HTTPS)
3. **Data Privacy**: Invoice data is transmitted over the network - ensure appropriate security measures
4. **Access Control**: Limit access to the service configuration and log files

## Support

For issues and questions:
1. Check the service logs for error messages
2. Verify QuickBooks Desktop SDK installation
3. Test webhook connectivity manually
4. Review configuration settings

## License

This project is provided as-is for integration purposes. Ensure compliance with QuickBooks SDK licensing requirements.
