using Microsoft.Extensions.Logging;
using Warren.Core.Slack.Client.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class NotificationService : INotificationService
    {
        private readonly string WARNING_CHANNEL = "#trade-warnings";
        private readonly string USER_NAME = "Risk Client Monitor";
        private readonly string EMOJI = ":warning:";
        private readonly ISlackClient _slackClient;

        public NotificationService(ISlackClient slackClient)
        {
            _slackClient = slackClient;
        }

        public async Task NotifyError(string errorMessage, ILogger logger)
        {
            await _slackClient.SendToChannelAsUserAsync(BuildErrorMessage(errorMessage), WARNING_CHANNEL, USER_NAME, EMOJI);
            logger.LogError(errorMessage);
        }

        public async Task NotifyError(string errorMessage, Exception ex, ILogger logger)
        {
            await _slackClient.SendToChannelAsUserAsync(BuildErrorMessage(errorMessage), WARNING_CHANNEL, USER_NAME, EMOJI);
            logger.LogError(ex, errorMessage, ex);
        }

        public async Task NotifyWarning(string warningMessage, ILogger logger)
        {
            await _slackClient.SendToChannelAsUserAsync(BuildWarningMessage(warningMessage), WARNING_CHANNEL, USER_NAME, EMOJI);
            logger.LogWarning(warningMessage);
        }

        private static string BuildErrorMessage(string message) => BuildMessage(message, "error");

        private static string BuildWarningMessage(string message) => BuildMessage(message, "warning");

        private static string BuildMessage(string message, string level)
        {
            var icon = level == "error"
                ? ":exclamation:"
                : ":warning:";

            return $"{icon} *{message}* {icon}";                
        }
    }
}
