using System;
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

        struct TradingNormal
        {
            public bool Buy;
            public bool Sell;
            public double price;
        }

        static CleosClient client;

        static Config config;
        static KyubeyContext db;
        static MailMessage mailMessage;
        static SmtpClient sendEmails;

        static async Task SelfBrokeredBotAsync()
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            var builder = new DbContextOptionsBuilder();
            builder.UseMySql(config.MySql);
            db = new KyubeyContext(builder.Options);
            client = new CleosClient();
            client.CreateWalletAsync("default", "/home/cleos-net/wallet/wallet.key").Wait();
            client.ImportPrivateKeyToWalletAsync(config.PrivateKey, "default").Wait();
            var currentTime = System.DateTime.Now;

            InitializeMailBox();

            var pairsLength = 0;
            foreach (var pair in config.Pairs)
            {
                pairsLength++;
            }
            TradingNormal[] TradingNormals = new TradingNormal[pairsLength];
            for (int i = 0; i < pairsLength; i++ ){
                TradingNormals[i].Buy = true;
                TradingNormals[i].Sell = true;
                TradingNormals[i].price = 0;
            }

            Console.WriteLine("Number of tokens: {0}", pairsLength);
            Console.WriteLine("—————————— GO ——————————");
            while (true)
            {

                Random timeSeed = new Random();
                var loopCounter = 0;

                foreach (var pair in config.Pairs)
                {

                    try 
                    {
                        var matches = db.MatchReceipts
                        .Where(x => x.TokenId == pair.Symbol)
                        .OrderByDescending(x => x.Time)
                        .Select(x => new
                        {
                            unitPrice = x.UnitPrice
                        }).FirstOrDefault();
                    
                        currentTime = DateTime.Now;
                        TradingNormals[loopCounter].price = Math.Min(matches.unitPrice, pair.Price);
                    }
                    catch
                    {
                        Console.WriteLine("<No>MySql error, <Note> The price will use the previous result.");
                        Mailox("BOT: " + "时间: " + currentTime.ToString("T") + "TokenID: " + pair.Symbol + " 读取数据库时出错",
                            " MySql出错,<注意>EOS价格将采用上一次结果。");
                    }
                    Console.WriteLine("[{2}] {0}: Price: {1} EOS", currentTime.ToString("T"),
                        TradingNormals[loopCounter].price.ToString("0.0000"),
                        pair.Symbol);

                    var eosBalance = await client.GetCurrencyBalanceAsync("eosio.token", config.TestAccount);
                    Console.WriteLine("EOS balance: {0}",eosBalance.Output);
                    Console.WriteLine("Buy！ ");
                    var buyErrorCount = 0;
                    do
                    {
                        var z = await client.PushActionAsync(
                           "eosio.token", "transfer", config.TestAccount, "active", new object[] {
                                config.TestAccount, "kyubeydex.bp", 
                                TradingNormals[loopCounter].price.ToString("0.0000")  + " EOS",
                                "1.0000 " + pair.Symbol });

                        if (z.IsSucceeded == false)
                        {
                            buyErrorCount++;
                            Console.WriteLine("Error:" + z.Error);
                            if (buyErrorCount <= 3)
                            {
                                Console.WriteLine("Will try again after 1 seconds");
                                await Task.Delay(1000);
                            }
                            else if (TradingNormals[loopCounter].Buy)
                            {
                                TradingNormals[loopCounter].Buy = Mailox("BOT: " + pair.Symbol + "买入时出现错误",
                                    "时间: " + currentTime.ToString("T") + "错误内容 Error: " + z.Error);
                            }
                            else
                            {
                                Console.WriteLine("<No>Repeat error, stop mail");
                            }
                        }
                        else
                        {
                            if (!TradingNormals[loopCounter].Buy)
                            {
                                Mailox("BOT: " + pair.Symbol + "买入功能恢复正常",
                                    "恢复正常");
                            }
                            TradingNormals[loopCounter].Buy = true;
                        }
                    } while (buyErrorCount >0 && buyErrorCount <= 3);

                    Console.WriteLine("Wait 0.5 seconds");
                    await Task.Delay(500);

                    Console.WriteLine("Sell! ");

                    var sellErrorCount = 0;
                    do
                    {
                        var y = await client.PushActionAsync(
                            "dacincubator", "transfer", config.TestAccount, "active", new object[] { 
                            config.TestAccount, "kyubeydex.bp", "1.0000 " + pair.Symbol,
                            TradingNormals[loopCounter].price.ToString("0.0000") + " EOS" });

                        if (y.IsSucceeded == false)
                        {

                            sellErrorCount++;
                            Console.WriteLine("Error:" + y.Error);
                            if (sellErrorCount <= 3)
                            {
                                Console.WriteLine("Will try again after 1 seconds");
                                await Task.Delay(1000);
                            }
                            else if (TradingNormals[loopCounter].Sell)
                            {
                                TradingNormals[loopCounter].Sell = Mailox("BOT: " + pair.Symbol + "卖出时出现错误",
                                    "时间: " + currentTime.ToString("T") + "错误内容 Error:" + y.Error);
                            }
                            else
                            {
                                Console.WriteLine("<No>Repeat error, stop mail");
                            }

                        }
                        else
                        {
                            if (!TradingNormals[loopCounter].Sell)
                            {
                                Mailox("BOT: " + pair.Symbol + "卖出功能恢复正常",
                                    "恢复正常");
                            }
                            TradingNormals[loopCounter].Sell = true;
                        }
                    } while (sellErrorCount > 0 && sellErrorCount <= 3);

                    var waitingTime = timeSeed.Next(1000 * 60 * 1, 1000 * 60 * 3);
                    Console.WriteLine("———— Next run time: {0} ————", StringToDateTime(waitingTime));
                    await Task.Delay(waitingTime);
                    loopCounter++;
                }
            }
        }
        static void InitializeMailBox()
        {


            mailMessage = new MailMessage
            {
                From = new MailAddress("littlesound@126.com")
            };
            mailMessage.To.Add(new MailAddress("t-littlesound@andoromeda.io"));
            mailMessage.CC.Add("911574351@qq.com,little_sound@qq.com,amandatian@andoromeda.io,minakokojima@andoromeda.io,may.xu@andoromeda.io");
            sendEmails = new SmtpClient
            {
                Host = "smtp.126.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("littlesound@126.com", "cV2oUtwDGH")
            };
        }

        static bool Mailox(string Subject,string Body)
        {
            try
            {
                    mailMessage.Subject = Subject;
                    mailMessage.Body = Body;
                    sendEmails.Send(mailMessage);
                    Console.WriteLine("<Yes>Successful mail delivery");
                    return false;
            }
            catch
            {
                Console.WriteLine("<No>Mail Delivery Failed");
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
            Console.ReadLine();
        }

    }
}
