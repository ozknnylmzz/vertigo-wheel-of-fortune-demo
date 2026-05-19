using UnityEngine;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public struct WheelRewardData
    {
        public WheelRewardData(string rewardKey, Sprite rewardIcon, string rewardAmountValue)
        {
            this.rewardKey = rewardKey;
            this.rewardIcon = rewardIcon;
            this.rewardAmountValue = rewardAmountValue;
        }

        public string rewardKey;
        public Sprite rewardIcon;
        public string rewardAmountValue;
    }
}
