using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuickBooksETLService.Models;
using QBFC13Lib;

namespace QuickBooksETLService.Services
{
    public class QuickBooksService : IDisposable
    {
        private readonly ILogger<QuickBooksService> _logger;
        private QBSessionManager? _sessionManager;
        private bool _isConnected = false;
        private readonly string _appName;
        private readonly string _appID;

        public QuickBooksService(ILogger<QuickBooksService> logger, string appName, string appID)
        {
            _logger = logger;
            _appName = appName;
            _appID = appID;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _sessionManager = new QBSessionManager();
                
                // Open connection to QuickBooks
                _sessionManager.OpenConnection2(_appID, _appName, ENConnectionType.ctLocalQBD);
                
                // Begin session
                _sessionManager.BeginSession("", ENOpenMode.omDontCare);
                
                _isConnected = true;
                _logger.LogInformation("Successfully connected to QuickBooks Desktop");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to QuickBooks Desktop");
                _isConnected = false;
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_sessionManager != null && _isConnected)
                {
                    _sessionManager.EndSession();
                    _sessionManager.CloseConnection();
                    _isConnected = false;
                    _logger.LogInformation("Disconnected from QuickBooks Desktop");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from QuickBooks Desktop");
            }
        }

        public async Task<List<InvoiceData>> GetInvoicesAsync(DateTime? fromDate = null)
        {
            var invoices = new List<InvoiceData>();

            if (!_isConnected || _sessionManager == null)
            {
                _logger.LogWarning("Not connected to QuickBooks. Attempting to reconnect...");
                if (!await ConnectAsync())
                {
                    return invoices;
                }
            }

            try
            {
                var requestSet = _sessionManager.CreateMsgSetRequest("US", 13, 0);
                requestSet.Attributes.OnError = ENRqOnError.roeContinue;

                var invoiceQuery = requestSet.AppendInvoiceQueryRq();
                
                // Set date filter if provided
                if (fromDate.HasValue)
                {
                    invoiceQuery.ORInvoiceListQuery.InvoiceFilter.FromModifiedDate.SetValue(fromDate.Value);
                }

                // Include line items
                invoiceQuery.IncludeLineItems.SetValue(true);

                var responseSet = _sessionManager.DoRequests(requestSet);
                var response = responseSet.ResponseList.GetAt(0);

                if (response.StatusCode == 0)
                {
                    var invoiceRetList = response.Detail as IInvoiceRetList;
                    if (invoiceRetList != null)
                    {
                        for (int i = 0; i < invoiceRetList.Count; i++)
                        {
                            var invoiceRet = invoiceRetList.GetAt(i);
                            var invoice = await ConvertToInvoiceDataAsync(invoiceRet);
                            if (invoice != null)
                            {
                                invoices.Add(invoice);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError("QuickBooks query failed: {StatusMessage}", response.StatusMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices from QuickBooks");
            }

            return invoices;
        }

        private async Task<InvoiceData?> ConvertToInvoiceDataAsync(IInvoiceRet invoiceRet)
        {
            try
            {
                var invoice = new InvoiceData
                {
                    InvoiceNumber = invoiceRet.RefNumber?.GetValue() ?? "",
                    Date = invoiceRet.TxnDate?.GetValue() ?? DateTime.MinValue,
                    Amount = (decimal)(invoiceRet.Subtotal?.GetValue() ?? 0),
                    TaxAmount = (decimal)(invoiceRet.SalesTaxTotal?.GetValue() ?? 0),
                    TotalAmount = (decimal)(invoiceRet.TotalAmount?.GetValue() ?? 0),
                    DueDate = invoiceRet.DueDate?.GetValue(),
                    Memo = invoiceRet.Memo?.GetValue(),
                    Terms = invoiceRet.TermsRef?.FullName?.GetValue(),
                    Status = GetInvoiceStatus(invoiceRet),
                    CreatedDate = invoiceRet.TimeCreated?.GetValue() ?? DateTime.MinValue,
                    ModifiedDate = invoiceRet.TimeModified?.GetValue() ?? DateTime.MinValue
                };

                // Customer information
                if (invoiceRet.CustomerRef != null)
                {
                    invoice.Customer = new CustomerInfo
                    {
                        Id = invoiceRet.CustomerRef.ListID?.GetValue() ?? "",
                        Name = invoiceRet.CustomerRef.FullName?.GetValue() ?? ""
                    };
                }

                // Line items
                if (invoiceRet.ORInvoiceLineRetList != null)
                {
                    for (int i = 0; i < invoiceRet.ORInvoiceLineRetList.Count; i++)
                    {
                        var lineRet = invoiceRet.ORInvoiceLineRetList.GetAt(i);
                        if (lineRet.InvoiceLineRet != null)
                        {
                            var lineItem = new LineItem
                            {
                                ItemName = lineRet.InvoiceLineRet.ItemRef?.FullName?.GetValue() ?? "",
                                Description = lineRet.InvoiceLineRet.Desc?.GetValue(),
                                Quantity = (decimal)(lineRet.InvoiceLineRet.Quantity?.GetValue() ?? 0),
                                Rate = (decimal)(lineRet.InvoiceLineRet.Rate?.GetValue() ?? 0),
                                Amount = (decimal)(lineRet.InvoiceLineRet.Amount?.GetValue() ?? 0),
                                ItemType = lineRet.InvoiceLineRet.ItemRef?.FullName?.GetValue()
                            };
                            invoice.LineItems.Add(lineItem);
                        }
                    }
                }

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting QuickBooks invoice to InvoiceData");
                return null;
            }
        }

        private string GetInvoiceStatus(IInvoiceRet invoiceRet)
        {
            // Map QuickBooks status to our status
            if (invoiceRet.IsPaid?.GetValue() == true)
                return "Paid";
            if (invoiceRet.IsPending?.GetValue() == true)
                return "Pending";
            return "Open";
        }

        public void Dispose()
        {
            DisconnectAsync().Wait();
            _sessionManager = null;
        }
    }
}
