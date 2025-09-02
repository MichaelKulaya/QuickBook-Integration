using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace QuickBooksETLService.Extensions
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath)
        {
            builder.Services.AddSingleton<ILoggerProvider>(provider =>
                new FileLoggerProvider(filePath));
            return builder;
        }
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _filePath);
        }

        public void Dispose()
        {
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _filePath;

        public FileLogger(string categoryName, string filePath)
        {
            _categoryName = categoryName;
            _filePath = filePath;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] [{_categoryName}] {message}";

            if (exception != null)
            {
                logEntry += $"\nException: {exception}";
            }

            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(_filePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently ignore file logging errors to prevent service crashes
            }
        }
    }
}
