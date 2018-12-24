using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Andoromeda.CleosNet.Client;

namespace Andoromeda.Kyubey.Dex.Contract.Tests
{
    public class SmartContractTests
    {
        private CleosClient client;

        public SmartContractTests()
        {
            client = new CleosClient("http://localhost:8888");
            client.QuickLaunchOneBoxAsync().Wait();
        }

        private string GetProjectRootPath()
        {
            var current = Directory.GetCurrentDirectory();
            return current.Substring(0, current.IndexOf(@"Andoromeda.Kyubey.Dex.Contract.Tests") - 1);
        }

        [Fact(Skip = "Wait for contract upgrade to cdt")]
        public async Task CompileAndSetContractTests()
        {
            // Arrange
            await client.CreateFolderAsync("/opt/eosio/contracts/pomelo");
            await client.UploadFileAsync("/opt/eosio/contracts/pomelo/pomelo.hpp", File.ReadAllBytes(Path.Combine(GetProjectRootPath(), "Contract/pomelo.hpp")));
            await client.UploadFileAsync("/opt/eosio/contracts/pomelo/pomelo.cpp", File.ReadAllBytes(Path.Combine(GetProjectRootPath(), "Contract/pomelo.cpp")));
            await client.UploadFileAsync("/opt/eosio/contracts/pomelo/CMakeLists.txt", File.ReadAllBytes(Path.Combine(GetProjectRootPath(), "Contract/CMakeLists.txt")));
            await client.CompileSmartContractAsync("/opt/eosio/contracts/pomelo");
            await client.GenerateKeyValuePair("/home/cleos-net/wallet/pomelo.txt");
            var keys = await client.RetriveKeyPairsAsync("/home/cleos-net/wallet/pomelo.txt");
            await client.ImportPrivateKeyToWalletAsync(keys.PrivateKey, "eosio.token");
            await client.CreateAccountAsync("eosio", "pomelo", keys.PublicKey, keys.PublicKey);
            await client.PushActionAsync("eosio.token", "transfer", "eosio.token", "active", new[] { "eosio.token", "pomelo", "1000.0000 EOS", "" });
            await client.SetContractAsync("/opt/eosio/contracts/pomelo", "pomelo", "pomelo");
        }
    }
}
