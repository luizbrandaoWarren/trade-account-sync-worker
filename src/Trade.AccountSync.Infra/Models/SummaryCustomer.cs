using Newtonsoft.Json;
using System.Collections.Generic;

namespace Warren.Trade.Risk.Infra.Models
{
    public class SummaryCustomer
    {
        public int Id { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public string BirthDate { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Neighborhood { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public string Phone { get; set; }
        [JsonProperty("ddd")]
        public string DDD { get; set; }
        public bool IsNew { get; set; }
        public bool IsLegalPerson { get; set; }
        public string RiskProfile { get; set; }
        public bool UpdateRiskProfile { get; set; }
        public bool IsProfessionalInvestor { get; set; }
        public bool IsQualifiedInvestor { get; set; }
        public int? TradePortfolioId { get; set; }
        public IEnumerable<int> SinacorIds { get; set; }
    }
}
