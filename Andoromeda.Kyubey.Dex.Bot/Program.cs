using Andoromeda.CleosNet.Client;
using Andoromeda.Framework.EosNode;
using Andoromeda.Kyubey.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Bot
{
    class Program
    {
        const string DexAccount = "kyubeydex.bp";
        static CleosClient client;
        static Config configObject;
        static KyubeyContext dbContext;
        static Random random = new Random();
        const decimal MaxEOSExchangeTotal = 0.02m;
        static NodeApiInvoker nodeAPI = new NodeApiInvoker(new DefaultNodeProvider());

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start");
            await InitAsync();
            await StartBotMatchAsync();
            Console.WriteLine("End");
        }

        static async Task InitAsync()
        {
            await InitializeOneBox();
            await EmailService.InitializeMailBoxAsync(configObject.EmailAddress, configObject.EmailPassword, configObject.SmtpHost);
        }

        static async Task StartBotMatchAsync()
        {
            Console.WriteLine("Total {0} tokens found in configuration file.", configObject.Pairs.Count());
            Console.WriteLine("—————————— GO ——————————");
            while (true)
            {
                //foreach (var pair in configObject.Pairs)
                {
                    var randomPairIndex = random.Next(configObject.Pairs.Count());
                    var pair = configObject.Pairs[randomPairIndex];
                    try
                    {
                        decimal price = (decimal)await GetPriceAsync(pair.Symbol);

                        Console.WriteLine("[{2}] {0}: Price: {1} EOS", DateTime.Now.ToString("T"),
                            price.ToString("0.00000000"),
                            pair.Symbol);

                        var eosBalance = await client.GetCurrencyBalanceAsync("eosio.token", configObject.TestAccount, "EOS");
                        var tokenBalance = await nodeAPI.GetCurrencyBalanceAsync(configObject.TestAccount, pair.Contract, pair.Symbol);

                        if ((decimal)eosBalance.Result.Amount < price)
                        {
                            Console.WriteLine("EOS balance: {0}, Price = {1}, Cannot execute buy", eosBalance.Output, price);

                            await EmailService.SendEmailAsync("BOT: " + "时间: " + DateTime.Now.ToString("T") + "TokenID: " + pair.Symbol + "EOS 余额不足",
                                                                String.Format("EOS balance: {0}, Price = {1}, Cannot execute buy", eosBalance.Output, price),
                                                                configObject.EmailAddress,
                                                                configObject.SendTo
                                                               );
                            continue;
                        }

                        var amount = BotMath.GetEffectiveRandomAmount(ref price, MaxEOSExchangeTotal, (decimal)tokenBalance, pair.Symbol);

                        Console.WriteLine("Sell! ");
                        await AutoSellAsync(pair.Symbol, amount, price, pair.Contract);
                        Console.WriteLine($"Sell {amount} {pair.Symbol},total {price * amount} EOS");

                        Console.WriteLine("Wait 0.2 seconds");
                        await Task.Delay(200);

                        Console.WriteLine("Buy！ ");
                        await AutoBuyAsync(pair.Symbol, amount, price);
                        Console.WriteLine($"Buy {amount} {pair.Symbol},total {price * amount} EOS");

                        var waitingTime = random.Next(1000 * 60 * 1, 1000 * 60 * 3);
                        Console.WriteLine("———— Next run time: {0} ————", MillisecondsSpanToDateTimeString(waitingTime));
                        await Task.Delay(waitingTime);
                    }
                    catch (Exception ex)
                    {
                        await EmailService.SendEmailAsync("BOT: " + "时间: " + DateTime.Now.ToString("T") + "TokenID: " + pair.Symbol + "发生异常",
                                                            ex.ToString(),
                                                            configObject.EmailAddress,
                                                            configObject.SendTo
                                                          );
                    }
                }
            }
        }

        static async Task InitializeOneBox()
        {
            var configFileName = "config";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{configFileName}.json")
                .AddEnvironmentVariables();
            var config = builder.Build();
            configObject = config.Get<Config>();

            var optionsBuilder = new DbContextOptionsBuilder<KyubeyContext>();
            optionsBuilder.UseMySql(config["MySQL"]);
            dbContext = new KyubeyContext(optionsBuilder.Options);

            client = new CleosClient();
            var createResult = await client.CreateWalletAsync("default", "/home/cleos-net/wallet/wallet.key");
            var privateKey = SecurityTool.Decrypt(config["EncryptText"], config["EncryptKey"]);
            //manual unlock
            //var unlockResult = await client.UnlockWalletAsync(SecurityTool.Decrypt(config["EncryptWalletKey"], config["EncryptKey"]));
            var importResult = await client.ImportPrivateKeyToWalletAsync(privateKey, "default");
        }

        static async Task<double> GetPriceAsync(string symbol)
        {
            var dbToken = await dbContext.Tokens.SingleAsync(x => x.Id == symbol);
            var configToken = configObject.Pairs.Single(x => x.Symbol == symbol);
            double matched = 99999999;
            try
            {
                matched = dbContext.MatchReceipts
                    .Where(x => x.TokenId == symbol)
                    .OrderByDescending(x => x.Time)
                    .Select(x => x.UnitPrice)
                    .FirstOrDefault();
            }
            catch
            {
            }
            var priceR = Math.Min(Math.Min(Math.Min(configToken.MaxPrice, dbToken.WhaleExPrice ?? 99999999), dbToken.NewDexAsk ?? 99999999), (matched == 0 ? 99999999 : matched));
            return Math.Round(priceR, 8);
        }

        static async Task<decimal> AutoBuyAsync(string token, decimal amount, decimal price)
        {
            var total = price * amount;

            if (total > 0 && total <= MaxEOSExchangeTotal)
            {
                await BuyAsync($"{amount.ToString("0.0000")} {token}", total.ToString("0.0000") + " EOS", configObject.TestAccount, "owner");
                return amount;
            }

            return 0;
        }

        static async Task AutoSellAsync(string token, decimal amount, decimal price, string contract)
        {
            var total = amount * price;
            await SellAsync($"{amount.ToString("0.0000")} {token}", total.ToString("0.0000") + " EOS", contract, configObject.TestAccount, "owner");
        }


        static async Task BuyAsync(string token, string totalEos, string account, string permission = "active", int retryLeft = 3)
        {
            try
            {
                var result = await client.PushActionAsync(
                        "eosio.token", "transfer", account, permission, new object[] {
                        account, DexAccount,
                        totalEos,
                        token });
                if (!result.IsSucceeded)
                {
                    throw new Exception(result.Error);
                }
            }
            catch
            {
                if (retryLeft == 0)
                {
                    throw;
                }
                await BuyAsync(token, totalEos, account, permission, retryLeft: --retryLeft);
            }
        }

        static async Task SellAsync(string token, string eos, string contract, string account, string permission = "active", int retryLeft = 3)
        {
            try
            {
                var result = await client.PushActionAsync(
                        contract, "transfer", account, permission, new object[] {
                        account, DexAccount,
                        token,
                        eos });
                if (!result.IsSucceeded)
                {
                    throw new Exception(result.Error);
                }
            }
            catch
            {
                if (retryLeft == 0)
                {
                    throw;
                }
                await SellAsync(token, eos, contract, account, permission, retryLeft: --retryLeft);
            }
        }

        static string MillisecondsSpanToDateTimeString(double createTime)
        {
            var fh = DateTime.Parse(DateTime.Now.ToString()).AddMilliseconds(createTime);
            return fh.ToString("T");
        }
    }
}
