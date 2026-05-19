using UnityEngine;

namespace Vertigo.WheelOfFortune.Rewards.Data
{
    public static class WheelRewardAmountResolver
    {
        private const int MaxLevel = 60;

        public static string Resolve(int level, WheelRewardType rewardType)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, MaxLevel);

            switch (rewardType)
            {
                case WheelRewardType.Cash:
                    return Format(ResolveCashAmount(normalizedLevel));
                case WheelRewardType.Gold:
                    return Format(ResolveGoldAmount(normalizedLevel));
                case WheelRewardType.Cards:
                case WheelRewardType.Points:
                    return Format(ResolveCardPointAmount(normalizedLevel));
                default:
                    return string.Empty;
            }
        }

        private static int ResolveCashAmount(int level)
        {
            if (IsGoldenLevel(level))
            {
                return level >= MaxLevel ? Random.Range(25000, 30001) : Random.Range(10000, 15001);
            }

            if (IsSilverLevel(level))
            {
                if (level <= 5)
                {
                    return Random.Range(2000, 5001);
                }

                if (level < 25)
                {
                    return Random.Range(5000, 10001);
                }

                if (level < 45)
                {
                    return Random.Range(10000, 15001);
                }

                return Random.Range(15000, 20001);
            }

            int step = ResolveBronzeStep(level);
            int minValue = step == 1 ? 600 : (step - 1) * 1000;
            return Random.Range(minValue, step * 1000 + 1);
        }

        private static int ResolveGoldAmount(int level)
        {
            if (IsGoldenLevel(level))
            {
                return level;
            }

            if (IsSilverLevel(level))
            {
                int silverLevel = ResolveSilverAmountLevel(level);
                return Random.Range(silverLevel, silverLevel + 6);
            }

            return ResolveBronzeStep(level);
        }

        private static int ResolveCardPointAmount(int level)
        {
            if (IsGoldenLevel(level))
            {
                return level;
            }

            return IsSilverLevel(level) ? ResolveSilverAmountLevel(level) : ResolveBronzeStep(level);
        }

        private static int ResolveBronzeStep(int level)
        {
            return Mathf.Max(1, Mathf.CeilToInt(level / 5f));
        }

        private static bool IsSilverLevel(int level)
        {
            return level == 1 || (level % 5 == 0 && !IsGoldenLevel(level));
        }

        private static bool IsGoldenLevel(int level)
        {
            return level % 30 == 0;
        }

        private static int ResolveSilverAmountLevel(int level)
        {
            return level == 1 ? 5 : level;
        }

        private static string Format(int amount)
        {
            return "x" + amount;
        }
    }
}
