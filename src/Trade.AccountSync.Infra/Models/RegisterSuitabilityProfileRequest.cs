using Newtonsoft.Json;

namespace Warren.Trade.Risk.Infra.Models
{
    public class RegisterSuitabilityProfileRequest
    {
        private const string REQUEST_FIELD = "update_suitability";

        public RegisterSuitabilityProfileRequest(
            string contaId,
            int perfil,
            string authenticationCode)
        {
            Request = REQUEST_FIELD;
            ContaId = contaId;
            Perfil = perfil;
            AuthenticationCode = authenticationCode;
        }
        [JsonProperty("request")]
        public string Request { get; private set; }
        [JsonProperty("contaId")]
        public string ContaId { get; private set; }
        [JsonProperty("perfil")]
        public int Perfil { get; private set; }
        [JsonProperty("authenticationCode")]
        public string AuthenticationCode { get; private set; }
    }
}
