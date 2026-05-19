using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Rewards.UI;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public sealed class RewardsListController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private RectTransform ui_container_rewards_list;
        [SerializeField] private RewardListItemView rewardItemPrefab;
        #endregion

        #region Runtime State
        private readonly Dictionary<string, RewardItemState> rewardItems = new Dictionary<string, RewardItemState>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (ui_container_rewards_list == null)
            {
                ui_container_rewards_list = transform as RectTransform;
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.RewardWon += HandleRewardWon;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.RewardWon -= HandleRewardWon;
        }
        #endregion

        #region Event Handlers
        private void HandleRewardWon(WheelRewardData rewardData)
        {
            if (ui_container_rewards_list == null || rewardItemPrefab == null)
            {
                return;
            }

            string rewardKey = ResolveRewardKey(rewardData);
            if (!rewardItems.TryGetValue(rewardKey, out RewardItemState itemState))
            {
                itemState = new RewardItemState(Instantiate(rewardItemPrefab, ui_container_rewards_list));
                rewardItems.Add(rewardKey, itemState);
            }

            itemState.AmountValue += ParseRewardAmount(rewardData.rewardAmountValue);
            itemState.AmountPrefix = ResolveRewardAmountPrefix(rewardData.rewardAmountValue);
            itemState.ItemView.Apply(rewardData.rewardIcon, FormatRewardAmount(itemState.AmountValue, itemState.AmountPrefix));
        }
        #endregion

        #region Reward Helpers
        private static string ResolveRewardKey(WheelRewardData rewardData)
        {
            if (!string.IsNullOrWhiteSpace(rewardData.rewardKey))
            {
                return rewardData.rewardKey;
            }

            return rewardData.rewardIcon != null ? rewardData.rewardIcon.name : string.Empty;
        }

        private static int ParseRewardAmount(string rewardAmountValue)
        {
            if (string.IsNullOrWhiteSpace(rewardAmountValue))
            {
                return 0;
            }

            string digits = string.Empty;
            for (int i = 0; i < rewardAmountValue.Length; i++)
            {
                char c = rewardAmountValue[i];
                if (c >= '0' && c <= '9')
                {
                    digits += c;
                }
            }

            return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount)
                ? amount
                : 0;
        }

        private static string ResolveRewardAmountPrefix(string rewardAmountValue)
        {
            return !string.IsNullOrWhiteSpace(rewardAmountValue) && rewardAmountValue.TrimStart().StartsWith("x")
                ? "x"
                : string.Empty;
        }

        private static string FormatRewardAmount(int amount, string amountPrefix)
        {
            return amountPrefix + amount.ToString("N0", CultureInfo.InvariantCulture);
        }
        #endregion

        #region Nested Types
        private sealed class RewardItemState
        {
            public RewardItemState(RewardListItemView itemView)
            {
                ItemView = itemView;
            }

            public RewardListItemView ItemView { get; }
            public int AmountValue { get; set; }
            public string AmountPrefix { get; set; } = string.Empty;
        }
        #endregion
    }
}
