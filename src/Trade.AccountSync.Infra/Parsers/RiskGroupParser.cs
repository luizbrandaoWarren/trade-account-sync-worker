using System;
using Warren.Trade.Risk.Infra.Enums;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.Infra
{
    public class RiskGroupParser : IRiskGroupParser
    {
        private const string AGRESSIVO = "agressivo";
        private const string MODERADO = "moderado";
        private const string CONSERVADOR = "conservador";

        public RiskGroupParser()
        { }

        public string ParseToGroupRisk(SummaryCustomer customer) =>
            this.GetExternalRiskGroup(
                customer.RiskProfile,
                customer.IsProfessionalInvestor,
                customer.IsQualifiedInvestor);

        public SuitabilityProfileType ParseToSuitabilityProfile(SummaryCustomer customer)
        {
            var riskGroup = customer.RiskProfile;

            if (riskGroup.EndsWith(AGRESSIVO, StringComparison.InvariantCultureIgnoreCase)) return SuitabilityProfileType.Agressivo;
            if (riskGroup.EndsWith(MODERADO, StringComparison.InvariantCultureIgnoreCase)) return SuitabilityProfileType.Moderado;
            if (riskGroup.EndsWith(CONSERVADOR, StringComparison.InvariantCultureIgnoreCase)) return SuitabilityProfileType.Conservador;
            return SuitabilityProfileType.Conservador;
        }

        private string GetExternalRiskGroup(
            string coreRiskProfile,
            bool IsProfessionalInvestor,
            bool IsQualifiedInvestor)
        {
            switch (coreRiskProfile.ToLowerInvariant())
            {
                case "agressivo" when IsProfessionalInvestor:
                case "moderado agressivo" when IsProfessionalInvestor:
                    return "Agressivo - Profissional";
                case "agressivo" when IsQualifiedInvestor:
                case "moderado agressivo" when IsQualifiedInvestor:
                    return "Agressivo - Qualificado";
                case "agressivo":
                case "moderado agressivo":
                    return "Agressivo";

                case "moderado" when IsProfessionalInvestor:
                case "conservador moderado" when IsProfessionalInvestor:
                    return "Moderado - Profissional";
                case "moderado" when IsQualifiedInvestor:
                case "conservador moderado" when IsQualifiedInvestor:
                    return "Moderado - Qualificado";
                case "moderado":
                case "conservador moderado":
                    return "Moderado";

                default:
                    return "Conservador";
            }
        }
    }
}
