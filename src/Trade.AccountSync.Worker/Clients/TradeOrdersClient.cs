using Microsoft.Extensions.Logging;
using System.Net;
using Warren.Core.Http.Client.Exceptions;
using Warren.Core.Http.Client.Implementation;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Clients;
public class TradeOrdersClient : HttpClientBase, ITradeOrdersClient
{
    private readonly ILogger<TradeOrdersClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public TradeOrdersClient(
        ILogger<TradeOrdersClient> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task DeleteCustomerCacheAsync(string customerApiId)
    {
        var uri = $"api/customer-cache/{customerApiId}";

        try
        {
            await DeleteAsync(uri);
        }
        catch (RequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No cache found for customer {customerApiId}", customerApiId);
        }
    }

    protected override HttpClient CreateHttpClient()
    {
        return _httpClientFactory.CreateClient(nameof(TradeOrdersClient));
    }
}
