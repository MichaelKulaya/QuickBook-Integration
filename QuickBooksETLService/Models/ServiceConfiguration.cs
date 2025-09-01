namespace QuickBooksETLService.Models
{
    public class ServiceConfiguration
    {
        public ServiceSettings ServiceSettings { get; set; } = new ServiceSettings();
        public QuickBooksSettings QuickBooksSettings { get; set; } = new QuickBooksSettings();
        public WebhookSettings WebhookSettings { get; set; } = new WebhookSettings();
    }

    public class ServiceSettings
    {
        public string ServiceName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PollingIntervalMinutes { get; set; } = 5;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 30;
    }

    public class QuickBooksSettings
    {
        public string CompanyFile { get; set; } = string.Empty;
        public int ConnectionTimeoutSeconds { get; set; } = 30;
        public int QueryLimit { get; set; } = 100;
    }

    public class WebhookSettings
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 60;
        public string ContentType { get; set; } = "application/json";
    }
} 