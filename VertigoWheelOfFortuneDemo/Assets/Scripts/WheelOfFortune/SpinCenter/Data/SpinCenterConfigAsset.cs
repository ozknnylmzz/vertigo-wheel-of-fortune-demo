using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vertigo.WheelOfFortune.SpinCenter.Data
{
    [CreateAssetMenu(
        fileName = "spin_center_config",
        menuName = "Vertigo/Wheel Of Fortune/Spin Center Config")]
    public sealed class SpinCenterConfigAsset : ScriptableObject
    {
        public const int MaxLevel = 60;

        [SerializeField] private SpinCenterTierVisualData bronze = new SpinCenterTierVisualData();
        [SerializeField] private SpinCenterTierVisualData silver = new SpinCenterTierVisualData();
        [SerializeField] private SpinCenterTierVisualData golden = new SpinCenterTierVisualData();

        private void OnValidate()
        {
            EnsureTierContainers();
        }

        public SpinCenterTierVisualData ResolveByLevelOrThrow(int level)
        {
            return GetTierVisualDataOrThrow(ResolveTierFromLevel(level));
        }

        public SpinCenterTierVisualData GetTierVisualDataOrThrow(SpinCenterTier tier)
        {
            SpinCenterTierVisualData visualData = null;

            switch (tier)
            {
                case SpinCenterTier.Bronze:
                    visualData = bronze;
                    break;
                case SpinCenterTier.Silver:
                    visualData = silver;
                    break;
                case SpinCenterTier.Golden:
                    visualData = golden;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tier), tier, null);
            }

            if (visualData != null)
            {
                return visualData;
            }

            throw new InvalidOperationException(
                $"No spin center tier data found for {tier} in {name}.");
        }

        private static SpinCenterTier ResolveTierFromLevel(int level)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, MaxLevel);

            if (normalizedLevel % 30 == 0)
            {
                return SpinCenterTier.Golden;
            }

            if (normalizedLevel % 5 == 0)
            {
                return SpinCenterTier.Silver;
            }

            return SpinCenterTier.Bronze;
        }

        [ContextMenu("Copy Silver Data To Golden")]
        private void CopySilverDataToGolden()
        {
            EnsureTierContainers();
            CopyTierData(silver, golden);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        private void EnsureTierContainers()
        {
            if (bronze == null)
            {
                bronze = new SpinCenterTierVisualData();
            }

            if (silver == null)
            {
                silver = new SpinCenterTierVisualData();
            }

            if (golden == null)
            {
                golden = new SpinCenterTierVisualData();
            }
        }

        private static void CopyTierData(SpinCenterTierVisualData source, SpinCenterTierVisualData target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.titleValue = source.titleValue;
            target.rewardInfoAmountValue = source.rewardInfoAmountValue;
            target.titleColor = source.titleColor;
            target.rewardInfoColor = source.rewardInfoColor;
            target.wheelBaseSprite = source.wheelBaseSprite;
            target.wheelIndicatorSprite = source.wheelIndicatorSprite;

            if (target.slices == null)
            {
                target.slices = new List<SpinCenterSliceVisualData>();
            }
            else
            {
                target.slices.Clear();
            }

            if (source.slices == null)
            {
                return;
            }

            for (int i = 0; i < source.slices.Count; i++)
            {
                SpinCenterSliceVisualData sourceSlice = source.slices[i];
                if (sourceSlice == null)
                {
                    target.slices.Add(new SpinCenterSliceVisualData());
                    continue;
                }

                target.slices.Add(new SpinCenterSliceVisualData
                {
                    sliceId = sourceSlice.sliceId,
                    rewardIcon = sourceSlice.rewardIcon,
                    rewardAmountValue = sourceSlice.rewardAmountValue
                });
            }
        }
    }
}
