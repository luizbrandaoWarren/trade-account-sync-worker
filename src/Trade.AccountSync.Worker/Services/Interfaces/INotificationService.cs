using Microsoft.Extensions.Logging;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public interface INotificationService
    {
        Task NotifyError(string errorMessage, ILogger logger);

        Task NotifyError(string errorMessage, Exception ex, ILogger logger);

        Task NotifyWarning(string warningMessage, ILogger logger);
    }
}
