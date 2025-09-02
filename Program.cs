using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Configuration;
using QuickBooksETLService.Services;
using System;
using System.IO;

namespace QuickBooksETLService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Check if running as console app or service
            if (args.Length > 0 && args[0] == "--console")
            {
                // Run as console application for debugging
                host.Run();
            }
            else
            {
                // Run as Windows Service
                host.RunAsService();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "QuickBooks ETL Service";
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    
                    // Add console logging for debugging
                    logging.AddConsole();
                    
                    // Add Windows Event Log
                    logging.AddEventLog(new EventLogSettings
                    {
                        SourceName = "QuickBooksETLService",
                        LogName = "Application"
                    });

                    // Add file logging if configured
                    var configuration = context.Configuration;
                    var logToFile = configuration.GetValue<bool>("QuickBooksETL:LogToFile", false);
                    if (logToFile)
                    {
                        var logPath = configuration.GetValue<string>("QuickBooksETL:LogFilePath", "C:\\Logs\\QuickBooksETL");
                        if (!Directory.Exists(logPath))
                        {
                            Directory.CreateDirectory(logPath);
                        }
                        
                        logging.AddFile($"{logPath}\\QuickBooksETL-{{Date}}.log");
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    
                    // Configure options
                    services.Configure<ETLServiceOptions>(
                        configuration.GetSection(ETLServiceOptions.SectionName));

                    // Register HttpClient
                    services.AddHttpClient<WebhookService>(client =>
                    {
                        var timeout = configuration.GetValue<int>("QuickBooksETL:HttpTimeoutSeconds", 30);
                        client.Timeout = TimeSpan.FromSeconds(timeout);
                        client.DefaultRequestHeaders.Add("User-Agent", "QuickBooks-ETL-Service/1.0");
                    });

                    // Register services
                    services.AddSingleton<QuickBooksService>(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<QuickBooksService>>();
                        var appName = configuration.GetValue<string>("QuickBooksETL:QuickBooksAppName", "QuickBooks ETL Service");
                        var appID = configuration.GetValue<string>("QuickBooksETL:QuickBooksAppID", "YourAppID");
                        return new QuickBooksService(logger, appName, appID);
                    });

                    services.AddScoped<WebhookService>(provider =>
                    {
                        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                        var logger = provider.GetRequiredService<ILogger<WebhookService>>();
                        var webhookEndpoint = configuration.GetValue<string>("QuickBooksETL:WebhookEndpoint", "");
                        var maxRetries = configuration.GetValue<int>("QuickBooksETL:MaxRetryAttempts", 3);
                        var retryDelay = configuration.GetValue<int>("QuickBooksETL:RetryDelaySeconds", 5);
                        
                        return new WebhookService(httpClient, logger, webhookEndpoint, maxRetries, retryDelay);
                    });

                    // Register the main ETL service
                    services.AddHostedService<ETLService>();
                });
    }
}
