using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Vertigo.WheelOfFortune.SpinCenter.Data
{
    [Serializable]
    public sealed class SpinCenterTierVisualData
    {
        [Header("Header Texts")]
        public string titleValue = "SILVER SPIN";
        [FormerlySerializedAs("subtitleValue")]
        [FormerlySerializedAs("rewardInfoValue")]
        public string rewardInfoAmountValue = "x10";
        
        [Header("Header Colors")]
        public Color titleColor = Color.white;
        public Color rewardInfoColor = Color.white;

        [Header("Wheel Sprites")]
        public Sprite wheelBaseSprite;
        public Sprite wheelIndicatorSprite;

        [Header("Slices")]
        public List<SpinCenterSliceVisualData> slices = new List<SpinCenterSliceVisualData>(8);
    }
}
