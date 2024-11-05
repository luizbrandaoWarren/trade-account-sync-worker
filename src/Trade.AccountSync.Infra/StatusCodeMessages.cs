using System.Net;

namespace Warren.Trade.Risk.Infra
{
    public static class StatusCodeMessages
    {
        public const HttpStatusCode unexpected_error = HttpStatusCode.InternalServerError;
        public const HttpStatusCode error_internal_server_error = HttpStatusCode.InternalServerError;
    }
}
