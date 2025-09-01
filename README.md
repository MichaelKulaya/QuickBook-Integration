# QuickBooks ETL Service

A Windows Service built in C# that integrates QuickBooks Desktop with an ETL webhook API. The service automatically extracts invoices from QuickBooks Desktop and sends them to a specified webhook endpoint.

## Features

- **Windows Service**: Runs as a background service on Windows 10
- **QuickBooks Integration**: Connects to QuickBooks Desktop using QBXML
- **Automatic Polling**: Periodically checks for new invoices (configurable interval)
- **Webhook Delivery**: Sends invoice data via HTTP POST to ETL endpoint
- **Error Handling**: Comprehensive error handling with retry logic
- **Logging**: Detailed logging to file, console, and Windows Event Log
- **Configuration**: Flexible configuration via JSON files
- **Service Management**: Easy installation, uninstallation, and management scripts

## Requirements

- **Operating System**: Windows 10 or later
- **.NET Framework**: .NET 6.0 or later
- **QuickBooks Desktop**: Any recent version with QBXML support
- **Administrator Rights**: Required for service installation

## Project Structure

```
QuickBooksETLService/
├── Models/                          # Data models
│   ├── Invoice.cs                   # Invoice data structure
│   └── ServiceConfiguration.cs      # Configuration models
├── Services/                        # Business logic services
│   ├── IQuickBooksService.cs        # QuickBooks service interface
│   ├── QuickBooksService.cs         # QuickBooks integration
│   ├── IWebhookService.cs           # Webhook service interface
│   ├── WebhookService.cs            # HTTP webhook delivery
│   ├── IETLService.cs               # ETL orchestration interface
│   └── ETLService.cs                # Main ETL logic
├── QuickBooksETLWindowsService.cs   # Windows Service implementation
├── Program.cs                       # Application entry point
├── appsettings.json                 # Main configuration
├── appsettings.Development.json     # Development configuration
└── QuickBooksETLService.csproj      # Project file
```

## Configuration

### appsettings.json

The main configuration file contains all service settings:

```json
{
  "ServiceSettings": {
    "ServiceName": "QuickBooksETLService",
    "DisplayName": "QuickBooks ETL Service",
    "Description": "Extracts invoices from QuickBooks Desktop and sends to ETL webhook",
    "PollingIntervalMinutes": 5,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 30
  },
  "QuickBooksSettings": {
    "CompanyFile": "",
    "ConnectionTimeoutSeconds": 30,
    "QueryLimit": 100
  },
  "WebhookSettings": {
    "EndpointUrl": "https://etl.saris.info.tz/api/webhook/ngorongoro-quickbook",
    "TimeoutSeconds": 60,
    "ContentType": "application/json"
  }
}
```

### Configuration Options

#### ServiceSettings
- **PollingIntervalMinutes**: How often to check for new invoices (default: 5)
- **MaxRetryAttempts**: Number of retry attempts for failed webhook calls (default: 3)
- **RetryDelaySeconds**: Delay between retry attempts (default: 30)

#### QuickBooksSettings
- **CompanyFile**: Path to QuickBooks company file (.QBW). Leave empty for auto-detection
- **ConnectionTimeoutSeconds**: QuickBooks connection timeout (default: 30)
- **QueryLimit**: Maximum number of invoices to retrieve per query (default: 100)

#### WebhookSettings
- **EndpointUrl**: ETL webhook endpoint URL
- **TimeoutSeconds**: HTTP request timeout (default: 60)
- **ContentType**: Content type for HTTP requests (default: application/json)

## Invoice Data Structure

The service sends invoice data in the following JSON format:

```json
{
  "invoiceNumber": "INV-001",
  "date": "2024-01-15T10:30:00Z",
  "dueDate": "2024-02-14T10:30:00Z",
  "customer": {
    "name": "John Doe",
    "companyName": "ABC Company Inc.",
    "email": "john@abc.com",
    "phone": "+1-555-0123",
    "address": {
      "line1": "123 Main St",
      "line2": "Suite 100",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA"
    }
  },
  "amount": 1500.00,
  "subtotal": 1500.00,
  "taxAmount": 0.00,
  "balance": 1500.00,
  "memo": "Invoice description",
  "lineItems": [
    {
      "description": "Professional Services",
      "quantity": 1,
      "unitPrice": 1500.00,
      "amount": 1500.00,
      "itemName": "Service Item",
      "itemType": "Service"
    }
  ],
  "quickBooksId": "QB-001",
  "extractedAt": "2024-01-15T10:35:00Z"
}
```

## Building the Project

### Prerequisites
1. Install Visual Studio 2022 or .NET 6.0 SDK
2. Ensure you have Windows development tools installed

