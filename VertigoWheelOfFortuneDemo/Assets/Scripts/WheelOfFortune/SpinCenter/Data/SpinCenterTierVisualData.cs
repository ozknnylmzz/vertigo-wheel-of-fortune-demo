using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.WheelOfFortune.Rewards.Data;

namespace Vertigo.WheelOfFortune.SpinCenter.Data
{
    [Serializable]
    public sealed class SpinCenterTierVisualData
    {
        [Header("Header Texts")]
        public string titleValue = "SILVER SPIN";

        [Header("Header Colors")]
        public Color titleColor = Color.white;
        public Color rewardInfoColor = Color.white;

        [Header("Wheel Sprites")]
        public Sprite wheelBaseSprite;
        public Sprite wheelIndicatorSprite;

        [Header("Reward Rules")]
        public List<SpinCenterRewardTypeRule> rewardRules = new List<SpinCenterRewardTypeRule>();

        [NonSerialized] public string rewardInfoAmountValue;
        [NonSerialized] public List<SpinCenterSliceVisualData> slices = new List<SpinCenterSliceVisualData>(15);
    }

    [Serializable]
    public sealed class SpinCenterRewardTypeRule
    {
        public WheelRewardType rewardType = WheelRewardType.Cash;
        [Min(0)] public int minCount;
        [Min(0)] public int maxCount = 1;

        public void Normalize()
        {
            minCount = Mathf.Max(0, minCount);
            maxCount = Mathf.Max(minCount, maxCount);
        }
    }
}
