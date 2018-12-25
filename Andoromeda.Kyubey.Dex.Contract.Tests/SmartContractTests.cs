using System;
using System.IO;
using System.Linq;
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

        [Fact]
        public async Task CompileAndSetContractTests()
        {
            // Arrange
            await client.PushActionAsync("eosio.token", "create", "eosio.token", "active", new[] { "eosio.token", "1000000000.0000 KBY" });
            await client.PushActionAsync("eosio.token", "issue", "eosio.token", "active", new[] { "eosio.token", "1000000000.0000 KBY", "OneBox Init" });
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
            var result = await client.SetContractAsync("/opt/eosio/contracts/pomelo", "pomelo", "pomelo");
            await client.PushActionAsync("pomelo", "setwhitelist", "pomelo", "active", new[] { "KBY", "eosio.token" });

            // Assert
            Assert.True(result.IsSucceeded);
        }

        [Fact]
        public async Task PublishBuyOrderTests()
        {
            // Arrange
            var beforeBalance = await client.GetCurrencyBalanceAsync("eosio.token", "eosio.token");
            await client.PushActionAsync("eosio.token", "transfer", "eosio.token", "active", new object[] { "eosio.token", "pomelo", "1.0000 EOS", "1.0000 KBY" });

            // Assert
            var afterBalance = await client.GetCurrencyBalanceAsync("eosio.token", "eosio.token");
            Assert.Equal(beforeBalance.Result.First().Amount - 1, afterBalance.Result.First().Amount);
        }
    }
}
