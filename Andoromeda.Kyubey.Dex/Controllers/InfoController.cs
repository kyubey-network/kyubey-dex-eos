using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class InfoController : BaseController
    {
        [HttpGet("api/v1/lang/{lang}/slides")]
        [ProducesResponseType(typeof(ApiResult<List<GetSlidesResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public IActionResult Banner([FromServices] KyubeyContext db, [FromQuery]GetSlidesRequest request)
        {
            var testColumn = request.TestColumn;
            var tokens = db.Tokens.ToList();
            var responseData = new List<GetSlidesResponse> {
                      new GetSlidesResponse(){
                           Background="1.png",
                           Foreground="2.png",
                           Link="/token/exchange",
                           Priority=99
                      },
                      new GetSlidesResponse(){
                           Background="1.png",
                           Foreground="2.png",
                           Link="/token/exchange",
                           Priority=98
                      }
                 };

            return ApiResult(responseData);
        }

        [HttpGet("api/v1/lang/{lang}/volume")]
        [ProducesResponseType(typeof(ApiResult<double>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> Volume([FromServices] KyubeyContext db)
        {
            var volumeVal = await db.MatchReceipts.Where(x => x.Time > DateTime.Now.AddDays(-1)).SumAsync(x => x.Bid);
            return ApiResult(volumeVal);
        }
    }
}
