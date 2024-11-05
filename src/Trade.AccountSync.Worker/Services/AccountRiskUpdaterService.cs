using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Extensions;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class AccountRiskUpdaterService : IAccountRiskUpdaterService
    {
        private readonly string _nelogicaToken;

        private readonly INelogicaClient _nelogicaClient;
        private readonly IRiskGroupParser _riskGroupParser;
        private readonly ICustomerClient _customerClient;

        private const string NelogicaTokenPathKey = "nelogica:AccessToken";

        public AccountRiskUpdaterService(
            IAppConfig appConfig,
            INelogicaClient nelogicaClient,
            IRiskGroupParser riskGroupParser,
            ICustomerClient customerClient)
        {
            _nelogicaToken = appConfig.GetConfig(NelogicaTokenPathKey);

            _nelogicaClient = nelogicaClient;
            _riskGroupParser = riskGroupParser;
            _customerClient = customerClient;
        }

        public async Task<ParsedResponseMessage?> RegisterAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId)
        {
            var riskGroup = _riskGroupParser.ParseToGroupRisk(customer);
            var riskResult = await UpdateAsync(sinacorId, riskGroup);

            if (riskResult is null)
            {
                return null;
            }

            if (riskResult.HasAccountNotExistsResponse())
            {
                await _customerClient.DeleteCustomerExternalSystem(customerApiId);
                return null;
            }

            return riskResult.BuildResponseContent(this, customerApiId, customer);
        }

        private async Task<ParsedResponseMessage> UpdateAsync(
            int customerId,
            string riskGroup)
        {
            var data = new
            {
                request = "risco_conta",
                contaID = customerId,
                grupo = riskGroup,
                habilitado = true,
                percentualCustodia = 0,
                percentualCustodiaBMF = 0,
                autenticationCode = _nelogicaToken
            };

            return await _nelogicaClient.MakeRequest(customerId, data);
        }
    }
}
