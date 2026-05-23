using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelOfFortune.Rewards.Data
{
    [CreateAssetMenu(
        fileName = "wheel_reward_pool",
        menuName = "Vertigo/Wheel Of Fortune/Reward Pool")]
    public sealed class WheelRewardPoolAsset : ScriptableObject
    {
        #region Inspector Fields
        [SerializeField] private List<WheelRewardPoolEntry> rewards = new List<WheelRewardPoolEntry>();
        #endregion

        #region Properties
        public int Count => rewards != null ? rewards.Count : 0;
        #endregion

        #region Public API
        public bool HasRewardByType(WheelRewardType rewardType)
        {
            if (rewards == null)
            {
                return false;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                WheelRewardPoolEntry reward = rewards[i];
                if (reward != null && reward.RewardType == rewardType)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetRandomRewardByType(WheelRewardType rewardType, out WheelRewardPoolEntry reward)
        {
            reward = null;

            if (rewards == null)
            {
                return false;
            }

            int matchCount = 0;
            for (int i = 0; i < rewards.Count; i++)
            {
                WheelRewardPoolEntry candidate = rewards[i];
                if (candidate != null && candidate.RewardType == rewardType)
                {
                    matchCount++;
                }
            }

            if (matchCount == 0)
            {
                return false;
            }

            int targetIndex = UnityEngine.Random.Range(0, matchCount);
            for (int i = 0; i < rewards.Count; i++)
            {
                WheelRewardPoolEntry candidate = rewards[i];
                if (candidate == null || candidate.RewardType != rewardType)
                {
                    continue;
                }

                if (targetIndex == 0)
                {
                    reward = candidate;
                    return true;
                }

                targetIndex--;
            }

            return false;
        }

        public Sprite ResolveIcon(WheelRewardType rewardType, Sprite selectedIcon)
        {
            WheelRewardPoolEntry exactMatch = ResolveExactReward(rewardType, selectedIcon);
            if (exactMatch != null)
            {
                return exactMatch.RewardIcon;
            }

            if (selectedIcon != null)
            {
                return selectedIcon;
            }

            WheelRewardPoolEntry firstByType = ResolveFirstRewardByType(rewardType);
            return firstByType != null ? firstByType.RewardIcon : null;
        }

        public string ResolveName(WheelRewardType rewardType, Sprite selectedIcon, string fallbackName)
        {
            WheelRewardPoolEntry exactMatch = ResolveExactReward(rewardType, selectedIcon);
            if (HasName(exactMatch))
            {
                return exactMatch.RewardName;
            }

            WheelRewardPoolEntry firstByType = ResolveFirstRewardByType(rewardType);
            return HasName(firstByType) ? firstByType.RewardName : fallbackName;
        }

        public int ResolveProgressRequiredAmount(WheelRewardType rewardType, Sprite selectedIcon, int fallbackAmount)
        {
            int safeFallbackAmount = Mathf.Max(1, fallbackAmount);
            WheelRewardPoolEntry exactMatch = ResolveExactReward(rewardType, selectedIcon);
            if (exactMatch != null)
            {
                return exactMatch.ProgressRequiredAmount;
            }

            WheelRewardPoolEntry firstByType = ResolveFirstRewardByType(rewardType);
            return firstByType != null ? firstByType.ProgressRequiredAmount : safeFallbackAmount;
        }
        #endregion

        #region Private Methods
        private static bool HasName(WheelRewardPoolEntry reward)
        {
            return reward != null && !string.IsNullOrWhiteSpace(reward.RewardName);
        }

        private WheelRewardPoolEntry ResolveExactReward(WheelRewardType rewardType, Sprite selectedIcon)
        {
            if (selectedIcon == null || rewards == null)
            {
                return null;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                WheelRewardPoolEntry reward = rewards[i];
                if (reward != null && reward.RewardType == rewardType && reward.RewardIcon == selectedIcon)
                {
                    return reward;
                }
            }

            return null;
        }

        private WheelRewardPoolEntry ResolveFirstRewardByType(WheelRewardType rewardType)
        {
            if (rewards == null)
            {
                return null;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                WheelRewardPoolEntry reward = rewards[i];
                if (reward != null && reward.RewardType == rewardType)
                {
                    return reward;
                }
            }

            return null;
        }
        #endregion
    }

    [Serializable]
    public sealed class WheelRewardPoolEntry
    {
        #region Inspector Fields
        [SerializeField] private WheelRewardType rewardType = WheelRewardType.Cash;
        [SerializeField] private string rewardName;
        [SerializeField] private Sprite rewardIcon;
        [SerializeField] [Min(1)] private int progressRequiredAmount = 90;
        #endregion

        #region Properties
        public WheelRewardType RewardType => rewardType;
        public string RewardName => rewardName;
        public Sprite RewardIcon => rewardIcon;
        public int ProgressRequiredAmount => Mathf.Max(1, progressRequiredAmount);
        #endregion
    }
}
