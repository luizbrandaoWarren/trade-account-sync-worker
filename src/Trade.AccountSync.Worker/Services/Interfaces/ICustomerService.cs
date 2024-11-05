namespace Warren.Trade.Risk.ClientV2.Services.Interfaces;

public interface ICustomerService : IDisposable
{
    Task UpdateSinacorAccountAsync(string customerApiId, int sinacorId);
}
