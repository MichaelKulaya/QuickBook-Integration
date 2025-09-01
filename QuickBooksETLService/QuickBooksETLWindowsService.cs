using System;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuickBooksETLService
{
    public partial class QuickBooksETLWindowsService : ServiceBase
    {
        private readonly IHost _host;
        private readonly ILogger<QuickBooksETLWindowsService> _logger;

        public QuickBooksETLWindowsService()
        {
            ServiceName = "QuickBooksETLService";
            
            // Build the host
            _host = Host.CreateDefaultBuilder()
                .UseWindowsService(options =>
                {
                    options.ServiceName = "QuickBooksETLService";
                })
                .ConfigureServices((context, services) =>
                {
                    // Register services
                    services.Configure<QuickBooksETLService.Models.ServiceConfiguration>(context.Configuration);

                    services.AddSingleton<Services.IQuickBooksService, Services.QuickBooksService>();
                    services.AddSingleton<Services.IWebhookService, Services.WebhookService>();
                    services.AddSingleton<Services.IETLService, Services.ETLService>();
                    services.AddHostedService<Services.ETLService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddEventLog();
                    logging.AddConsole();
                })
                .Build();

            _logger = _host.Services.GetRequiredService<ILogger<QuickBooksETLWindowsService>>();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _logger.LogInformation("Starting QuickBooks ETL Windows Service...");
                
                _host.Start();
                
                _logger.LogInformation("QuickBooks ETL Windows Service started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start QuickBooks ETL Windows Service");
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _logger.LogInformation("Stopping QuickBooks ETL Windows Service...");
                
                _host.StopAsync(TimeSpan.FromSeconds(30)).Wait();
                
                _logger.LogInformation("QuickBooks ETL Windows Service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping QuickBooks ETL Windows Service");
            }
        }

        protected override void OnShutdown()
        {
            try
            {
                _logger.LogInformation("QuickBooks ETL Windows Service shutting down...");
                
                _host.StopAsync(TimeSpan.FromSeconds(30)).Wait();
                
                _logger.LogInformation("QuickBooks ETL Windows Service shutdown completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during QuickBooks ETL Windows Service shutdown");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _host?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 