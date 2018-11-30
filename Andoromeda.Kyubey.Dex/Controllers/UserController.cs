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
    public class UserController : Controller
    {
        [HttpGet("/api/v1/lang/{lang}/user/{account}/favorite")]
        public async Task<IActionResult> GetFavorite([FromServices] KyubeyContext db,string account)
        {
            var last = await db.MatchReceipts
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.LastOrDefault()
                })
                .ToListAsync();

            var last24 = await db.MatchReceipts
                .Where(y => y.Time < DateTime.UtcNow.AddDays(-1))
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.LastOrDefault()
                })
                .ToListAsync();

            var rOrdinal = last.Select(x => new
            {
                id = x.TokenId,
                price = x.Last?.UnitPrice ?? 0,
                change = (x.Last?.UnitPrice ?? 0) / (last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice - 1)
            });

            var r = db.Tokens.Where(x => x.Status == TokenStatus.Active).ToList().Select(x => new
            {
                id = x.Id,
                price = rOrdinal.FirstOrDefault(o => o.id == x.Id)?.price ?? 0,
                change = rOrdinal.FirstOrDefault(o => o.id == x.Id)?.change ?? 0
            }).ToList();

            var favorite = await db.Favorites
                .Where(x=>x.Account == account)
                .ToListAsync();
      
            var responseData = new List<GetFavoriteRequest> {};
            
            responseData.AddRange(r.Select(
               x => new GetFavoriteRequest
               {
                   symbol = x.id,
                   unit_price = x.price,
                   change = x.change,
                   favorite = favorite.Exists(y => y.TokenId == x.id)
                }
            ));
           
            var response = new ApiResult() {
                   code = 200,
                   data = responseData,
                   msg = "Succeeded"
            };
            return Json(response);
        }
        
        [HttpGet("/api/v1/lang/{lang-id}/user/{account}/current-delegate")]
        public async Task<IActionResult> GetCurrentDelegate ([FromServices] KyubeyContext db, string account)
        {
            var buy = await db.DexBuyOrders.Where(x => x.Account == account).ToListAsync();
            var sell = await db.DexSellOrders.Where(x => x.Account == account).ToListAsync();
            var ret = new List<CurrentOrders>(buy.Count + sell.Count);
            
            ret.AddRange(buy.Select(x => new CurrentOrders
            {
                id = x.Id,
                token = x.TokenId,
                type = "Buy",
                amount = x.Ask,
                price = x.UnitPrice,
                total = x.Bid,
                time = x.Time
            }));
            ret.AddRange(sell.Select(x => new CurrentOrders
            {
                id = x.Id,
                token = x.TokenId,
                type = "Sell",
                amount = x.Bid,
                price = x.UnitPrice,
                total = x.Ask,
                time = x.Time
            }));
            var response = new ApiResult()
            {
                code = 200,
                data = ret.OrderByDescending(x=>x.time),
                msg = "Succeeded"
            };
            return Json(response);
      
        }

        [HttpGet("/api/v1/lang/{lang-id}/user/{account}/history-delegate")]
        public IActionResult GethistoryDelegate ([FromServices] KyubeyContext db, string account)
        {
            IQueryable<MatchReceipt> matches = db.MatchReceipts
                .Where(x => x.Bidder == account || x.Asker == account);
            var userHistoryList = new List<HistoryOrders> { };
            userHistoryList.AddRange(
                matches.Select(
                    x => new HistoryOrders
                    {
                       id = x.Id,
                       symbol = x.TokenId,
                       bidder = x.Bidder,
                       asker = x.Asker,
                       type = x.Bidder == account ?  "sell" : "buy",
                       unit_price = x.UnitPrice,
                       amount = x.Ask,
                       total = x.Bid,
                       time = x.Time
                    }
                ));
            var response = new ApiResult()
            {
                code = 200,
                data = userHistoryList.OrderByDescending(x => x.time),
                msg = "Succeeded"
            };
            return Json(response);
        }
        
        /*
        [HttpGet("/api/v1/lang/{lang-id}/user/{account}/wallet")]
        public async Task<IActionResult> GetWallet([FromServices] KyubeyContext db, string account)
        {
        }
        */

    }
}
