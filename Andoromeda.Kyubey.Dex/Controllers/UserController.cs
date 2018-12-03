using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet("/api/v1/lang/{lang}/user/{account}/favorite")]
        [ProducesResponseType(typeof(ApiResult<List<GetFavoriteResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetFavorite([FromServices] KyubeyContext db, string account, CancellationToken cancellationToken)
        {
            var last = await db.MatchReceipts
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.LastOrDefault()
                })
                .ToListAsync(cancellationToken);

            var last24 = await db.MatchReceipts
                .Where(y => y.Time < DateTime.UtcNow.AddDays(-1))
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.LastOrDefault()
                })
                .ToListAsync(cancellationToken);

            var lastUnitPrice = last.Select(x => new
            {
                id = x.TokenId,
                price = x.Last?.UnitPrice,
                change = (x.Last?.UnitPrice) / (last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice - 1)
            });

            var tokendUnitPriceResult = (await db.Tokens.Where(x => x.Status == TokenStatus.Active).ToListAsync(cancellationToken)).Select(x => new
            {
                id = x.Id,
                price = lastUnitPrice.FirstOrDefault(o => o.id == x.Id)?.price ?? 0,
                change = lastUnitPrice.FirstOrDefault(o => o.id == x.Id)?.change ?? 0
            }).ToList();

            var favorite = await db.Favorites
                .Where(x => x.Account == account)
                .ToListAsync(cancellationToken);

            var responseData = new List<GetFavoriteResponse> { };

            responseData.AddRange(tokendUnitPriceResult.Select(
               x => new GetFavoriteResponse
               {
                   Symbol = x.id,
                   UnitPrice = x.price,
                   Change = x.change,
                   Favorite = favorite.Exists(y => y.TokenId == x.id)
               }
            ));

            return ApiResult(responseData);
        }

        [HttpGet("/api/v1/lang/{lang}/user/{account}/current-delegate")]
        [ProducesResponseType(typeof(ApiResult<List<GetCurrentOrdersResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetCurrentDelegate([FromServices] KyubeyContext db, string account, CancellationToken cancellationToken)
        {
            var buy = await db.DexBuyOrders.Where(x => x.Account == account).ToListAsync(cancellationToken);
            var sell = await db.DexSellOrders.Where(x => x.Account == account).ToListAsync(cancellationToken);
            var ret = new List<GetCurrentOrdersResponse>();

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

            return ApiResult(ret.OrderByDescending(x => x.Time));
        }

        [HttpGet("/api/v1/lang/{lang}/user/{account}/history-delegate")]
        [ProducesResponseType(typeof(ApiResult<List<GetHistoryOrdersResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetHistoryDelegate([FromServices] KyubeyContext db, string account, CancellationToken cancellationToken)
        {
            var matches = await db.MatchReceipts
                .Where(x => x.Bidder == account || x.Asker == account).ToListAsync(cancellationToken);

            var userHistoryList = matches.Select(x => new GetHistoryOrdersResponse
            {
                Id = x.Id,
                Symbol = x.TokenId,
                Bidder = x.Bidder,
                Asker = x.Asker,
                Type = x.Bidder == account ? "sell" : "buy",
                UnitPrice = x.UnitPrice,
                Amount = x.Ask,
                Total = x.Bid,
                Time = x.Time
            });

            return ApiResult(userHistoryList.OrderByDescending(x => x.Time));
        }


        [HttpGet("/api/v1/lang/{lang}/user/{account}/wallet")]
        public IActionResult GetWallet([FromServices] KyubeyContext db, string account)
        {
            throw new NotImplementedException("");
        }
    }
}
