using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetContentBaseRequest : GetBaseRequest
    {
        [FromRoute]
        public string Id { get; set; }
    }
}
