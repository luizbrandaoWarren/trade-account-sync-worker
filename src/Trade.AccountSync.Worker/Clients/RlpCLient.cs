using Microsoft.Extensions.Logging;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services;
using Warren.Core.Http.Client.Implementation;
using Warren.Trade.Risk.Infra.Models;
using System.Diagnostics.CodeAnalysis;

namespace Warren.Trade.Risk.ClientV2.Clients
{
    [ExcludeFromCodeCoverage]
    public class RlpClient : HttpClientBase, IRlpClient
    {
        private readonly ILogger<IRlpClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INotificationService _notificationService;
        public RlpClient(
           ILogger<RlpClient> logger,
           IHttpClientFactory httpClientFactory,
           INotificationService notificationService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _notificationService = notificationService;
        }

        public async Task SendActivationRequest(string sinacorId)
        {
            var path = $"/api/v1/RlpUser/{sinacorId}/request-activation";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, path);
                request.Headers.Add("accept", "text/plain");
                var response = await SendAsync(HttpMethod.Put, path, request);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyError($"Error sending RLP activation for customer {sinacorId}", ex, _logger);                
            }
        }

        public async Task<bool> CheckIfExistsRlp(string sinacorId)
        {
            var path = $"/api/v1/RlpUser/{sinacorId}";

            try
            {
                var response = await GetAsync<RlpResponse>(path);
                return !string.IsNullOrEmpty(response.Status);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Customer {sinacorId} not found in RLP checking. Exception message: {ex.Message}");
                return false;
            }
        }
        protected override HttpClient CreateHttpClient()
        {
            return _httpClientFactory.CreateClient(nameof(RlpClient));
        }
    }
}
