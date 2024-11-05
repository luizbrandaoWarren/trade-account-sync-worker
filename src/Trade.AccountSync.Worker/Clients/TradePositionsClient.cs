using Warren.Core.Http.Client.Exceptions;
using Warren.Core.Http.Client.Implementation;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.Infra.Exceptions;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Clients;
public class TradePositionsClient : HttpClientBase, ITradePositionsClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TradePositionsClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task UpdateCustodyAsync(int sinacorId)
    {
        var uri = "api/position/customersall?processAll=false";
        var body = new string[]
        {
            sinacorId.ToString()
        };
        try
        {
            _ = await PostAsync<IEnumerable<string>, UpdateCustodyResponse>(uri, body);
        }
        catch (RequestException exception)
        {
            throw new CustodyNotUpdatedException("An error occurred while performing the custody update", exception);
        }
    }

    protected override HttpClient CreateHttpClient()
    {
        return _httpClientFactory.CreateClient(nameof(TradePositionsClient));
    }
}
