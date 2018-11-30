using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class CurrentOrders
    {
        public long id { get; set; }

        public string type { get; set; }

        public double price { get; set; }

        public double amount { get; set; }

        public double total { get; set; }

        public string token { get; set; }

        public DateTime time { get; set; }
    }
}
