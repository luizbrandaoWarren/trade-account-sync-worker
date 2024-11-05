using Warren.Trade.Risk.ClientV2.Services;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Extensions
{
    public static class ParsedResponseMessageExtension
    {
        private const string ACCOUNT_NOT_EXISTS_MESSAGE = "Erro no cadastramento da request risco_conta : Conta Inexistente.";

        public static ParsedResponseMessage BuildResponseContent(this ParsedResponseMessage riskResult, INelogicaService nelogicaService, string customerApiId, SummaryCustomer customer)
        {
            var isRegister = nelogicaService.GetType().Name.Contains("Register");

            if (riskResult.IsError)
                riskResult.ResponseContent = $"Error {(isRegister ? "creating" : "updating")} risk account for {customerApiId}: {riskResult.ResponseContent}";
            else
                riskResult.ResponseContent = $"Risk account {customer.Id} {(isRegister ? "registered" : "updated")} at external system for customer {customerApiId} " +
                    $"with {customer.RiskProfile} risk profile" +
                    $" - {customer.IsQualifiedInvestor}" +
                    $" - {customer.IsProfessionalInvestor}";

            return riskResult;
        }

        public static bool HasAccountNotExistsResponse(this ParsedResponseMessage riskResult) =>
            riskResult.IsError && riskResult.ResponseContent.Contains(ACCOUNT_NOT_EXISTS_MESSAGE);
    }
}
