using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetPagingRequest : GetBaseRequest
    {
        public int Skip { get; set; }

        public int Take { get; set; } = 10;
    }
}
