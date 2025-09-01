using System;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuickBooksETLService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Check if running as a service or console
            if (args.Length > 0 && args[0].Equals("--console", StringComparison.OrdinalIgnoreCase))
            {
                // Run as console application for debugging
                await RunAsConsole();
            }
            else
            {
                // Run as Windows Service
                ServiceBase.Run(new QuickBooksETLWindowsService());
            }
        }

        static async Task RunAsConsole()
        {
            Console.WriteLine("QuickBooks ETL Service - Console Mode");
            Console.WriteLine("Press Ctrl+C to exit");
            Console.WriteLine();

            try
            {
                // Build the host
                var host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // Register services
                        services.Configure<Models.ServiceConfiguration>(context.Configuration);

                        services.AddSingleton<Services.IQuickBooksService, Services.QuickBooksService>();
                        services.AddSingleton<Services.IWebhookService, Services.WebhookService>();
                        services.AddSingleton<Services.IETLService, Services.ETLService>();
                        services.AddHostedService<Services.ETLService>();
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        // File logging will be handled by Serilog in production
                    })
                    .Build();

                // Start the host
                host.Start();

                Console.WriteLine("Service started successfully. Press Ctrl+C to stop.");
                Console.WriteLine("Check the logs for detailed information.");

                // Wait for Ctrl+C
                var cts = new System.Threading.CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                // Keep running until cancelled
                try
                {
                    // Wait for cancellation
                    while (!cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when Ctrl+C is pressed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
} 