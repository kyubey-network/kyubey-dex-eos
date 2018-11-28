using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Dex.Models;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class BaseController : Controller
    {
        [NonAction]
        protected IActionResult ApiResult<T>(T ret, int code = 200)
        {
            Response.StatusCode = code;
            return Json(new ApiResult<T>
            {
                code = code,
                data = ret
            });
        }

        [NonAction]
        protected IActionResult ApiResult(int code, string msg)
        {
            Response.StatusCode = code;
            return Json(new ApiResult
            {
                code = code,
                msg = msg
            });
        }
    }
}
