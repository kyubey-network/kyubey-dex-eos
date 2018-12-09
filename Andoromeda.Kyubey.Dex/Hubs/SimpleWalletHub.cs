using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;

namespace Andoromeda.Kyubey.Dex.Hubs
{
    public class SimpleWalletHub : Hub
    {
        private static MD5 _md5 = new MD5CryptoServiceProvider();

        private Guid ToGuid(string str)
        {
            var bytes = Encoding.ASCII.GetBytes(str);
            var result = _md5.ComputeHash(bytes);
            var uuid = new Guid(result);
            return uuid;
        }

        public Guid GetUUID()
        {
            return ToGuid(Context.ConnectionId);
        }
    }
}
