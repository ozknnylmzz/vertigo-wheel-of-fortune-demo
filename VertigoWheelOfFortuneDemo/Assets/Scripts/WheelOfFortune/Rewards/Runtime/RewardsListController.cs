using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Rewards.Data;
using Vertigo.WheelOfFortune.Rewards.UI;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public sealed class RewardsListController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private RectTransform ui_container_rewards_list;
        [SerializeField] private RewardListItemView rewardItemPrefab;
        [SerializeField] private RectTransform rewardAnimationStartPoint;
        [SerializeField] private RewardFlyGroupController rewardFlyGroupController;
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
            WheelGameEventBus.RewardsResetRequested += ResetRewards;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.RewardWon -= HandleRewardWon;
            WheelGameEventBus.RewardsResetRequested -= ResetRewards;
        }
        #endregion

        #region Event Handlers
        private void HandleRewardWon(WheelRewardData rewardData)
        {
            if (ui_container_rewards_list == null || rewardItemPrefab == null)
            {
                CompleteRewardCollection();
                return;
            }

            if (rewardData.rewardType == WheelRewardType.None || rewardData.rewardType == WheelRewardType.Bomb)
            {
                CompleteRewardCollection();
                return;
            }

            int rewardAmount = ParseRewardAmount(rewardData.rewardAmountValue);
            RewardItemState itemState = PrepareRewardItem(rewardData);
            if (rewardFlyGroupController != null && rewardAnimationStartPoint != null)
            {
                rewardFlyGroupController.Play(
                    rewardData.rewardIcon,
                    rewardAnimationStartPoint,
                    itemState.ItemView.RectTransform,
                    rewardAmount,
                    amountStep => AddRewardAmount(itemState, amountStep),
                    CompleteRewardCollection);
                return;
            }

            AddRewardAmount(itemState, rewardAmount);
            CompleteRewardCollection();
        }
        #endregion

        #region Reward Helpers
        private RewardItemState PrepareRewardItem(WheelRewardData rewardData)
        {
            string rewardKey = ResolveRewardKey(rewardData);
            string amountPrefix = ResolveRewardAmountPrefix(rewardData.rewardAmountValue);
            if (!rewardItems.TryGetValue(rewardKey, out RewardItemState itemState))
            {
                itemState = new RewardItemState(Instantiate(rewardItemPrefab, ui_container_rewards_list), rewardData.rewardType);
                itemState.AmountPrefix = amountPrefix;
                itemState.ItemView.Apply(rewardData.rewardIcon, FormatRewardAmount(0, itemState.AmountPrefix));
                itemState.ItemView.transform.SetSiblingIndex(ResolveSiblingIndex(rewardData.rewardType));
                rewardItems.Add(rewardKey, itemState);
            }
            else
            {
                itemState.AmountPrefix = amountPrefix;
                itemState.ItemView.SetIcon(rewardData.rewardIcon);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui_container_rewards_list);
            Canvas.ForceUpdateCanvases();

            return itemState;
        }

        private void AddRewardAmount(RewardItemState itemState, int amount)
        {
            if (itemState == null || amount <= 0)
            {
                return;
            }

            itemState.AmountValue += amount;
            itemState.ItemView.SetAmount(FormatRewardAmount(itemState.AmountValue, itemState.AmountPrefix));
        }

        private static string ResolveRewardKey(WheelRewardData rewardData)
        {
            string rewardTypeKey = ((int)rewardData.rewardType).ToString(CultureInfo.InvariantCulture);
            return rewardData.rewardIcon != null
                ? rewardTypeKey + "_" + rewardData.rewardIcon.GetInstanceID().ToString(CultureInfo.InvariantCulture)
                : rewardTypeKey;
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

        private int ResolveSiblingIndex(WheelRewardType rewardType)
        {
            int rewardOrder = ResolveRewardOrder(rewardType);
            int siblingIndex = 0;
            foreach (RewardItemState itemState in rewardItems.Values)
            {
                if (ResolveRewardOrder(itemState.RewardType) <= rewardOrder)
                {
                    siblingIndex++;
                }
            }

            return siblingIndex;
        }

        private static int ResolveRewardOrder(WheelRewardType rewardType)
        {
            switch (rewardType)
            {
                case WheelRewardType.Gold:
                    return 0;
                case WheelRewardType.Cash:
                    return 1;
                case WheelRewardType.Points:
                    return 2;
                case WheelRewardType.Cards:
                    return 3;
                default:
                    return 99;
            }
        }

        private static void CompleteRewardCollection()
        {
            WheelGameFlowManager.Instance?.CompleteRewardCollection();
        }

        private void ResetRewards()
        {
            foreach (RewardItemState itemState in rewardItems.Values)
            {
                if (itemState.ItemView != null)
                {
                    Destroy(itemState.ItemView.gameObject);
                }
            }

            rewardItems.Clear();
        }
        #endregion

        #region Nested Types
        private sealed class RewardItemState
        {
            public RewardItemState(RewardListItemView itemView, WheelRewardType rewardType)
            {
                ItemView = itemView;
                RewardType = rewardType;
            }

            public RewardListItemView ItemView { get; }
            public WheelRewardType RewardType { get; }
            public int AmountValue { get; set; }
            public string AmountPrefix { get; set; } = string.Empty;
        }
        #endregion
    }
}
