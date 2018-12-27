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
        static CleosClient client;

        static Config config;
        static KyubeyContext db;

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


            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("littlesound@126.com");
            mailMessage.To.Add(new MailAddress("t-littlesound@andoromeda.io"));
            mailMessage.CC.Add("911574351@qq.com,little_sound@qq.com,amandatian@andoromeda.io,minakokojima@andoromeda.io,may.xu@andoromeda.io");
            SmtpClient sendEmails = new SmtpClient();
            sendEmails.Host = "smtp.126.com";
            sendEmails.EnableSsl = true;
            sendEmails.UseDefaultCredentials = false;
            sendEmails.Credentials = new NetworkCredential("littlesound@126.com", "cV2oUtwDGH");

            Console.WriteLine("———————— GO ————————");

            while (true)
            {
                foreach(var pair in config.Pairs)
                {
                    var matches = db.MatchReceipts
                        .Where(x => x.TokenId == pair.Symbol)
                        .OrderByDescending(x => x.Time)
                        .Select(x => new
                        {
                            unitPrice = x.UnitPrice
                        }).FirstOrDefault();
                    currentTime = DateTime.Now;
                    var price = Math.Min(matches.unitPrice, pair.Price);
                    Console.WriteLine("[{2}] {0}: Price: {1} EOS", currentTime.ToString("T"), price.ToString("0.0000"), pair.Symbol);
                    Console.WriteLine("Buy！ ");
                    var z = await client.PushActionAsync("eosio.token", "transfer", config.TestAccount, "active", new object[] { config.TestAccount, "kyubeydex.bp", matches.unitPrice.ToString("0.0000") + " EOS", "1.0000 " + pair.Symbol });
                    if (z.IsSucceeded == false)
                    {
                        Console.WriteLine("Error:" + z.Error);
                        try
                        {
                            mailMessage.Subject = "BOT: 买入时出现错误";
                            mailMessage.Body = "时间: " + currentTime.ToString("T") + "错误内容 Error: " + z.Error;
                            sendEmails.Send(mailMessage);
                            Console.Write("<Yes>Successful mail delivery");
                        }
                        catch
                        {
                            Console.Write("<No>Mail Delivery Failed");
                        }
                    }
                    Console.WriteLine("Wait 0.5 seconds");
                    await Task.Delay(500);
                    Console.WriteLine("Sell! ");
                    var y = await client.PushActionAsync("dacincubator", "transfer", config.TestAccount, "active", new object[] { config.TestAccount, "kyubeydex.bp", "1.0000 " + pair.Symbol, price.ToString("0.0000") + " EOS" });
                    if (y.IsSucceeded == false)
                    {
                        try
                        {
                            Console.WriteLine("Error:" + y.Error);
                            mailMessage.Subject = "BOT: 卖出时出现错误";
                            mailMessage.Body = "时间: " + currentTime.ToString("T") + "错误内容 Error:" + y.Error;
                            sendEmails.Send(mailMessage);
                            Console.Write("<Yes>Successful mail delivery");
                        }
                        catch
                        {
                            Console.Write("<No>Mail Delivery Failed");
                        }

                    }
                    Console.WriteLine("———— Wait 2 minutes————");
                    await Task.Delay(1000 * 60 * 2);
                }
            }
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
