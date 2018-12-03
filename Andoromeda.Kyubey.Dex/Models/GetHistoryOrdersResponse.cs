using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetHistoryOrdersResponse
    {
        public string Id     { get; set; }

        public string Symbol { get; set; }

        public string Bidder { get; set; }

        public string Asker  { get; set; }

        public string Type   { get; set; }

        public double UnitPrice { get; set; }

        public double Amount { get; set;}

        public double Total  { get; set; }

        public DateTime Time   { get; set; }
    }
}
