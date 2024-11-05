using Flurl.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.Infra.Parsers
{
    /// <summary>
    /// Parse clients responses.
    /// </summary>
    public static class ResponseParser
    {
        /// <summary>
        /// Parse HTTP response message.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ParsedResponseMessage ParseResponse(this HttpResponseMessage response)
        {
            var parsedResponse = new ParsedResponseMessage()
            {
                OriginalResponse = response
            };
            
            var responseContent = response.Content.ReadAsStringAsync()?.Result;

            var objResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
            var status = objResponse["status"];
            var message = objResponse["msg"];


            if (!response.IsSuccessStatusCode || status != "1")
            {
                parsedResponse.IsError = true;
                parsedResponse.ResponseContent = message;
            }
            else parsedResponse.ResponseContent = message;

            return parsedResponse;
        }

        public static ParsedResponseMessage ParseResponse(this IFlurlResponse response) =>
            response.ResponseMessage.ParseResponse();
    }
}
