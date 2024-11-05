using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class HomebrokerUserRegisterService : IHomebrokerUserRegisterService
    {
        private readonly string _nelogicaToken;
        private readonly INelogicaClient _nelogicaClient;

        private const string NelogicaTokenPathKey = "nelogica:AccessToken";
        private const string AlreadyRegisteredUserMessage = "Usuário já cadastrado";

        public HomebrokerUserRegisterService(
            IAppConfig appConfig,
            INelogicaClient nelogicaClient)
        {
            _nelogicaToken = appConfig.GetConfig(NelogicaTokenPathKey);
            _nelogicaClient = nelogicaClient;
        }

        public async Task<ParsedResponseMessage?> RegisterAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId)
        {
            var responseMessage = await RegisterAsync(customer, sinacorId);
            if (responseMessage is null)
            {
                return null;
            }

            if (responseMessage.IsError)
            {
                if (responseMessage.ResponseContent.Contains(AlreadyRegisteredUserMessage))
                {
                    responseMessage.IsError = false;
                }
                else
                {
                    var responseContent = responseMessage.ResponseContent;
                    responseMessage.ResponseContent =
                        $"Error creating homebroker account for {customerApiId}: {responseContent}";
                }
            }
            else
            {
                responseMessage.ResponseContent =
                    $"Homebroker user {customer.Id} registered at external system for customer {customerApiId}";
            }

            return responseMessage;
        }

        private Task<ParsedResponseMessage> RegisterAsync(SummaryCustomer customer, int sinacorId)
        {
            var data = new
            {
                request = "insert_usuario_homebroker",
                contaID = sinacorId,
                documento = customer.Document,
                pessoaFisica = customer.IsLegalPerson ? 0 : 1,
                tipoDocumento = customer.IsLegalPerson ? "cnpj" : "cpf",
                nome = customer.Name,
                sexo = customer.Gender,
                dataNascimento = customer.BirthDate,
                cep = customer.ZipCode,
                estado = customer.State,
                cidade = customer.City,
                bairro = customer.Neighborhood,
                endereco = customer.Street,
                numero = customer.Number,
                complemento = customer.Complement,
                pais = "BRA",
                email = GenerateDummyEmail(sinacorId),
                telefone = customer.Phone,
                ddd = "",
                autenticationCode = _nelogicaToken
            };

            return _nelogicaClient.MakeRequest(sinacorId, data);
        }

        private static string GenerateDummyEmail(int id) => $"{id}@warren.com";
    }
}
