using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Andoromeda.Kyubey.Dex.Repository.TokenRespository;

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
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetRecentTransactionResponse>>), 200)]
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

        [HttpGet("/api/v1/lang/{lang}/token/{id}")]
        [ProducesResponseType(typeof(ApiResult<GetTokenDetailResponse>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> TokenDetails(
            string id,
            [FromServices] KyubeyContext db,
            [FromServices] TokenRepositoryFactory tokenRepositoryFactory,
            [FromQuery] GetNewsListRequest request,
            CancellationToken cancellationToken
            )
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

            var tokenRepository = await tokenRepositoryFactory.CreateAsync(request.Lang);
            var token = tokenRepository.GetSingle(id);

            var responseData = new GetTokenDetailResponse()
            {
                Symbol = token.Id,
                CurrentPrice = todayList.FirstOrDefault(t => t.TokenId == token.Id)?.CurrentPrice ?? yesterdayList.FirstOrDefault(t => t.TokenId == token.Id)?.CurrentPrice ?? 0,
                MaxPriceRecentDay = todayList.FirstOrDefault(s => s.TokenId == token.Id)?.MaxPrice ?? 0,
                MinPriceRecentDay = todayList.FirstOrDefault(s => s.TokenId == token.Id)?.MinPrice ?? 0,
                VolumeRecentDay = todayList.FirstOrDefault(s => s.TokenId == token.Id)?.Volume ?? 0,
                IsRecommend = true,
                IconSrc = $"/token_assets/{token.Id}/icon.png",
                Priority = token.Priority,
                Description = tokenRepository.GetTokenIncubationDescription(id, request.Lang),
                TotalSupply = token.Basic?.Total_Supply ?? 0,
                TotalCirculate = token.Basic?.Total_Circulate ?? 0,
                Contract = new GetTokenResultContract()
                {
                    Depot = token.Basic?.Contract?.Depot,
                    Pricing = token.Basic?.Contract?.Pricing,
                    Transfer = token.Basic?.Contract?.Transfer
                },
                Website = token.Basic.Website
            };
            return ApiResult(responseData);
        }


        [HttpGet("/api/v1/lang/{lang}/token/{id}/candlestick")]
        public async Task<IActionResult> Candlestick([FromServices] KyubeyContext db, GetCandlestickRequest Request, CancellationToken token)
        {
            var ticks = new TimeSpan(0, 0, Request.Period);
            var begin = new DateTime(Request.Begin.Ticks / ticks.Ticks * ticks.Ticks);
            var end = new DateTime(Request.End.Ticks / ticks.Ticks * ticks.Ticks);
            var data = await db.MatchReceipts
                .Where(x => x.TokenId == Request.Id)
                .Where(x => x.Time < end)
                .OrderBy(x => x.Time)
                .GroupBy(x => x.Time >= begin ? x.Time.Ticks / ticks.Ticks * ticks.Ticks : 0)
                .Select(x => new GetCandlestickResponse
                {
                    Time = new DateTime(x.Key),
                    Min = x.Select(y => y.UnitPrice).Min(),
                    Max = x.Select(y => y.UnitPrice).Max(),
                    Opening = x.Select(y => y.UnitPrice).FirstOrDefault(),
                    Closing = x.OrderByDescending(y => y.Time).Select(y => y.UnitPrice).FirstOrDefault(),
                    Volume = x.Count()
                })
                .ToListAsync(token);

            if (data.Count <= 1) return ApiResult(data.Where(x => x.Time >= begin).OrderBy(x => x.Time));

            for (var i = begin; i < end; i = i.Add(ticks))
            {
                if (data.Any(x => x.Time == i)) continue;
                var prev = data
                    .Where(x => x.Time < i)
                    .OrderBy(x => x.Time)
                    .LastOrDefault();
                if (prev == null) continue;
                data.Add(new GetCandlestickResponse
                {
                    Min = prev.Closing,
                    Max = prev.Closing,
                    Closing = prev.Closing,
                    Opening = prev.Closing,
                    Time = i,
                    Volume = 0
                });
            }
            return ApiResult(data.Where(x => x.Time >= begin).OrderBy(x => x.Time));
        }

    }
}
