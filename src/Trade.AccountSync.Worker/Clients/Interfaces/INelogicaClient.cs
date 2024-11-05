using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Clients.Interfaces
{
    public interface INelogicaClient
    {
        Task<ParsedResponseMessage> MakeRequest(int customerId, object data);
    }
}
