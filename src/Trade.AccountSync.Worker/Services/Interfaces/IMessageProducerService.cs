namespace Warren.Trade.Risk.ClientV2.Services.Interfaces
{
    public interface IMessageProducerService
    {
        Task ProduceBalanceUpdateMessageAsync(int portfolioId, string? customerApiId = null);
    }
}
