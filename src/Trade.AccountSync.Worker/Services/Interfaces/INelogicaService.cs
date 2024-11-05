using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public interface INelogicaService
    {
        Task<ParsedResponseMessage?> RegisterAsync(
            SummaryCustomer customer,
            string customerApiId,
            int sinacorId);
    }
}
