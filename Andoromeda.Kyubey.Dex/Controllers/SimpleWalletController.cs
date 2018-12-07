using Andoromeda.Framework.EosNode;
using Andoromeda.Kyubey.Dex.Lib;
using Andoromeda.Kyubey.Dex.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    [Route("api/v1/[controller]")]
    public class SimpleWalletController : BaseController
    {
        [HttpPost("callback/login")]
        public async Task<IActionResult> Post([FromBody]PostSimpleWalletLoginRequest request, [FromServices] NodeApiInvoker nodeApiInvoker,  [FromServices] EosSignatureValidator eosSignatureValidator, CancellationToken cancellationToken)
        {
            var accountInfo = await nodeApiInvoker.GetAccountAsync(request.Account, cancellationToken);
            var keys = accountInfo.Permissions.Select(x => x.RequiredAuth).SelectMany(x => x.Keys).Select(x => x.Key).ToList();
            var data = request.Timestamp + request.Account + request.UUID + request.Ref;

            var verify = keys.Any(k => eosSignatureValidator.Verify(request.Sign, data, k).Result);

            if (verify)
            {
                return Json(new PostSimpleWalletLoginResponse
                {
                    Code = 0
                });
            }

            return Json(new PostSimpleWalletLoginResponse
            {
                Code = 1,
                Error = "sign error"
            });
        }
    }
}
