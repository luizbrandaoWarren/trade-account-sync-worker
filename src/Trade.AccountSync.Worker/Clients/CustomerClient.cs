using Microsoft.Extensions.Logging;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Clients
{
    public class CustomerClient : ICustomerClient
    {
        #region Properties

        private readonly INotificationService _notificationService;
        private readonly IResilientlyHttpClient _httpClient;
        private readonly ILogger<CustomerClient> _logger;

        private const string INVESTMENTS_API_URL_PATH_KEY = "investmentsAPI:BaseUrlRoute";
        private const string EXTERNAL_SYSTEM_TYPE = "90";

        private readonly string _investmentsApiUrl;

        #endregion

        #region Constructor

        public CustomerClient(
            INotificationService notificationService,
            IResilientlyHttpClient httpClient,
            IAppConfig appConfig,
            ILogger<CustomerClient> logger)
        {
            _notificationService = notificationService;
            _httpClient = httpClient;
            _investmentsApiUrl = appConfig.GetConfig(INVESTMENTS_API_URL_PATH_KEY);
            _logger = logger;
        }

        #endregion

        #region Methods

        public async Task<SummaryCustomer> GetCoreCustomer(string apiId)
        {
            var path = $"{_investmentsApiUrl}{apiId}/Summary/{EXTERNAL_SYSTEM_TYPE}";

            try
            {
                return await _httpClient.GetAsync<SummaryCustomer>(path);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyError($"Error searching customer {apiId} on core", ex, _logger);
                return null;
            }
        }

        public async Task UpdateCustomerExternalSystem(string apiId)
        {
            var path = $"{_investmentsApiUrl}{apiId}/Register/{EXTERNAL_SYSTEM_TYPE}";

            try
            {
                await _httpClient.GetAsync(path);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyError($"Error updating customer {apiId} at external system", ex, _logger);               
            }
        }

        public async Task DeleteCustomerExternalSystem(string apiId)
        {
            var path = $"{_investmentsApiUrl}{apiId}/Delete/{EXTERNAL_SYSTEM_TYPE}";

            try
            {
                await _httpClient.DeleteAsync(path);
            }
            catch (Exception ex)
            {
                await _notificationService.NotifyError($"Error deleting customer {apiId} at external system", ex, _logger);
            }
        }
        #endregion
    }
}
