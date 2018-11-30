using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class InfoController : BaseController
    {
        [HttpGet("api/v1/lang/{lang}/slides")]
        public IActionResult Banner([FromServices] KyubeyContext db, [FromQuery]GetSlidesRequest request)
        {
            var textC = request.test_column;
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

            var response = new ApiResult()
            {
                code = 200,
                data = responseData,
                msg = "Succeeded"

            };
            return Json(response);
        }
    }
}
