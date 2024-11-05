using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Clients.Interfaces
{
    public interface ICustomerClient
    {
        Task<SummaryCustomer> GetCoreCustomer(string apiId);
        Task UpdateCustomerExternalSystem(string apiId);

        Task DeleteCustomerExternalSystem(string apiId);
    }
}
