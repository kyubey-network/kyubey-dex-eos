using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetSlidesResponse
    {
        public string foreground_src { get; set; }

        public string background_src { get; set; }

        public string href { get; set; }

        public int priority { get; set; }
    }
}
