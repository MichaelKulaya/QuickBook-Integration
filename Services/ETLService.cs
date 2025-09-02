using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickBooksETLService.Models;

namespace QuickBooksETLService.Services
{
    public class ETLService : BackgroundService
    {
        private readonly ILogger<ETLService> _logger;
        private readonly QuickBooksService _quickBooksService;
        private readonly WebhookService _webhookService;
        private readonly ETLServiceOptions _options;
        private DateTime _lastProcessedDate;

        public ETLService(
            ILogger<ETLService> logger,
            QuickBooksService quickBooksService,
            WebhookService webhookService,
            IOptions<ETLServiceOptions> options)
        {
            _logger = logger;
            _quickBooksService = quickBooksService;
            _webhookService = webhookService;
            _options = options.Value;
            _lastProcessedDate = DateTime.Now.AddDays(-1); // Start from yesterday
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QuickBooks ETL Service started");

            // Initial connection to QuickBooks
            if (!await _quickBooksService.ConnectAsync())
            {
                _logger.LogError("Failed to connect to QuickBooks. Service will retry periodically.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessInvoicesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during invoice processing cycle");
                }

                // Wait for the specified interval before next processing
                var delay = TimeSpan.FromMinutes(_options.PollingIntervalMinutes);
                _logger.LogInformation("Waiting {Minutes} minutes before next processing cycle", _options.PollingIntervalMinutes);
                
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
            }

            _logger.LogInformation("QuickBooks ETL Service stopped");
        }

        private async Task ProcessInvoicesAsync()
        {
            try
            {
                _logger.LogInformation("Starting invoice processing cycle");

                // Get invoices from QuickBooks
                var invoices = await _quickBooksService.GetInvoicesAsync(_lastProcessedDate);
                
                if (invoices.Count == 0)
                {
                    _logger.LogInformation("No new invoices found since {LastProcessedDate}", _lastProcessedDate);
                    return;
                }

                _logger.LogInformation("Found {Count} new invoices to process", invoices.Count);

                // Filter out invoices that might have been processed already
                var newInvoices = invoices.Where(i => i.ModifiedDate > _lastProcessedDate).ToList();

                if (newInvoices.Count == 0)
                {
                    _logger.LogInformation("No new invoices after filtering by modification date");
                    return;
                }

                _logger.LogInformation("Processing {Count} filtered invoices", newInvoices.Count);

                // Send invoices to webhook
                var success = await _webhookService.SendInvoicesBatchAsync(newInvoices);

                if (success)
                {
                    _lastProcessedDate = newInvoices.Max(i => i.ModifiedDate);
                    _logger.LogInformation("Successfully processed {Count} invoices. Last processed date updated to {LastProcessedDate}", 
                        newInvoices.Count, _lastProcessedDate);
                }
                else
                {
                    _logger.LogWarning("Some invoices failed to process. Will retry in next cycle");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during invoice processing");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping QuickBooks ETL Service...");
            
            try
            {
                await _quickBooksService.DisconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from QuickBooks during shutdown");
            }

            await base.StopAsync(cancellationToken);
        }
    }

    public class ETLServiceOptions
    {
        public const string SectionName = "QuickBooksETL";

        public string WebhookEndpoint { get; set; } = string.Empty;
        public int PollingIntervalMinutes { get; set; } = 5;
        public string QuickBooksAppName { get; set; } = string.Empty;
        public string QuickBooksAppID { get; set; } = string.Empty;
        public string QuickBooksCompanyFile { get; set; } = string.Empty;
        public int HttpTimeoutSeconds { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public bool LogToFile { get; set; } = true;
        public string LogFilePath { get; set; } = string.Empty;
    }
}
