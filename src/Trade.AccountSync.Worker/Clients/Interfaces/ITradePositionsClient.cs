namespace Warren.Trade.Risk.ClientV2.Clients.Interfaces;
public interface ITradePositionsClient
{
    Task UpdateCustodyAsync(int sinacorId);
}
