using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }



        [HttpGet("api/v1/lang/{lang}/news?skip={skip}&take={take}&type={type}")]
        public IActionResult Get([FromServices] NewsRepositoryFactory newsRepositoryFactory) {

        }
    }
}
