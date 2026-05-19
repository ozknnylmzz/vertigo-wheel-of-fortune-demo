using UnityEngine;
using Vertigo.WheelOfFortune.Rewards.Data;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public struct WheelRewardData
    {
        public WheelRewardData(WheelRewardType rewardType, Sprite rewardIcon, string rewardAmountValue)
        {
            this.rewardType = rewardType;
            this.rewardIcon = rewardIcon;
            this.rewardAmountValue = rewardAmountValue;
        }

        public WheelRewardType rewardType;
        public Sprite rewardIcon;
        public string rewardAmountValue;
    }
}
