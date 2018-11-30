using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetBaseRequest
    {
        [FromRoute]
        public string Lang { get; set; }
    }
}
