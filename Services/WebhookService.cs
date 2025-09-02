using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuickBooksETLService.Models;

namespace QuickBooksETLService.Services
{
    public class WebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebhookService> _logger;
        private readonly string _webhookEndpoint;
        private readonly int _maxRetryAttempts;
        private readonly int _retryDelaySeconds;

        public WebhookService(HttpClient httpClient, ILogger<WebhookService> logger, 
            string webhookEndpoint, int maxRetryAttempts = 3, int retryDelaySeconds = 5)
        {
            _httpClient = httpClient;
            _logger = logger;
            _webhookEndpoint = webhookEndpoint;
            _maxRetryAttempts = maxRetryAttempts;
            _retryDelaySeconds = retryDelaySeconds;
        }

        public async Task<bool> SendInvoiceAsync(InvoiceData invoice)
        {
            var attempt = 0;
            Exception? lastException = null;

            while (attempt < _maxRetryAttempts)
            {
                try
                {
                    attempt++;
                    _logger.LogInformation("Sending invoice {InvoiceNumber} to webhook (attempt {Attempt}/{MaxAttempts})", 
                        invoice.InvoiceNumber, attempt, _maxRetryAttempts);

                    var json = JsonConvert.SerializeObject(invoice, Formatting.Indented);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(_webhookEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("Successfully sent invoice {InvoiceNumber} to webhook. Response: {Response}", 
                            invoice.InvoiceNumber, responseContent);
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Webhook returned error status {StatusCode} for invoice {InvoiceNumber}. Response: {Response}", 
                            response.StatusCode, invoice.InvoiceNumber, errorContent);
                        
                        if (attempt < _maxRetryAttempts)
                        {
                            _logger.LogInformation("Retrying in {DelaySeconds} seconds...", _retryDelaySeconds);
                            await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds));
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, "HTTP error sending invoice {InvoiceNumber} to webhook (attempt {Attempt}/{MaxAttempts})", 
                        invoice.InvoiceNumber, attempt, _maxRetryAttempts);
                    
                    if (attempt < _maxRetryAttempts)
                    {
                        _logger.LogInformation("Retrying in {DelaySeconds} seconds...", _retryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds));
                    }
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, "Timeout sending invoice {InvoiceNumber} to webhook (attempt {Attempt}/{MaxAttempts})", 
                        invoice.InvoiceNumber, attempt, _maxRetryAttempts);
                    
                    if (attempt < _maxRetryAttempts)
                    {
                        _logger.LogInformation("Retrying in {DelaySeconds} seconds...", _retryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds));
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, "Unexpected error sending invoice {InvoiceNumber} to webhook (attempt {Attempt}/{MaxAttempts})", 
                        invoice.InvoiceNumber, attempt, _maxRetryAttempts);
                    
                    if (attempt < _maxRetryAttempts)
                    {
                        _logger.LogInformation("Retrying in {DelaySeconds} seconds...", _retryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds));
                    }
                }
            }

            _logger.LogError(lastException, "Failed to send invoice {InvoiceNumber} to webhook after {MaxAttempts} attempts", 
                invoice.InvoiceNumber, _maxRetryAttempts);
            return false;
        }

        public async Task<bool> SendInvoicesBatchAsync(List<InvoiceData> invoices)
        {
            var successCount = 0;
            var failureCount = 0;

            _logger.LogInformation("Starting batch send of {Count} invoices to webhook", invoices.Count);

            foreach (var invoice in invoices)
            {
                if (await SendInvoiceAsync(invoice))
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }

            _logger.LogInformation("Batch send completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                successCount, failureCount);

            return failureCount == 0;
        }
    }
}
