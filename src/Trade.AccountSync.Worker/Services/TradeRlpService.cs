using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Warren.Trade.Risk.ClientV2.Clients;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Services;

[ExcludeFromCodeCoverage]
public class TradeRlpService : ITradeRlpService
{
    private readonly ILogger<ITradeRlpService> _logger;
    private IRlpClient _rlpClient;

    public TradeRlpService(ILogger<TradeRlpService> logger,
                           IRlpClient rlpClient)
    {
        _logger = logger;
        _rlpClient = rlpClient;
    }

    public async Task SendActivationRequest(string sinacorId)
    {
        if (await _rlpClient.CheckIfExistsRlp(sinacorId))
        {
            _logger.LogInformation($"Customer {sinacorId} already has RLP.");
            return;
        }

        await _rlpClient.SendActivationRequest(sinacorId);

    }
}