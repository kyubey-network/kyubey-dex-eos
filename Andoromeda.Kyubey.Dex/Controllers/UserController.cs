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
        public async Task<IActionResult> GetFavorite([FromServices] KyubeyContext db, string account)
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

            var lastUnitPrice = last.Select(x => new
            {
                id = x.TokenId,
                price = x.Last?.UnitPrice ?? 0,
                change = (x.Last?.UnitPrice) / (last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice - 1)
            });

            var tokendUnitPriceResult = db.Tokens.Where(x => x.Status == TokenStatus.Active).ToList().Select(x => new
            {
                id = x.Id,
                price = lastUnitPrice.FirstOrDefault(o => o.id == x.Id)?.price ?? 0,
                change = lastUnitPrice.FirstOrDefault(o => o.id == x.Id)?.change ?? 0
            }).ToList();

            var favorite = await db.Favorites
                .Where(x=>x.Account == account)
                .ToListAsync();
      
            var responseData = new List<GetFavoriteRequest> {};
            
            responseData.AddRange(tokendUnitPriceResult.Select(
               x => new GetFavoriteRequest
               {
                   Symbol = x.id,
                   Unit_price = x.price,
                   Change = x.change,
                   Favorite = favorite.Exists(y => y.TokenId == x.id)
                }
            ));
           
            var response = new ApiResult{
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
            var ret = new List<GetCurrentOrdersResponse>(buy.Count + sell.Count);
            
            ret.AddRange(buy.Select(x => new GetCurrentOrdersResponse
            {
                Id = x.Id,
                Token = x.TokenId,
                Type = "Buy",
                Amount = x.Ask,
                Price = x.UnitPrice,
                Total = x.Bid,
                Time = x.Time
            }));
            ret.AddRange(sell.Select(x => new GetCurrentOrdersResponse
            {
                Id = x.Id,
                Token = x.TokenId,
                Type = "Sell",
                Amount = x.Bid,
                Price = x.UnitPrice,
                Total = x.Ask,
                Time = x.Time
            }));
            var response = new ApiResult()
            {
                code = 200,
                data = ret.OrderByDescending(x => x.Time),
                msg = "Succeeded"
            };
            return Json(response);
      
        }

        [HttpGet("/api/v1/lang/{lang-id}/user/{account}/history-delegate")]
        public async Task<IActionResult>  GethistoryDelegate ([FromServices] KyubeyContext db, string account)
        {
            var matches = await db.MatchReceipts
                .Where(x => x.Bidder == account || x.Asker == account).ToListAsync();
            var userHistoryList = new List<HistoryOrders> { };
            userHistoryList.AddRange(
                matches.Select(
                    x => new HistoryOrders
                    {
                       Id = x.Id,
                       Symbol = x.TokenId,
                       Bidder = x.Bidder,
                       Asker = x.Asker,
                       Type = x.Bidder == account ?  "sell" : "buy",
                       Unit_price = x.UnitPrice,
                       Amount = x.Ask,
                       Total = x.Bid,
                       Time = x.Time
                    }
                ));
            var response = new ApiResult()
            {
                code = 200,
                data = userHistoryList.OrderByDescending(x => x.Time),
                msg = "Succeeded"
            };
            return Json(response);
        }
        
        
        [HttpGet("/api/v1/lang/{lang-id}/user/{account}/wallet")]
        public IActionResult GetWallet([FromServices] KyubeyContext db, string account)
        {
            throw new NotImplementedException("");
        }
       

    }
}
