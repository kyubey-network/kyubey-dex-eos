using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetFavoriteRequest
    {
        public string symbol { get; set; } 

        public double unit_price { get; set; }

        public double change { get; set; }

        public bool favorite { get; set; }
    }
}
