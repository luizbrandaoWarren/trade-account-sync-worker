using Warren.Trade.Risk.Infra.Enums;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.Infra.Interfaces
{
    public interface IRiskGroupParser
    {
        string ParseToGroupRisk(SummaryCustomer customer);
        SuitabilityProfileType ParseToSuitabilityProfile(SummaryCustomer customer);
    }
}
