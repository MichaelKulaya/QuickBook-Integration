# QuickBooks Desktop SDK Library

This directory should contain the QuickBooks Desktop SDK interop library.

## Required File

- `Interop.QBFC13Lib.dll` - QuickBooks Desktop SDK interop library

## How to Obtain

1. Download the QuickBooks Desktop SDK from [Intuit Developer](https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/get-started-with-quickbooks-desktop-sdk)
2. Install the SDK on your development machine
3. Locate the `Interop.QBFC13Lib.dll` file (usually in the SDK installation directory)
4. Copy the file to this `lib` directory

## Alternative Locations

The interop library is typically found in one of these locations after SDK installation:

- `C:\Program Files (x86)\Intuit\IDN\QBSDK13\`
- `C:\Program Files\Intuit\IDN\QBSDK13\`
- `C:\QBSDK\`

## Important Notes

- The SDK must be installed on the target machine where the service will run
- Ensure the SDK version matches the interop library version
- The service requires QuickBooks Desktop to be installed and running for proper operation

## Troubleshooting

If you encounter issues with the QuickBooks SDK:

1. Verify the SDK is properly installed
2. Check that QuickBooks Desktop is running
3. Ensure the interop library version matches your QuickBooks version
4. Run the service with administrator privileges
