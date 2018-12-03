using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class TokenController : BaseController
    {
        [HttpGet("api/v1/lang/{lang}/token")]
        [ProducesResponseType(typeof(ApiResult<GetTokenListResponse>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> TokenList([FromServices] KyubeyContext db, CancellationToken cancellationToken)
        {
            var todayList = await db.MatchReceipts.Where(x => x.Time >= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice,
                MaxPrice = x.Max(c => c.UnitPrice),
                MinPrice = x.Min(c => c.UnitPrice),
                Volume = x.Sum(c => c.Bid)
            }).ToListAsync(cancellationToken);

            var yesterdayList = await db.MatchReceipts.Where(x => x.Time <= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice
            }).ToListAsync(cancellationToken);

            var responseData = (await db.Tokens.OrderByDescending(x => x.Priority).ToListAsync(cancellationToken)).Select(x => new GetTokenListResponse()
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

            return ApiResult(responseData);
        }


        [HttpGet("api/v1/lang/{lang}/token/{tokenId}/buy-order")]
        [ProducesResponseType(typeof(ApiResult<GetBaseOrderResponse>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> BuyOrder([FromServices] KyubeyContext db, string tokenId, CancellationToken cancellationToken)
        {
            var orders = await db.DexBuyOrders
                        .Where(x => x.TokenId == tokenId)
                        .OrderByDescending(x => x.UnitPrice)
                        .Take(15)
                        .ToListAsync(cancellationToken);

            var responseData = orders
                .Select(x => new GetBaseOrderResponse
                {
                    UnitPrice = x.UnitPrice,
                    Amount = x.Bid,
                    Total = x.Ask
                });

            return ApiResult(responseData);
        }

        [HttpGet("api/v1/lang/{lang}/token/{tokenId}/sell-order")]
        [ProducesResponseType(typeof(ApiResult<GetBaseOrderResponse>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> SellOrder([FromServices] KyubeyContext db, string tokenId, CancellationToken cancellationToken)
        {
            var orders = await db.DexSellOrders
                        .Where(x => x.TokenId == tokenId)
                        .OrderBy(x => x.UnitPrice)
                        .Take(15)
                        .ToListAsync(cancellationToken);
            orders.Reverse();

            var responseData = orders
                .Select(x => new GetBaseOrderResponse
                {
                    UnitPrice = x.UnitPrice,
                    Amount = x.Bid,
                    Total = x.Ask
                });

            return ApiResult(responseData);
        }

        [HttpGet("/api/v1/lang/{langId}/token/{tokenId}/match")]
        [ProducesResponseType(typeof(ApiResult<List<GetRecentTransactionResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> RecentTransactionRecord([FromServices] KyubeyContext db, string tokenId, CancellationToken token)
        {
            var responseData = await db.MatchReceipts
                .Where(x => x.TokenId == tokenId)
                .OrderByDescending(x => x.Time)
                .Take(20)
                .ToListAsync(token);

            return ApiResult(responseData.Select(x => new GetRecentTransactionResponse
            {
                UnitPrice = x.UnitPrice,
                Amount = x.Ask,
                Time = x.Time
            }));
        }
    }
}
