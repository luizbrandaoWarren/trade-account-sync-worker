using System.Threading.Tasks;

namespace Warren.Trade.Risk.ClientV2.Clients.Interfaces
{
    public interface IRlpClient
    {
        Task SendActivationRequest(string sinacorId);
        Task<bool> CheckIfExistsRlp(string sinacorId);
    }
}
