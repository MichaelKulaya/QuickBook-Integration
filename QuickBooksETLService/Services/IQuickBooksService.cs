using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuickBooksETLService.Models;

namespace QuickBooksETLService.Services
{
    public interface IQuickBooksService
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<bool> IsConnectedAsync();
        Task<List<Invoice>> GetInvoicesAsync(DateTime? sinceDate = null);
        Task<List<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
} 