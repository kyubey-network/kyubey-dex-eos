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
    public class TokenController : BaseController
    {
        [HttpGet("api/v1/lang/{lang}/token")]
        public async Task<IActionResult> TokenList([FromServices] KyubeyContext db)
        {
            var todayList = await db.MatchReceipts.Where(x => x.Time >= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice,
                MaxPrice = x.Max(c => c.UnitPrice),
                MinPrice = x.Min(c => c.UnitPrice),
                Volume = x.Sum(c => c.Bid)
            }).ToListAsync();
            var yesterdayList = await db.MatchReceipts.Where(x => x.Time <= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice
            }).ToListAsync();

            var responseData = db.Tokens.OrderByDescending(x=>x.Priority).ToList().Select(x => new GetTokenListResponse()
            {
                icon_src = $"/token_assets/{x.Id}/icon.png",
                current_price = todayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice ?? yesterdayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice ?? 0,
                change_recent_day =
                 todayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice == null ||
                 yesterdayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice == null ||
                 yesterdayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice == 0 ?
                 0 : todayList.FirstOrDefault(t => t.TokenId == x.Id).CurrentPrice / yesterdayList.FirstOrDefault(t => t.TokenId == x.Id).CurrentPrice,
                is_recommend = true,
                max_price_recent_day = todayList.FirstOrDefault(s => s.TokenId == x.Id)?.MaxPrice ?? 0,
                min_price_recent_day = todayList.FirstOrDefault(s => s.TokenId == x.Id)?.MinPrice ?? 0,
                symbol = x.Id,
                volume_recent_day = todayList.FirstOrDefault(s => s.TokenId == x.Id)?.Volume ?? 0,
                priority = x.Priority
            });

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
