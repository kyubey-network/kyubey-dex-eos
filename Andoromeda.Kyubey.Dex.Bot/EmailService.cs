using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Bot
{
    internal class EmailService
    {
        static MailMessage mailMessage;
        static SmtpClient sendEmails;
        public static async Task InitializeMailBoxAsync(string fromAddress, string pwd, string host)
        {
            mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress)
            };

            sendEmails = new SmtpClient
            {
                Host = host,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, pwd)
            };
        }

        public static async Task<bool> SendEmailAsync(string subject, string body, string toAddress, string ccAddress)
        {
            try
            {
                mailMessage.To.Add(toAddress);
                if (!string.IsNullOrWhiteSpace(ccAddress))
                    mailMessage.CC.Add(ccAddress);

                mailMessage.Subject = subject;
                mailMessage.Body = body;
                await sendEmails.SendMailAsync(mailMessage);
                Console.WriteLine("Email has been sent successfully.");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Email has been sent failed.");
                return true;
            }
        }
    }
}
