using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warren.Trade.Risk.Infra.Models
{
    public class RlpResponse
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Status { get; set; }
    }
}
