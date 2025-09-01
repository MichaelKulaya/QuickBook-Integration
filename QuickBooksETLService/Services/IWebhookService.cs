using System.Collections.Generic;
using System.Threading.Tasks;
using QuickBooksETLService.Models;

namespace QuickBooksETLService.Services
{
    public interface IWebhookService
    {
        Task<bool> SendInvoiceAsync(Invoice invoice);
        Task<bool> SendInvoicesAsync(List<Invoice> invoices);
        Task<bool> TestConnectionAsync();
    }
} 