namespace Warren.Trade.Risk.ClientV2.Services.Interfaces;
public interface ICacheManagerService
{
    Task PurgeCachesAsync(string customerApiId);
}