### Build Steps
1. Open the solution in Visual Studio
2. Set configuration to "Release"
3. Build the solution (Ctrl+Shift+B)
4. The executable will be created in `bin\Release\net6.0-windows\`

### Command Line Build
```bash
cd QuickBooksETLService
dotnet build --configuration Release
```

## Installation

### Method 1: Using Batch Script (Recommended)
1. Right-click on `install-service.bat`
2. Select "Run as administrator"
3. The script will automatically install and start the service

### Method 2: Using PowerShell Script
```powershell
# Run PowerShell as Administrator
.\manage-service.ps1 -Action install
```

### Method 3: Manual Installation
```cmd
# Run Command Prompt as Administrator
sc create QuickBooksETLService binPath= "C:\path\to\QuickBooksETLService.exe" DisplayName= "QuickBooks ETL Service" start= auto
sc description QuickBooksETLService "Extracts invoices from QuickBooks Desktop and sends to ETL webhook"
sc start QuickBooksETLService
```

## Service Management

### Using PowerShell Script (Recommended)
```powershell
# Check service status
.\manage-service.ps1 -Action status

# Start the service
.\manage-service.ps1 -Action start

# Stop the service
.\manage-service.ps1 -Action stop

# Restart the service
.\manage-service.ps1 -Action restart

# View logs
.\manage-service.ps1 -Action logs

# View configuration
.\manage-service.ps1 -Action config
```

### Using Windows Services Console
1. Press `Win + R`, type `services.msc`, press Enter
2. Find "QuickBooks ETL Service"
3. Right-click to start, stop, or restart

### Using Command Line
```cmd
# Check status
sc query QuickBooksETLService

# Start service
sc start QuickBooksETLService

# Stop service
sc stop QuickBooksETLService
```

## Testing

### Console Mode
Run the service in console mode for debugging:
```bash
QuickBooksETLService.exe --console
```

### Test Webhook Connection
The service automatically tests connections on startup. Check logs for connection status.

## Logging

The service logs to multiple destinations:

### File Logs
- Location: `logs/quickbooks-etl-YYYY-MM-DD.log`
- Rolling daily logs with 30-day retention
- Maximum file size: 10MB

### Windows Event Log
- Source: QuickBooksETLService
- Log: Application
- View in Event Viewer

### Console Logs
- Available when running in console mode
- Useful for debugging

## Troubleshooting

### Common Issues

#### Service Won't Start
1. Check Windows Event Log for errors
2. Verify QuickBooks company file exists
3. Ensure webhook endpoint is accessible
4. Check service account permissions

#### No Invoices Being Sent
1. Verify QuickBooks connection in logs
2. Check webhook endpoint URL
3. Review polling interval configuration
4. Check for new invoices in QuickBooks

#### Connection Errors
1. Verify QuickBooks is running
2. Check company file path in configuration
3. Ensure network access to webhook endpoint
4. Review timeout settings

### Debug Mode
1. Stop the Windows Service
2. Run in console mode: `QuickBooksETLService.exe --console`
3. Monitor console output for detailed information

## Uninstallation

### Using Batch Script
1. Right-click on `uninstall-service.bat`
2. Select "Run as administrator"

### Using PowerShell Script
```powershell
.\manage-service.ps1 -Action uninstall
```

### Manual Uninstallation
```cmd
sc stop QuickBooksETLService
sc delete QuickBooksETLService
```

## Security Considerations

- The service runs under the Local System account by default
- Consider using a dedicated service account for production use
- Ensure webhook endpoint uses HTTPS
- Review and restrict network access as needed

## Performance Tuning

### Polling Interval
- **Frequent updates**: Set to 1-2 minutes
- **Standard operation**: 5-10 minutes (default)
- **Batch processing**: 15-30 minutes

### Query Limits
- Adjust `QueryLimit` based on QuickBooks performance
- Consider memory usage for large datasets

### Retry Settings
- Increase `MaxRetryAttempts` for unreliable networks
- Adjust `RetryDelaySeconds` based on webhook response times

## Development

### Adding New Features
1. Implement new interfaces in the Services folder
2. Register services in dependency injection
3. Update configuration models as needed
4. Add comprehensive logging

### Testing
1. Use console mode for development
2. Test with sample QuickBooks data
3. Verify webhook delivery
4. Check error handling scenarios

## Support

For issues and questions:
1. Check the logs for detailed error information
2. Review Windows Event Log entries
3. Test webhook connectivity independently
4. Verify QuickBooks integration separately

## License

This project is provided as-is for integration purposes. Ensure compliance with QuickBooks licensing and your organization's policies. 