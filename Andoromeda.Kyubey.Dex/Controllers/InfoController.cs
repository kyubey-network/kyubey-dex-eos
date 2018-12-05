using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Dex.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class InfoController : BaseController
    {
        [HttpGet("api/v1/lang/{lang}/slides")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetSlidesResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> Banner([FromServices] SlidesRepositoryFactory slidesRepositoryFactory, [FromQuery]GetSlidesRequest request)
        {
            var newSlides = await slidesRepositoryFactory.CreateAsync(request.Lang);
            var responseData = newSlides.EnumerateAll()
                .Select(x => new GetSlidesResponse()
                {
                    Background = $"/slides_assets/{x.Background}".Replace(@"\","/"),
                    Foreground = $"/slides_assets/{x.Foreground}".Replace(@"\", "/")
                });
            return ApiResult(responseData);
        }
    }
}
