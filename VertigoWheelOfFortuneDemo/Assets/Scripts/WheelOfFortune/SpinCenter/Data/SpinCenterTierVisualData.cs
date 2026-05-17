using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelOfFortune.SpinCenter.Data
{
    [Serializable]
    public sealed class SpinCenterTierVisualData
    {
        [Header("Header Texts")]
        public string titleValue = "SILVER SPIN";
        public string subtitleValue = "Up To x10 Rewards";

        [Header("Wheel Sprites")]
        public Sprite wheelBaseSprite;
        public Sprite wheelIndicatorSprite;

        [Header("Slices")]
        public List<SpinCenterSliceVisualData> slices = new List<SpinCenterSliceVisualData>(8);
    }
}
