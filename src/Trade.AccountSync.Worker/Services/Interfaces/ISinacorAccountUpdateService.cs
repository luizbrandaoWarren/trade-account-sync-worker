namespace Warren.Trade.Risk.ClientV2.Services.Interfaces;
public interface ISinacorAccountUpdateService : IDisposable
{
    Task UpdateAsync(string customerApiId, int sinacorId);
}
