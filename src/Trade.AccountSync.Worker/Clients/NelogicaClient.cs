using Flurl;
using Microsoft.Extensions.Logging;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;
using Warren.Trade.Risk.Infra.Parsers;

namespace Warren.Trade.Risk.ClientV2.Clients
{
    public class NelogicaClient : INelogicaClient
    {
        private readonly IResilientlyHttpClient _resilientlyHttpClient;
        private readonly ILogger<INelogicaClient> _logger;
        private readonly INotificationService _notificationService;

        private const string CONTENT_TYPE = "application/json";
        private const string NELOGICA_URL_PATH_KEY = "nelogica:BaseUrlRoute";
        private const string NELOGICA_ORIGIN_PATH_KEY = "nelogica:AuthorizedOrigin";

        private readonly string _nelogicaUrl;
        private readonly string _nelogicaOrigin;

        public NelogicaClient(
            IResilientlyHttpClient resilientlyHttpClient,
            IAppConfig appConfig,
            ILogger<NelogicaClient> logger,
            INotificationService notificationService)
        {
            _resilientlyHttpClient = resilientlyHttpClient;
            _nelogicaUrl = appConfig.GetConfig(NELOGICA_URL_PATH_KEY);
            _nelogicaOrigin = appConfig.GetConfig(NELOGICA_ORIGIN_PATH_KEY);
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<ParsedResponseMessage> MakeRequest(int customerId, object data)
        {
            var fullPath = Url.Combine(_nelogicaUrl, "request.php");

            var header = new
            {
                Content_Type = CONTENT_TYPE,
                Origin = _nelogicaOrigin
            };

            ParsedResponseMessage result = null;

            try
            {
                int retries = 3;
                while (retries > 0)
                {
                    var responseMessage = await _resilientlyHttpClient.PostAsync(fullPath, data, header);
                    result = responseMessage?.ParseResponse();

                    _logger.LogInformation($"Request sent for {customerId}: {data}");

                    if (result.IsError && result.ResponseContent.Contains("Erro no cadastramento da request"))
                    {
                        retries--;
                        Thread.Sleep(1000);
                        continue;
                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyError($"Request to external system failed - Customer {customerId}", ex, _logger);
            }

            return result;
        }
    }
}
