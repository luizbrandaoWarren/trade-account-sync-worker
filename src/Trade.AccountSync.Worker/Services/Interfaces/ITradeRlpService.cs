namespace Warren.Trade.Risk.ClientV2.Services.Interfaces;

public interface ITradeRlpService
{
    Task SendActivationRequest(string sinacorId);
}