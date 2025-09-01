using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QuickBooksETLService.Models;

namespace QuickBooksETLService.Services
{
    public class WebhookService : IWebhookService, IDisposable
    {
        private readonly ILogger<WebhookService> _logger;
        private readonly WebhookSettings _settings;
        private readonly ServiceSettings _serviceSettings;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSettings;

        public WebhookService(ILogger<WebhookService> logger, IOptions<ServiceConfiguration> configuration)
        {
            _logger = logger;
            _settings = configuration.Value.WebhookSettings;
            _serviceSettings = configuration.Value.ServiceSettings;
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
            
            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        }

        public async Task<bool> SendInvoiceAsync(Invoice invoice)
        {
            try
            {
                _logger.LogInformation("Sending invoice {InvoiceNumber} to webhook", invoice.InvoiceNumber);

                var json = JsonConvert.SerializeObject(invoice, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, _settings.ContentType);

                var response = await _httpClient.PostAsync(_settings.EndpointUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent invoice {InvoiceNumber} to webhook", invoice.InvoiceNumber);
                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send invoice {InvoiceNumber}. Status: {StatusCode}, Response: {Response}", 
                        invoice.InvoiceNumber, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice {InvoiceNumber} to webhook", invoice.InvoiceNumber);
                return false;
            }
        }

        public async Task<bool> SendInvoicesAsync(List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
            {
                _logger.LogInformation("No invoices to send");
                return true;
            }

            _logger.LogInformation("Sending {Count} invoices to webhook", invoices.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var invoice in invoices)
            {
                var success = await SendInvoiceWithRetryAsync(invoice);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }

                // Small delay between requests to avoid overwhelming the endpoint
                await Task.Delay(100);
            }

            _logger.LogInformation("Completed sending invoices. Success: {SuccessCount}, Failures: {FailureCount}", 
                successCount, failureCount);

            return failureCount == 0;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing webhook connection to {EndpointUrl}", _settings.EndpointUrl);

                var testData = new { test = true, timestamp = DateTime.UtcNow };
                var json = JsonConvert.SerializeObject(testData, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, _settings.ContentType);

                var response = await _httpClient.PostAsync(_settings.EndpointUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook connection test successful");
                    return true;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Webhook connection test failed. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook connection test failed");
                return false;
            }
        }

        private async Task<bool> SendInvoiceWithRetryAsync(Invoice invoice)
        {
            for (int attempt = 1; attempt <= _serviceSettings.MaxRetryAttempts; attempt++)
            {
                try
                {
                    var success = await SendInvoiceAsync(invoice);
                    if (success)
                    {
                        return true;
                    }

                    if (attempt < _serviceSettings.MaxRetryAttempts)
                    {
                        _logger.LogWarning("Attempt {Attempt} failed for invoice {InvoiceNumber}. Retrying in {Delay} seconds...", 
                            attempt, invoice.InvoiceNumber, _serviceSettings.RetryDelaySeconds);
                        
                        await Task.Delay(TimeSpan.FromSeconds(_serviceSettings.RetryDelaySeconds));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Attempt {Attempt} failed with exception for invoice {InvoiceNumber}", 
                        attempt, invoice.InvoiceNumber);
                    
                    if (attempt < _serviceSettings.MaxRetryAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_serviceSettings.RetryDelaySeconds));
                    }
                }
            }

            _logger.LogError("Failed to send invoice {InvoiceNumber} after {MaxAttempts} attempts", 
                invoice.InvoiceNumber, _serviceSettings.MaxRetryAttempts);
            return false;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
} 