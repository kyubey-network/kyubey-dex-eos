using Andoromeda.CleosNet.Client;
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
                foreach (var pair in configObject.Pairs)
                {
                    try
                    {
                        double price = await GetPriceAsync(pair.Symbol);

                        Console.WriteLine("[{2}] {0}: Price: {1} EOS", DateTime.Now.ToString("T"),
                            price.ToString("0.0000"),
                            pair.Symbol);

                        var eosBalance = await client.GetCurrencyBalanceAsync("eosio.token", configObject.TestAccount, "EOS");
                        if (eosBalance.Result.Amount < price)
                        {
                            Console.WriteLine("EOS balance: {0}, Price = {1}, Cannot execute buy", eosBalance.Output, price);

                            await EmailService.SendEmailAsync("BOT: " + "时间: " + DateTime.Now.ToString("T") + "TokenID: " + pair.Symbol + "EOS 余额不足",
                                                                String.Format("EOS balance: {0}, Price = {1}, Cannot execute buy", eosBalance.Output, price),
                                                                configObject.EmailAddress,
                                                                configObject.SendTo
                                                               );

                            continue;
                        }

                        Console.WriteLine("Buy！ ");
                        await BuyAsync("1.0000 " + pair.Symbol, price.ToString("0.0000") + " EOS", configObject.TestAccount);

                        Console.WriteLine("Wait 0.5 seconds");
                        await Task.Delay(500);

                        Console.WriteLine("Sell! ");
                        await SellAsync("1.0000 " + pair.Symbol, price.ToString("0.0000") + " EOS", pair.Contract, configObject.TestAccount);

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
            await client.CreateWalletAsync("default", "/home/cleos-net/wallet/wallet.key");
            var privateKey = SecurityTool.Decrypt(config["encryptText"], config["encryptKey"]);
            await client.ImportPrivateKeyToWalletAsync(privateKey, "default");
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
            return Math.Min(Math.Min(Math.Min(configToken.MaxPrice, dbToken.WhaleExPrice ?? 99999999), dbToken.NewDexAsk ?? 99999999), matched);
        }

        static async Task BuyAsync(string token, string eos, string account, int retryLeft = 3)
        {
            try
            {
                var result = await client.PushActionAsync(
                        "eosio.token", "transfer", account, "active", new object[] {
                        account, DexAccount,
                        eos,
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
                await BuyAsync(token, eos, account, --retryLeft);
            }
        }

        static async Task SellAsync(string token, string eos, string contract, string account, int retryLeft = 3)
        {
            try
            {
                var result = await client.PushActionAsync(
                        contract, "transfer", account, "active", new object[] {
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
                await SellAsync(token, eos, contract, account, --retryLeft);
            }
        }

        static string MillisecondsSpanToDateTimeString(double createTime)
        {
            var fh = DateTime.Parse(DateTime.Now.ToString()).AddMilliseconds(createTime);
            return fh.ToString("T");
        }
    }
}
