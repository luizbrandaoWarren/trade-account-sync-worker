using Warren.Core.Messaging.Consumers;
using Warren.Core.Messaging.Risk.Contracts.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public interface IRequestService
    {
        Task ProcessCustomerRegister(MessageConsumerResult<CreateCustomerRequestMessage> result);
    }
}
