using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickBooksETLService.Models;

namespace QuickBooksETLService.Services
{
    public class ETLService : BackgroundService, IETLService
    {
        private readonly ILogger<ETLService> _logger;
        private readonly IQuickBooksService _quickBooksService;
        private readonly IWebhookService _webhookService;
        private readonly ServiceSettings _serviceSettings;
        private readonly Timer _pollingTimer;
        private DateTime _lastProcessedDate = DateTime.MinValue;

        public ETLService(
            ILogger<ETLService> logger,
            IQuickBooksService quickBooksService,
            IWebhookService webhookService,
            IOptions<ServiceConfiguration> configuration)
        {
            _logger = logger;
            _quickBooksService = quickBooksService;
            _webhookService = webhookService;
            _serviceSettings = configuration.Value.ServiceSettings;
            
            _pollingTimer = new Timer(ProcessInvoicesCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting QuickBooks ETL Service...");

                // Test connections
                await TestConnectionsAsync();

                // Start the polling timer
                var pollingInterval = TimeSpan.FromMinutes(_serviceSettings.PollingIntervalMinutes);
                _pollingTimer.Change(TimeSpan.Zero, pollingInterval);

                _logger.LogInformation("QuickBooks ETL Service started successfully. Polling every {Interval} minutes", 
                    _serviceSettings.PollingIntervalMinutes);

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ETL Service execution");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping QuickBooks ETL Service...");

            // Stop the polling timer
            _pollingTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            // Disconnect from QuickBooks
            await _quickBooksService.DisconnectAsync();

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("QuickBooks ETL Service stopped");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ETL Service starting...");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ETL Service stopping...");
            await Task.CompletedTask;
        }

        public async Task ProcessInvoicesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting invoice processing cycle...");

                // Connect to QuickBooks if not already connected
                if (!await _quickBooksService.IsConnectedAsync())
                {
                    if (!await _quickBooksService.ConnectAsync())
                    {
                        _logger.LogError("Failed to connect to QuickBooks. Skipping this processing cycle.");
                        return;
                    }
                }

                // Get invoices since last processed date
                var invoices = await _quickBooksService.GetInvoicesAsync(_lastProcessedDate);
                
                if (invoices.Count == 0)
                {
                    _logger.LogInformation("No new invoices found since {LastProcessedDate}", _lastProcessedDate);
                    return;
                }

                _logger.LogInformation("Found {Count} new invoices to process", invoices.Count);

                // Send invoices to webhook
                var success = await _webhookService.SendInvoicesAsync(invoices);
                
                if (success)
                {
                    // Update last processed date to the most recent invoice date
                    _lastProcessedDate = invoices.Max(i => i.Date);
                    _logger.LogInformation("Successfully processed {Count} invoices. Last processed date: {LastProcessedDate}", 
                        invoices.Count, _lastProcessedDate);
                }
                else
                {
                    _logger.LogWarning("Some invoices failed to process. Will retry in next cycle.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during invoice processing cycle");
            }
        }

        private async void ProcessInvoicesCallback(object state)
        {
            try
            {
                await ProcessInvoicesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in invoice processing callback");
            }
        }

        private async Task TestConnectionsAsync()
        {
            try
            {
                _logger.LogInformation("Testing service connections...");

                // Test QuickBooks connection
                var qbConnected = await _quickBooksService.ConnectAsync();
                if (qbConnected)
                {
                    _logger.LogInformation("QuickBooks connection test successful");
                }
                else
                {
                    _logger.LogWarning("QuickBooks connection test failed");
                }

                // Test webhook connection
                var webhookConnected = await _webhookService.TestConnectionAsync();
                if (webhookConnected)
                {
                    _logger.LogInformation("Webhook connection test successful");
                }
                else
                {
                    _logger.LogWarning("Webhook connection test failed");
                }

                if (!qbConnected || !webhookConnected)
                {
                    _logger.LogWarning("Some connection tests failed. Service will continue but may not function properly.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during connection testing");
            }
        }

        public override void Dispose()
        {
            _pollingTimer?.Dispose();
            base.Dispose();
        }
    }
} 