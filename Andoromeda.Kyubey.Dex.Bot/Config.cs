using System.Collections.Generic;

namespace Andoromeda.Kyubey.Dex.Bot
{
    public class Config
    {
        public string MySql { get; set; }

        public string PrivateKey { get; set; }

        public IEnumerable<Pair> Pairs { get; set; }
    }

    public class Pair
    {
        public string Contract { get; set; }

        public string Symbol { get; set; }

        public double Price { get; set; }
    }
}
