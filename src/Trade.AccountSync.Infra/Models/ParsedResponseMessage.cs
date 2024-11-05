using System.Net.Http;

namespace Warren.Trade.Risk.Infra.Models
{
    public class ParsedResponseMessage
    {
        #region Properties

        public HttpResponseMessage OriginalResponse { get; set; }

        public string ResponseContent { get; set; }
        public bool IsError { get; set; }
        public bool IsTimeoutError { get; set; }

        #endregion Properties

        public override string ToString()
        {
            return ResponseContent;
        }
    }
}
