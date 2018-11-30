using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Dex.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    public class NewsController : BaseController
    {
        [HttpGet("api/v1/lang/{lang}/news")]
        public async Task<IActionResult> List([FromServices] NewsRepositoryFactory newsRepositoryFactory, [FromQuery] GetNewsListRequest request)
        {
            var newRepository = await newsRepositoryFactory.CreateAsync(request.Lang);
            var responseData = newRepository
                .EnumerateAll()
                .OrderByDescending(x => x.PublishedAt)
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(x => new GetNewsListResponse()
                {
                    Id = x.Id,
                    Pinned = x.IsPinned,
                    Time = x.PublishedAt,
                    Title = x.Title
                });

            return ApiResult(responseData);
        }

        [HttpGet("api/v1/lang/{lang}/news/{id}")]
        public async Task<IActionResult> Content([FromServices] NewsRepositoryFactory newsRepositoryFactory, [FromQuery] GetContentBaseRequest request)
        {
            var newRepository = await newsRepositoryFactory.CreateAsync(request.Lang);
            var newsObj = newRepository.GetSingle(request.Id);

            if (newsObj == null)
            {
                return ApiResult(404, "Not Found");
            }
            return ApiResult(new GetNewsContentResponse()
            {
                Id = newsObj.Id,
                Content = newsObj.Content,
                Pinned = newsObj.IsPinned,
                Time = newsObj.PublishedAt,
                Title = newsObj.Title
            });
        }
    }
}
