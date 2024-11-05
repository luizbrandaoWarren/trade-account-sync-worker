using Flurl.Http;
using System.Threading.Tasks;

namespace Warren.Trade.Risk.Infra.Interfaces
{
    public interface IResilientlyHttpClient
    {
        Task<IFlurlResponse> PostAsync(string url, object data, object headers = null);
        Task<T> GetAsync<T>(string url, object headers = null);
        Task GetAsync(string url, object headers = null);
        Task DeleteAsync(string url, object headers = null);
    }
}
