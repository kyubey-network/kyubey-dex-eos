using System.Collections.Generic;

namespace Andoromeda.Kyubey.Dex.Bot
{
    public class Config
    {
        public string MySql { get; set; }

        public string EncryptText { get; set; }

        public string TestAccount { get; set; }

        public IList<Pair> Pairs { get; set; }

        public string EmailAddress { get; set; }

        public string SmtpHost { get; set; }

        public string EmailPassword { get; set; }

        public string SendTo { get; set; }
    }

    public class Pair
    {
        public string Contract { get; set; }

        public string Symbol { get; set; }

        public double MaxPrice { get; set; }
    }
}
