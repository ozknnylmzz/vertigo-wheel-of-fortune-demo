using System;
using System.Globalization;

namespace Vertigo.WheelOfFortune.Rewards.Data
{
    public static class WheelRewardAmountValueUtility
    {
        public static int ParseAmount(string rewardAmountValue)
        {
            if (string.IsNullOrWhiteSpace(rewardAmountValue))
            {
                return 0;
            }

            string digits = string.Empty;
            for (int i = 0; i < rewardAmountValue.Length; i++)
            {
                char c = rewardAmountValue[i];
                if (c >= '0' && c <= '9')
                {
                    digits += c;
                }
            }

            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount)
                ? amount
                : 0;
        }

        public static string ResolveAmountPrefix(string rewardAmountValue)
        {
            return !string.IsNullOrWhiteSpace(rewardAmountValue)
                && rewardAmountValue.TrimStart().StartsWith("x", StringComparison.OrdinalIgnoreCase)
                ? "x"
                : string.Empty;
        }

        public static string FormatAmount(int amount, string amountPrefix)
        {
            return amountPrefix + amount.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}
