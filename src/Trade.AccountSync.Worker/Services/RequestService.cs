using Microsoft.Extensions.Logging;
using Warren.Core.Messaging.Consumers;
using Warren.Core.Messaging.Risk.Contracts.Models;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Enums;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class RequestService : IRequestService
    {
        #region Properties
        private readonly ICustomerClient _customerClient;
        private readonly IHomebrokerUserRegisterService _homebrokerUserRegisterService;
        private readonly IAccountRiskRegisterService _accountRiskRegisterService;
        private readonly IAccountRiskUpdaterService _accountRiskUpdaterService;
        private readonly ISuitabilityProfileRegisterService _suitabilityProfileRegisterService;
        private readonly INotificationService _notificationService;
        private readonly bool _forceRiskAccountUpdate;
        private readonly ITradeCustomerPersistenceService _tradeCustomerPersistenceService;
        private readonly ITradeBalanceUpdater _tradeBalanceUpdater;
        private readonly ILogger<RequestService> _logger;
        private readonly ITradeRlpService _tradeRlpService;

        private const string ForceRiskAccountUpdate = "nelogica:ForceRiskAccountUpdate";

        #endregion

        #region Constructor

        public RequestService(
            ICustomerClient customerClient,
            IHomebrokerUserRegisterService homebrokerUserRegisterService,
            IAccountRiskRegisterService accountRiskRegisterService,
            IAccountRiskUpdaterService accountRiskUpdaterService,
            ISuitabilityProfileRegisterService suitabilityProfileRegisterService,
            INotificationService notificationService,
            ILogger<RequestService> logger,
            IAppConfig appConfig,
            ITradeCustomerPersistenceService tradeCustomerPersistenceService,
            ITradeBalanceUpdater tradeBalanceUpdater,
            ITradeRlpService tradeRlpService)
        {
            _customerClient = customerClient;

            _homebrokerUserRegisterService = homebrokerUserRegisterService;
            _accountRiskRegisterService = accountRiskRegisterService;
            _accountRiskUpdaterService = accountRiskUpdaterService;
            _suitabilityProfileRegisterService = suitabilityProfileRegisterService;

            _notificationService = notificationService;
            _logger = logger;
            _forceRiskAccountUpdate = appConfig.GetConfig<bool>(ForceRiskAccountUpdate);
            _tradeCustomerPersistenceService = tradeCustomerPersistenceService;
            _tradeBalanceUpdater = tradeBalanceUpdater;
            _tradeRlpService = tradeRlpService;
        }

        #endregion

        #region Methods

        public async Task ProcessCustomerRegister(MessageConsumerResult<CreateCustomerRequestMessage> result)
        {
            if (result is null)
            {
                return;
            }

            var customerApiId = result.Message.ApiId;

            try
            {
                _logger.LogInformation("Processing creation request at external system for customer {customerApiId}", customerApiId);
                var customer = await GetCustomerAsync(customerApiId).ConfigureAwait(false);
                if (customer is null)
                {
                    _logger.LogError("Customer {customerApiId} not found on core", customerApiId);
                }
                else
                {                 
                    _logger.LogInformation("Got customer {customerApiId} from core", customerApiId);

                    var sinacorIds = customer.SinacorIds ?? Enumerable.Empty<int>();
                    var sinacorMasterAccount = customer.Id;

                    await ProcessRegistrationForMasterAccountAsync(
                        customer,
                        customerApiId,
                        sinacorMasterAccount).ConfigureAwait(false);

                    var secundaryAccounts = GetSecundaryAccounts(sinacorIds, sinacorMasterAccount);

                    await ProcessRegistrationForSecundaryAccountsAsync(
                        customer,
                        customerApiId,
                        secundaryAccounts).ConfigureAwait(false);
                    
                    await _tradeRlpService.SendActivationRequest(sinacorMasterAccount.ToString());
                }

                _logger.LogInformation("Customer register request processed for {customerApiId}", customerApiId);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyError(
                    $"Error processing customer register for {customerApiId} at external system",
                    ex,
                    _logger).ConfigureAwait(false);

                throw new ArgumentException(ex.Message, ex);
            }
        }

        private static IEnumerable<int> GetSecundaryAccounts(IEnumerable<int> sinacorIds, int sinacorMasterAccount)
        {
            return sinacorIds.Where(sinacorId => sinacorId != sinacorMasterAccount);
        }

        private async Task ProcessRegistrationForMasterAccountAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId)
        {
            await ProcessRegisterAsync(
                customer,
                customerApiId,
                sinacorId,
                isSinacorMasterAccount: true).ConfigureAwait(false);
        }

        private async Task ProcessRegistrationForSecundaryAccountsAsync(
            SummaryCustomer customer,
            string customerApiId,
            IEnumerable<int> sinacorIds)
        {
            foreach (var sinacorId in sinacorIds)
            {
                await ProcessRegisterAsync(
                    customer,
                    customerApiId,
                    sinacorId).ConfigureAwait(false);
            }
        }

        private async Task ProcessRegisterAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId,
            bool isSinacorMasterAccount = false)
        {
            bool mustUpdateTheCustomerInTheExternalSystem = false;
            if (customer.IsNew)
            {
                mustUpdateTheCustomerInTheExternalSystem = await RegisterNewCustomerAsync(
                    customer,
                    customerApiId,
                    sinacorId,
                    isSinacorMasterAccount).ConfigureAwait(false);
            }
            else if (_forceRiskAccountUpdate || customer.UpdateRiskProfile)
            {
                mustUpdateTheCustomerInTheExternalSystem = await UpdateRiskRegisterOfCustomerAsync(
                    customer,
                    customerApiId,
                    sinacorId).ConfigureAwait(false);
            }

            if (mustUpdateTheCustomerInTheExternalSystem)
            {
                await UpdateCustomerAtExternalSystemAsync(customerApiId).ConfigureAwait(false);
            }
        }

        private async Task<bool> RegisterNewCustomerAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId,
            bool isSinacorMasterAccount)
        {
            var registeredAtHomebroker = await RegisterAsync(
                NelogicaServiceType.HomeBrokerUserRegister,
                customer,
                customerApiId,
                sinacorId).ConfigureAwait(false);

            if (!registeredAtHomebroker)
            {
                return false;
            }

            if (isSinacorMasterAccount)
            {
                await SaveTradeCustomerAsync(customer.TradePortfolioId, sinacorId, customerApiId);
                await _tradeBalanceUpdater.UpdateTradeBalanceAsync(customerApiId, customer.TradePortfolioId);
            }

            var registeredAtAccountRisk = await RegisterAsync(
                NelogicaServiceType.AcocuntRiskRegister,
                customer,
                customerApiId,
                sinacorId).ConfigureAwait(false);

            if (!registeredAtAccountRisk)
            {
                return false;
            }

            return await RegisterAsync(
                NelogicaServiceType.SuitabilityProfileRegister,
                customer,
                customerApiId,
                sinacorId).ConfigureAwait(false);
        }

        private async Task<bool> UpdateRiskRegisterOfCustomerAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId)
        {
            var updatedAtAccountRisk = await RegisterAsync(
                NelogicaServiceType.AccountRiskUpdater,
                customer,
                customerApiId,
                sinacorId).ConfigureAwait(false);

            if (!updatedAtAccountRisk)
            {
                return false;
            }

            return await RegisterAsync(
                NelogicaServiceType.SuitabilityProfileRegister,
                customer,
                customerApiId,
                sinacorId).ConfigureAwait(false);
        }

        private Task<SummaryCustomer> GetCustomerAsync(string customerApiId)
        {
            return _customerClient.GetCoreCustomer(customerApiId);
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

        private async Task<bool> RegisterAsync(
            NelogicaServiceType nelogicaServiceType,
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId)
        {
            var nelogicaService = GetNelogicaService(nelogicaServiceType);
            var riskResult = await nelogicaService
                .RegisterAsync(customer, customerApiId, sinacorId)
                .ConfigureAwait(false);

            if (riskResult is null)
            {
                return false;
            }

            if (riskResult.IsError)
            {
                await _notificationService.NotifyError(riskResult.ResponseContent, _logger);
                return false;
            }

            _logger.LogInformation(riskResult.ResponseContent);
            return true;
        }

        private async Task UpdateCustomerAtExternalSystemAsync(string customerApiId)
        {
            await _customerClient.UpdateCustomerExternalSystem(customerApiId).ConfigureAwait(false);

            _logger.LogInformation(
                "Customer external system updated for customer {customerApiId} at core",
                customerApiId);
        }

        private INelogicaService GetNelogicaService(NelogicaServiceType nelogicaServiceType)
        {
            return nelogicaServiceType switch
            {
                NelogicaServiceType.AccountRiskUpdater => _accountRiskUpdaterService,
                NelogicaServiceType.AcocuntRiskRegister => _accountRiskRegisterService,
                NelogicaServiceType.SuitabilityProfileRegister => _suitabilityProfileRegisterService,
                NelogicaServiceType.HomeBrokerUserRegister => _homebrokerUserRegisterService,
                _ => throw new NotImplementedException()
            };
        }

        #endregion
    }
}
