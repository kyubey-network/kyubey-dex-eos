using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class HistoryOrders
    {
        public string id     { get; set; }
        public string symbol { get; set; }
        public string bidder { get; set; }
        public string asker  { get; set; }
        public string type   { get; set; }
        public double unit_price { get; set; }
        public double amount { get; set; }
        public double total  { get; set; }
        public DateTime time   { get; set; }
    }
}
