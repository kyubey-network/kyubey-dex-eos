using System;
using System.Collections.Generic;

namespace Andoromeda.Kyubey.Dex.Bot
{
    public static class BotMath
    {
        static int amountPrecision = 4;
        static int totalPrecision = 4;
        static Random random = new Random();

        public static int GetPrecision(decimal val)
        {
            string valString = val.ToString().TrimEnd('0');
            int result = valString.Length - valString.IndexOf(".") - 1;
            return result;
        }

        static int GetPricePrecision(string symbol)
        {
            if (new List<string> { "CMU" }.Contains(symbol))
            {
                return 4;
            }
            else if (new List<string> { "IQ" }.Contains(symbol))
            {
                return 6;
            }
            return 8;
        }

        public static decimal GetEffectiveRandomAmount(ref decimal price, decimal maxEOSTotal, decimal tokenBalance, string symbol)
        {
            var amount = GetMinAmount(ref price, symbol);
            var minEosTotal = price * amount;

            var maxAvailEOSTotal = maxEOSTotal > (price * tokenBalance) ? (price * tokenBalance) : maxEOSTotal;
            if (minEosTotal > maxAvailEOSTotal)
            {
                var pricePrecision = GetPrecision(price);
                if (pricePrecision > totalPrecision)
                {
                    price = Math.Round(price, pricePrecision - 1);
                    amount = GetEffectiveRandomAmount(ref price, maxEOSTotal, tokenBalance, symbol);
                }
                else return 0;
            }

            int maxCount = (int)(maxAvailEOSTotal / (amount * price));
            var randomCount = random.Next(1, maxCount + 1);
            return randomCount * amount;
        }

        public static decimal GetMinAmount(ref decimal price, string symbol)
        {
            var pricePrecision = GetPricePrecision(symbol);
            price = Math.Round(price, pricePrecision);
            var pV = (long)(price * (long)Math.Pow(10, pricePrecision));
            var lcmV = LCM((long)pV, (long)Math.Pow(10, pricePrecision));
            var minAmountV = lcmV / pV;
            var minAmount = minAmountV / (decimal)Math.Pow(10, amountPrecision);
            return minAmount;
        }

        public static long LCM(long a, long b)
        {
            long num1, num2;
            if (a > b)
            {
                num1 = a; num2 = b;
            }
            else
            {
                num1 = b; num2 = a;
            }

            for (int i = 1; i < num2; i++)
            {
                if ((num1 * i) % num2 == 0)
                {
                    return i * num1;
                }
            }
            return num1 * num2;
        }
    }
}
