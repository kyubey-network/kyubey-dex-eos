using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class CurrentOrders
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public double Price { get; set; }

        public double Amount { get; set; }

        public double Total { get; set; }

        public string Token { get; set; }

        public DateTime Time { get; set; }
    }
}
