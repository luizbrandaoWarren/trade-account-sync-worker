using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Messages.Services;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra.Enums;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class SuitabilityProfileRegisterService : NelogicaService, ISuitabilityProfileRegisterService
    {
        private readonly INelogicaClient _nelogicaClient;
        private readonly IRiskGroupParser _riskGroupParser;

        public SuitabilityProfileRegisterService(
            INelogicaClient nelogicaClient,
            IRiskGroupParser riskGroupParser,
            IAppConfig appConfig)
        {
            ArgumentNullException.ThrowIfNull(nelogicaClient, nameof(nelogicaClient));
            ArgumentNullException.ThrowIfNull(riskGroupParser, nameof(riskGroupParser));
            ArgumentNullException.ThrowIfNull(appConfig, nameof(appConfig));

            _nelogicaClient = nelogicaClient;
            _riskGroupParser = riskGroupParser;

            NelogicaToken = appConfig.GetConfig(NelogicaTokenPathKey);
        }

        public async Task<ParsedResponseMessage?> RegisterAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId)
        {
            var perfil = _riskGroupParser.ParseToSuitabilityProfile(customer);
            var result = await RegisterAsync(sinacorId, (int)perfil)
                .ConfigureAwait(false);

            if (result is null) return null!;

            UpdateResponseContent(
                result,
                sinacorId,
                customerApiId,
                perfil);

            return result;
        }

        private async Task<ParsedResponseMessage?> RegisterAsync(
            int contaId,
            int perfil)
        {
            RegisterSuitabilityProfileRequest data = new RegisterSuitabilityProfileRequest(
                contaId: contaId.ToString(),
                perfil: perfil,
                authenticationCode: NelogicaToken);

            return await _nelogicaClient.MakeRequest(contaId, data)
                .ConfigureAwait(false);
        }

        private void UpdateResponseContent(
            ParsedResponseMessage result,
            int customerId,
            string customerApiId,
            SuitabilityProfileType perfil)
        {
            if (result.IsError)
            {
                result.ResponseContent = string.Format(SuitabilityProfileMessages.ERROR_MESSAGE,
                                                       customerApiId,
                                                       result.ResponseContent);
                return;
            }

            string suitabilityProfile = Enum.GetName(typeof(SuitabilityProfileType), perfil)!;
            result.ResponseContent = string.Format(SuitabilityProfileMessages.SUCESS_MESSAGE,
                                                   customerId,
                                                   customerApiId,
                                                   suitabilityProfile);
        }
    }
}
