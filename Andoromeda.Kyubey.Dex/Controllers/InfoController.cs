using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class InfoController : Controller
    {
        [HttpGet("api/v1/lang/{lang}/banner")]
        public IActionResult Banner([FromServices] KyubeyContext db, [FromQuery]GetBannerRequest request)
        {
            var textC = request.test_column;
            var tokens = db.Tokens.ToList();
            var responseData = new List<GetBannerResponse> {
                      new GetBannerResponse(){
                           background_src="1.png",
                           foreground_src="2.png",
                           href="/token/exchange",
                           priority=99
                      },
                      new GetBannerResponse(){
                           background_src="1.png",
                           foreground_src="2.png",
                           href="/token/exchange",
                           priority=98
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
