using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Extensions;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class AccountRiskRegisterService : IAccountRiskRegisterService
    {
        private readonly string _nelogicaToken;

        private readonly INelogicaClient _nelogicaClient;
        private readonly IRiskGroupParser _riskGroupParser;
        private readonly ICustomerClient _customerClient;

        private const string NelogicaTokenPathKey = "nelogica:AccessToken";

        public AccountRiskRegisterService(
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
            var riskResult = await Register(sinacorId, riskGroup);

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

        private async Task<ParsedResponseMessage> Register(
            int customerId,
            string riskGroup)
        {
            var data = new
            {
                request = "risco_conta",
                contaID = customerId,
                grupo = riskGroup,
                habilitado = true,
                habilitadoOperarNegativo = false,
                autenticationCode = _nelogicaToken
            };

            return await _nelogicaClient.MakeRequest(customerId, data);
        }
    }
}
