using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Threading.Tasks;
using Warren.Trade.Risk.Infra.Interfaces;

namespace Warren.Trade.Risk.Infra
{
    public class ResilientlyHttpClient : IResilientlyHttpClient
    {
        private readonly ILogger<ResilientlyHttpClient> _logger;

        public ResilientlyHttpClient(ILogger<ResilientlyHttpClient> logger)
        {
            _logger = logger;
        }

        public async Task<IFlurlResponse> PostAsync(string url, object data, object headers = null)
        {
            return await DispatchRequestResiliently(() => url
                .WithHeaders(headers ?? new { })
                .PostJsonAsync(data));
        }

        public async Task<T> GetAsync<T>(string url, object headers = null)
        {
            return await DispatchRequestResiliently(() => url
                .WithHeaders(headers ?? new { })
                .GetJsonAsync<T>());
        }

        public async Task GetAsync(string url, object headers = null)
        {
            await DispatchRequestResiliently(() => url
                .WithHeaders(headers ?? new { })
                .GetJsonAsync());
        }

        public async Task DeleteAsync(string url, object headers = null)
        {
            await DispatchRequestResiliently(() => url
                .WithHeaders(headers ?? new { })
                .DeleteAsync());
        }

        private async Task<T> DispatchRequestResiliently<T>(Func<Task<T>> requestAction)
        {
            var retryPolicy = BuildRetryPolicy();
            return await retryPolicy.ExecuteAsync(() => requestAction());
        }

        private AsyncRetryPolicy BuildRetryPolicy()
        {
            return Policy
              .Handle<FlurlHttpException>(IsWorthRetrying)
              .WaitAndRetryAsync(
                retryCount: 10,
                sleepDurationProvider: (attempt) => TimeSpan.FromSeconds(attempt * 5),
                onRetry: (ex, timestamp, retryCount, context) => LogException((FlurlHttpException)ex));
        }

        private bool IsWorthRetrying(FlurlHttpException ex)
        {
            bool worthRetrying;

            if (ex.Call.Response == null)
            {
                worthRetrying = false;
            }
            else
            {
                switch ((int)ex.Call.Response.StatusCode)
                {
                    case 429:
                    case 408:
                    case 502:
                    case 503:
                    case 504:
                        worthRetrying = true;
                        break;
                    default:
                        worthRetrying = false;
                        break;
                }
            }

            if (!worthRetrying) LogException(ex);

            return worthRetrying;
        }

        private void LogException(FlurlHttpException ex)
        {
            if (ex.Call?.Response is null)
            {
                _logger.LogError(ex.Message);
                return;
            }

            var uri = ex.Call?.HttpResponseMessage?.RequestMessage?.RequestUri?.AbsoluteUri;
            var statusCode = ex.Call?.HttpResponseMessage?.StatusCode;
            var message = ex.Message;
            var method = ex.Call?.HttpRequestMessage?.Method?.Method;

            var errorMessage = $"An error occurred while processing the request: " +
                $"Path: {uri}" +
                $"StatusCode: {statusCode}" +
                $"Message: {message}" +
                $"Method: {method}";

            _logger.LogError(errorMessage);
        }
    }
}
