using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Andoromeda.CleosNet.Client;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;

namespace Andoromeda.Kyubey.Dex.Bot
{

    class Program
    {

        struct TradingState
        {
            public bool Buy;
            public bool Sell;
            public double Price;
        }

        static CleosClient client;
        static Config config;
        static KyubeyContext db;
        static MailMessage mailMessage;
        static SmtpClient sendEmails;
        static Dictionary<string, TradingState> states;
        static Random random = new Random();

        static void InitializeOneBox()
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            var builder = new DbContextOptionsBuilder();
            builder.UseMySql(config.MySql);
            db = new KyubeyContext(builder.Options);
            client = new CleosClient();
            client.CreateWalletAsync("default", "/home/cleos-net/wallet/wallet.key").Wait();
            client.ImportPrivateKeyToWalletAsync(config.PrivateKey, "default").Wait();
        }

        static double GetPrice(string symbol)
        {
            var token = db.Tokens.Single(x => x.Id == symbol);
            var token2 = config.Pairs.Single(x => x.Symbol == symbol);
            double matched = 99999999;
            try 
            {   
                matched = db.MatchReceipts
                    .Where(x => x.TokenId == symbol)
                    .OrderByDescending(x => x.Time)
                    .Select(x => x.UnitPrice)
                    .FirstOrDefault();
            }
            catch 
            { 
            }
            return Math.Min(Math.Min(Math.Min(token2.Price, token.WhaleExPrice ?? 99999999), token.NewDexAsk ?? 99999999), matched);
        }

        static async Task BuyAsync(string token, string eos, string account, int retryLeft = 3) 
        { 
            try
            {

                var result = await client.PushActionAsync(
                        "eosio.token", "transfer", account, "active", new object[] {
                        account, "kyubeydex.bp",
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
                        account, "kyubeydex.bp",
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

        static async Task SelfBrokeredBotAsync()
        {
            InitializeOneBox();
            InitializeMailBox();

            Console.WriteLine("Total {0} tokens found in configuration file.", config.Pairs.Count());
            Console.WriteLine("—————————— GO ——————————");
            while (true)
            {
                foreach (var pair in config.Pairs)
                {
                    try 
                    {
                        double price = GetPrice(pair.Symbol);

                        Console.WriteLine("[{2}] {0}: Price: {1} EOS", DateTime.Now.ToString("T"),
                            price.ToString("0.0000"),
                            pair.Symbol);

                        var eosBalance = await client.GetCurrencyBalanceAsync("eosio.token", config.TestAccount, "EOS");
                        if (eosBalance.Result.Amount < price)
                        {
                            Console.WriteLine("EOS balance: {0}, Price = {1}, Cannot execute buy", eosBalance.Output, price);

                            SendEmail("BOT: " + "时间: " + DateTime.Now.ToString("T") + "TokenID: " + pair.Symbol + "EOS 余额不足",
                                String.Format("EOS balance: {0}, Price = {1}, Cannot execute buy", eosBalance.Output, price));

                            continue;
                        }

                        Console.WriteLine("Buy！ ");
                        await BuyAsync("1.0000 " + pair.Symbol, price.ToString("0.0000") + " EOS", config.TestAccount);

                        Console.WriteLine("Wait 0.5 seconds");
                        await Task.Delay(500);

                        Console.WriteLine("Sell! ");
                        await SellAsync("1.0000 " + pair.Symbol, price.ToString("0.0000") + " EOS", pair.Contract, config.TestAccount);

                        var waitingTime = random.Next(1000 * 60 * 1, 1000 * 60 * 3);
                        Console.WriteLine("———— Next run time: {0} ————", StringToDateTime(waitingTime));
                        await Task.Delay(waitingTime);
                    } 
                    catch (Exception ex)
                    {
                        SendEmail("BOT: " + "时间: " + DateTime.Now.ToString("T") + "TokenID: " + pair.Symbol + "发生异常",
                            ex.ToString());
                    }
                }
            }
        }

        static void InitializeMailBox()
        {
            mailMessage = new MailMessage
            {
                From = new MailAddress(config.EmailAddress)
            };
            mailMessage.To.Add(new MailAddress(config.EmailAddress));
            mailMessage.CC.Add(config.SendTo);
            sendEmails = new SmtpClient
            {
                Host = config.SmtpHost,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(config.EmailAddress, config.EmailPassword)
            };
        }

        static bool SendEmail(string Subject,string Body)
        {
            try
            {
                    mailMessage.Subject = Subject;
                    mailMessage.Body = Body;
                    sendEmails.Send(mailMessage);
                    Console.WriteLine("Email has been sent successfully.");
                    return false;
            }
            catch
            {
                Console.WriteLine("Email has been sent failed.");
                return true;
            }
        }


        static string StringToDateTime(double createTime)
        {
            var fh = DateTime.Parse(DateTime.Now.ToString()).AddMilliseconds(createTime);
            return fh.ToString("T");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            SelfBrokeredBotAsync().Wait();
            Console.WriteLine("End");
        }

    }
}
