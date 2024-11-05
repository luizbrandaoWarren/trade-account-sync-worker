using Microsoft.Extensions.Logging;
using Warren.Core.Messaging.Providers.Kafka.Producers;
using Warren.Core.Messaging.Risk.Contracts.Models;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra.Exceptions;

namespace Warren.Trade.Risk.ClientV2.Services;
public class SinacorAccountUpdateService : ISinacorAccountUpdateService
{
    private ILogger<SinacorAccountUpdateService> _logger;
    private ICacheManagerService _cacheManagerService;
    private IKafkaMessageProducer<CreateCustomerRequestMessage> _messageProducer;
    private ITradePositionsClient _positionClient;
    private bool _disposed;

    public SinacorAccountUpdateService(
        ILogger<SinacorAccountUpdateService> logger,
        ICacheManagerService cacheManagerService,
        IKafkaMessageProducer<CreateCustomerRequestMessage> messageProducer,
        ITradePositionsClient positionsClient)
    {
        _logger = logger;
        _cacheManagerService = cacheManagerService;
        _messageProducer = messageProducer;
        _positionClient = positionsClient;
    }

    public async Task UpdateAsync(string customerApiId, int sinacorId)
    {
        await _cacheManagerService.PurgeCachesAsync(customerApiId);
        await ProduceCreateCustomerMessageAsync(customerApiId);
        await _positionClient.UpdateCustodyAsync(sinacorId);
    }

    public async Task ProduceCreateCustomerMessageAsync(string customerApiId)
    {
        var message = new CreateCustomerRequestMessage
        {
            Identifier = Guid.NewGuid(),
            ApiId = customerApiId,
        };

        var result = await _messageProducer.ProduceMessageAsync(message);
        if (result.Error is not null && result.Error.IsError)
        {
            var messageError = "An error occurred while producing the message. Reason: ";
            _logger.LogError(messageError + "{reason}", result.Error.Reason);

            throw new UnproducedMessageException(messageError + result.Error.Reason);
        }

        _logger.LogInformation(
                   "Message successfully sent to topic {topic} partition {partition} and offset {offset}",
                   result.Topic,
                   result.Partition.Value,
                   result.Offset.Value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _logger = null!;
            _cacheManagerService = null!;
            _messageProducer = null!;
            _positionClient = null!;
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
