using System.Threading;
using System.Threading.Tasks;

namespace QuickBooksETLService.Services
{
    public interface IETLService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task ProcessInvoicesAsync(CancellationToken cancellationToken);
    }
} 