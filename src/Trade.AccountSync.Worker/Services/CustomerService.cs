using Microsoft.Extensions.Logging;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services;
public class CustomerService : ICustomerService
{
    private ILogger<CustomerService> _logger;
    private ITradeCacheService _tradeCacheService;
    private ISinacorAccountUpdateService _sinacorAccountUpdateService;
    private readonly IHomebrokerUserRegisterService _homebrokerUserRegisterService;
    private readonly ICustomerClient _customerClient;
    private readonly ITradeRlpService _tradeRlpService;
    private readonly ITradeCustomerPersistenceService _tradeCustomerPersistenceService;
    private bool _disposed;

    public CustomerService(
        ILogger<CustomerService> logger,
        ITradeCacheService tradeCacheService,
        ISinacorAccountUpdateService sinacorAccountUpdateService,
        IHomebrokerUserRegisterService homebrokerUserRegisterService,
        ICustomerClient customerClient,
        ITradeRlpService tradeRlpService,
        ITradeCustomerPersistenceService tradeCustomerPersistenceService)
    {
        _logger = logger;
        _tradeCacheService = tradeCacheService;
        _sinacorAccountUpdateService = sinacorAccountUpdateService;
        _homebrokerUserRegisterService = homebrokerUserRegisterService;
        _customerClient = customerClient;
        _tradeRlpService = tradeRlpService;
        _tradeCustomerPersistenceService = tradeCustomerPersistenceService;
    }

    public async Task UpdateSinacorAccountAsync(string customerApiId, int sinacorId)
    {
        ValidateInput(customerApiId);

        var tradeCustomer = await GetTradeCustomerAsync(customerApiId);
        var summaryCustomer = await _customerClient.GetCoreCustomer(customerApiId);

        if (tradeCustomer is null && summaryCustomer is null)
        {
            _logger.LogInformation(
               "The customer {customerApiId} is not the trade type, therefore it cannot be processed",
               customerApiId);
            return;
        }

        if (tradeCustomer is null && summaryCustomer is not null)
        {
            await SaveTradeCustomerAsync(summaryCustomer.TradePortfolioId, sinacorId, customerApiId);
            tradeCustomer = await GetTradeCustomerAsync(customerApiId);
        }

        if (tradeCustomer is null)
        {
            _logger.LogInformation(
               "The customer {customerApiId} is not the trade type, therefore it cannot be processed",
               customerApiId);
            return;
        }

        if (summaryCustomer is null)
        {
            _logger.LogInformation(
                "The customer {customerApiId} not found",
                customerApiId);
            return;
        }      

        await _homebrokerUserRegisterService.RegisterAsync(summaryCustomer, customerApiId, sinacorId);
        await _tradeRlpService.SendActivationRequest(sinacorId.ToString());
        await _sinacorAccountUpdateService.UpdateAsync(customerApiId, sinacorId);       
    }

    private async Task SaveTradeCustomerAsync(int? tradePortfolioId, int externalId, string customerApiId)
    {
        if (tradePortfolioId is null)
        {
            _logger.LogInformation("Unable to save trade customer given that portfolio id is null");
            return;
        }

        await _tradeCustomerPersistenceService.SaveTradeCustomer(customerApiId, externalId, (int)tradePortfolioId);
        _logger.LogInformation("Saved trade customer with Api Id: {customerApiId}", customerApiId);
    }

    private async Task<TradeCustomer?> GetTradeCustomerAsync(string customerApiId)
    {
        return await _tradeCacheService.GetTradeCustomerAsync(customerApiId);
    }

    private static void ValidateInput(string customerApiId)
    {
        if (!GuardClause.IsNullOrEmpty(customerApiId))
        {
            return;
        }

        throw new ArgumentException($"{nameof(customerApiId)} cannot be null or empty");
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
            _tradeCacheService = null!;
            _sinacorAccountUpdateService.Dispose();
            _sinacorAccountUpdateService = null!;
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
