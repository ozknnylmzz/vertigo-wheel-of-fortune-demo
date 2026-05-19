using System;
using UnityEngine;
using Vertigo.WheelOfFortune.Rewards.Data;

namespace Vertigo.WheelOfFortune.SpinCenter.Data
{
    [Serializable]
    public sealed class SpinCenterSliceVisualData
    {
        public WheelRewardType rewardType = WheelRewardType.Cash;
        public Sprite rewardIcon;
        [NonSerialized] public string selectedRewardAmountValue;
    }
}
