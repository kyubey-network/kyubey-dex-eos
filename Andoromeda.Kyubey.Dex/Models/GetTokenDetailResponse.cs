using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetTokenDetailResponse
    {
        public string Symbol { get; set; }

        public double CurrentPrice { get; set; }

        public double ChangeRecentDay { get; set; }

        public double MaxPriceRecentDay { get; set; }

        public double MinPriceRecentDay { get; set; }

        public double VolumeRecentDay { get; set; }

        public bool IsRecommend { get; set; }

        public string IconSrc { get; set; }

        public int Priority { get; set; }

        public string Description { get; set; }

        public double TotalSupply { get; set; }

        public double TotalCirculate { get; set; }

        public string Contract { get; set; }

        public string Website { get; set; }


    }
}
