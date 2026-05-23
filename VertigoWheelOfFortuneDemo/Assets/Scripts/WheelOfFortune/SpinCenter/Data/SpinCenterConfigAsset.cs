using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.WheelOfFortune.Rewards.Data;
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

        [Header("Reward Pool")]
        [SerializeField] [Min(1)] private int sliceCount = 15;
        [SerializeField] private WheelRewardPoolAsset rewardPool;

        [Header("Tier Visuals")]
        [SerializeField] private SpinCenterTierVisualData bronze = new SpinCenterTierVisualData();
        [SerializeField] private SpinCenterTierVisualData silver = new SpinCenterTierVisualData();
        [SerializeField] private SpinCenterTierVisualData golden = new SpinCenterTierVisualData();

        private void OnValidate()
        {
            EnsureTierContainers();
        }

        public SpinCenterTierVisualData ResolveByLevelOrThrow(int level)
        {
            EnsureTierContainers();

            SpinCenterTier tier = ResolveTierFromLevel(level);
            SpinCenterTierVisualData visualData = GetTierVisualDataOrThrow(tier);
            ApplyRuntimeInfo(visualData, tier);
            return visualData;
        }

        public SpinCenterTierVisualData GenerateSlicesForLevel(int level)
        {
            return GenerateSlicesForLevel(level, sliceCount);
        }

        public SpinCenterTierVisualData GenerateSlicesForLevel(int level, int targetSliceCount)
        {
            EnsureTierContainers();

            SpinCenterTier tier = ResolveTierFromLevel(level);
            SpinCenterTierVisualData visualData = GetTierVisualDataOrThrow(tier);
            ApplyRuntimeInfo(visualData, tier);
            BuildSlices(visualData, targetSliceCount > 0 ? targetSliceCount : sliceCount);
            return visualData;
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

            if (normalizedLevel == 1)
            {
                return SpinCenterTier.Silver;
            }

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
            sliceCount = Mathf.Max(1, sliceCount);

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

            EnsureDefaultRules(bronze, SpinCenterTier.Bronze);
            EnsureDefaultRules(silver, SpinCenterTier.Silver);
            EnsureDefaultRules(golden, SpinCenterTier.Golden);
            NormalizeRules(bronze);
            NormalizeRules(silver);
            NormalizeRules(golden);
        }

        private static void CopyTierData(SpinCenterTierVisualData source, SpinCenterTierVisualData target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.titleValue = source.titleValue;
            target.titleColor = source.titleColor;
            target.rewardInfoColor = source.rewardInfoColor;
            target.wheelBaseSprite = source.wheelBaseSprite;
            target.wheelIndicatorSprite = source.wheelIndicatorSprite;

            if (target.rewardRules == null)
            {
                target.rewardRules = new List<SpinCenterRewardTypeRule>();
            }
            else
            {
                target.rewardRules.Clear();
            }

            if (source.rewardRules == null)
            {
                return;
            }

            for (int i = 0; i < source.rewardRules.Count; i++)
            {
                SpinCenterRewardTypeRule sourceRule = source.rewardRules[i];
                if (sourceRule == null)
                {
                    continue;
                }

                target.rewardRules.Add(new SpinCenterRewardTypeRule
                {
                    rewardType = sourceRule.rewardType,
                    minCount = sourceRule.minCount,
                    maxCount = sourceRule.maxCount
                });
            }
        }

        private void BuildSlices(SpinCenterTierVisualData visualData, int targetSliceCount)
        {
            if (visualData.slices == null)
            {
                visualData.slices = new List<SpinCenterSliceVisualData>(targetSliceCount);
            }
            else
            {
                visualData.slices.Clear();
            }

            if (rewardPool == null || rewardPool.Count == 0 || visualData.rewardRules == null)
            {
                return;
            }

            int[] rewardCounts = ResolveRewardCounts(visualData.rewardRules, Mathf.Max(1, targetSliceCount));
            for (int i = 0; i < visualData.rewardRules.Count; i++)
            {
                SpinCenterRewardTypeRule rule = visualData.rewardRules[i];
                if (rule == null)
                {
                    continue;
                }

                rule.Normalize();
                for (int count = 0; count < rewardCounts[i]; count++)
                {
                    AddRandomRewardByType(visualData.slices, rule.rewardType);
                }
            }

            Shuffle(visualData.slices);
        }

        private int[] ResolveRewardCounts(List<SpinCenterRewardTypeRule> rules, int targetSliceCount)
        {
            int[] counts = new int[rules.Count];
            int minTotal = 0;
            int maxTotal = 0;

            for (int i = 0; i < rules.Count; i++)
            {
                SpinCenterRewardTypeRule rule = rules[i];
                if (rule == null || !HasRewardByType(rule.rewardType))
                {
                    continue;
                }

                rule.Normalize();
                counts[i] = rule.minCount;
                minTotal += rule.minCount;
                maxTotal += rule.maxCount;
            }

            int remaining = Mathf.Clamp(targetSliceCount, minTotal, maxTotal) - minTotal;
            while (remaining > 0)
            {
                int expandableCount = 0;
                for (int i = 0; i < rules.Count; i++)
                {
                    SpinCenterRewardTypeRule rule = rules[i];
                    if (rule != null && counts[i] < rule.maxCount && HasRewardByType(rule.rewardType))
                    {
                        expandableCount++;
                    }
                }

                if (expandableCount == 0)
                {
                    break;
                }

                int targetIndex = UnityEngine.Random.Range(0, expandableCount);
                for (int i = 0; i < rules.Count; i++)
                {
                    SpinCenterRewardTypeRule rule = rules[i];
                    if (rule == null || counts[i] >= rule.maxCount || !HasRewardByType(rule.rewardType))
                    {
                        continue;
                    }

                    if (targetIndex == 0)
                    {
                        counts[i]++;
                        remaining--;
                        break;
                    }

                    targetIndex--;
                }
            }

            return counts;
        }

        private bool HasRewardByType(WheelRewardType rewardType)
        {
            return rewardPool != null && rewardPool.HasRewardByType(rewardType);
        }

        private bool AddRandomRewardByType(List<SpinCenterSliceVisualData> target, WheelRewardType rewardType)
        {
            if (rewardPool == null || !rewardPool.TryGetRandomRewardByType(rewardType, out WheelRewardPoolEntry rewardData))
            {
                return false;
            }

            target.Add(new SpinCenterSliceVisualData
            {
                rewardType = rewardData.RewardType,
                rewardIcon = rewardData.RewardIcon
            });

            return true;
        }

        private static void Shuffle(List<SpinCenterSliceVisualData> slices)
        {
            for (int i = slices.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                SpinCenterSliceVisualData temp = slices[i];
                slices[i] = slices[swapIndex];
                slices[swapIndex] = temp;
            }
        }

        private static void ApplyRuntimeInfo(SpinCenterTierVisualData visualData, SpinCenterTier tier)
        {
            visualData.rewardInfoAmountValue = tier == SpinCenterTier.Bronze ? "x3" : "x10";
        }

        private static void NormalizeRules(SpinCenterTierVisualData visualData)
        {
            if (visualData.rewardRules == null)
            {
                visualData.rewardRules = new List<SpinCenterRewardTypeRule>();
                return;
            }

            for (int i = 0; i < visualData.rewardRules.Count; i++)
            {
                visualData.rewardRules[i]?.Normalize();
            }
        }

        private static void EnsureDefaultRules(SpinCenterTierVisualData visualData, SpinCenterTier tier)
        {
            if (visualData.rewardRules == null)
            {
                visualData.rewardRules = new List<SpinCenterRewardTypeRule>();
            }

            if (visualData.rewardRules.Count > 0)
            {
                return;
            }

            switch (tier)
            {
                case SpinCenterTier.Bronze:
                    AddRule(visualData, WheelRewardType.Points, 2, 6);
                    AddRule(visualData, WheelRewardType.Cash, 1, 2);
                    AddRule(visualData, WheelRewardType.Gold, 1, 2);
                    AddRule(visualData, WheelRewardType.Cards, 0, 2);
                    AddRule(visualData, WheelRewardType.Bomb, 0, 2);
                    break;
                case SpinCenterTier.Silver:
                    AddRule(visualData, WheelRewardType.Cards, 3, 5);
                    AddRule(visualData, WheelRewardType.Gold, 1, 2);
                    AddRule(visualData, WheelRewardType.Cash, 1, 2);
                    break;
                case SpinCenterTier.Golden:
                    AddRule(visualData, WheelRewardType.Gold, 1, 2);
                    AddRule(visualData, WheelRewardType.Cash, 1, 2);
                    AddRule(visualData, WheelRewardType.Cards, 2, 6);
                    AddRule(visualData, WheelRewardType.Points, 0, 2);
                    break;
            }
        }

        private static void AddRule(SpinCenterTierVisualData visualData, WheelRewardType rewardType, int minCount, int maxCount)
        {
            visualData.rewardRules.Add(new SpinCenterRewardTypeRule
            {
                rewardType = rewardType,
                minCount = minCount,
                maxCount = maxCount
            });
        }
    }
}
