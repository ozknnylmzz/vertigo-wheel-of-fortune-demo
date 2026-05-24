using System.Collections;
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
        [SerializeField] [Min(0.5f)] private float rewardCompletionFallbackSeconds = 3f;
        #endregion

        #region Runtime State
        private readonly Dictionary<string, RewardItemState> rewardItems = new Dictionary<string, RewardItemState>();
        private Coroutine rewardCompletionFallbackCoroutine;
        private int rewardCompletionToken;
        private bool rewardCompletionPending;
        private float spinFastForwardMultiplier = 1f;
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
            WheelGameEventBus.SpinFastForwardMultiplierChanged += HandleSpinFastForwardMultiplierChanged;
            WheelGameEventBus.RewardsResetRequested += ResetRewards;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.RewardWon -= HandleRewardWon;
            WheelGameEventBus.SpinFastForwardMultiplierChanged -= HandleSpinFastForwardMultiplierChanged;
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

            int rewardAmount = WheelRewardAmountValueUtility.ParseAmount(rewardData.rewardAmountValue);
            RewardItemState itemState = PrepareRewardItem(rewardData);
            if (CanPlayRewardFlyAnimation())
            {
                int completionToken = BeginRewardCompletionWait();
                rewardFlyGroupController.Play(
                    rewardData.rewardIcon,
                    rewardAnimationStartPoint,
                    itemState.ItemView.RectTransform,
                    rewardAmount,
                    amountStep => AddRewardAmount(itemState, amountStep),
                    ResolveRewardFlyTimeScale(),
                    () => CompleteRewardCollectionIfPending(completionToken));
                return;
            }

            AddRewardAmount(itemState, rewardAmount);
            CompleteRewardCollection();
        }

        private void HandleSpinFastForwardMultiplierChanged(float multiplier)
        {
            spinFastForwardMultiplier = Mathf.Max(1f, multiplier);
        }
        #endregion

        #region Reward Helpers
        private RewardItemState PrepareRewardItem(WheelRewardData rewardData)
        {
            string rewardKey = ResolveRewardKey(rewardData);
            string amountPrefix = WheelRewardAmountValueUtility.ResolveAmountPrefix(rewardData.rewardAmountValue);
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

        private bool CanPlayRewardFlyAnimation()
        {
            return rewardFlyGroupController != null
                   && rewardFlyGroupController.isActiveAndEnabled
                   && rewardAnimationStartPoint != null;
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

        private static string FormatRewardAmount(int amount, string amountPrefix)
        {
            return WheelRewardAmountValueUtility.FormatAmount(amount, amountPrefix);
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

        private int BeginRewardCompletionWait()
        {
            rewardCompletionToken++;
            rewardCompletionPending = true;

            if (rewardCompletionFallbackCoroutine != null)
            {
                StopCoroutine(rewardCompletionFallbackCoroutine);
            }

            rewardCompletionFallbackCoroutine = StartCoroutine(CompleteRewardCollectionFallback(rewardCompletionToken));
            return rewardCompletionToken;
        }

        private float ResolveRewardFlyTimeScale()
        {
            return spinFastForwardMultiplier;
        }

        private IEnumerator CompleteRewardCollectionFallback(int completionToken)
        {
            yield return new WaitForSeconds(Mathf.Max(0.5f, rewardCompletionFallbackSeconds));
            CompleteRewardCollectionIfPending(completionToken);
        }

        private void CompleteRewardCollectionIfPending(int completionToken)
        {
            if (!rewardCompletionPending || completionToken != rewardCompletionToken)
            {
                return;
            }

            rewardCompletionPending = false;
            if (rewardCompletionFallbackCoroutine != null)
            {
                StopCoroutine(rewardCompletionFallbackCoroutine);
                rewardCompletionFallbackCoroutine = null;
            }

            CompleteRewardCollection();
        }

        private void ResetRewards()
        {
            spinFastForwardMultiplier = 1f;
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
