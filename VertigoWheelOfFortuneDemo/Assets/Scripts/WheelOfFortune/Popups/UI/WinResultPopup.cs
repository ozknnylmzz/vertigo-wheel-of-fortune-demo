using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.Popups.Runtime;
using Vertigo.WheelOfFortune.Rewards.Data;
using Vertigo.WheelOfFortune.Rewards.Runtime;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class WinResultPopup : WheelPopupBase
    {
        #region Constants
        private const int MaxVisibleRewardCards = 4;
        private const float DefaultViewportInset = 36f;
        private const int DefaultCardProgressRequiredAmount = 90;
        #endregion

        #region Inspector Fields
        [Header("Reward Cards")]
        [SerializeField] private WheelRewardPoolAsset rewardPool;
        [SerializeField] private WinResultRewardCardView rewardCardTemplate;

        [Header("UI References")]
        [SerializeField] private TMP_Text ui_text_win_result_title_value;
        [SerializeField] private ScrollRect ui_scroll_win_result_rewards;
        [SerializeField] private RectTransform ui_container_win_result_rewards_content;
        [SerializeField] private Button ui_button_win_result_continue;
        [SerializeField] private TMP_Text ui_text_button_win_result_continue_value;
        #endregion

        #region Runtime State
        private readonly List<WinResultRewardCardView> spawnedRewardCards = new List<WinResultRewardCardView>();
        #endregion

        #region Properties
        public override WheelPopupType PopupType => WheelPopupType.WinResult;
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            SubscribeButtons();
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            AssignReferences();
        }
#endif
        #endregion

        #region Public API
        public override void Show()
        {
            base.Show();
            ResetRewardScrollPosition();
            StartCoroutine(ResetRewardScrollPositionNextFrame());
        }

        public void Setup(IReadOnlyList<WheelRewardData> rewards)
        {
            AssignReferences();
            ClearRewardCards();
            HideTemplateRewardCards();

            if (ui_container_win_result_rewards_content == null || rewardCardTemplate == null || rewards == null)
            {
                return;
            }

            List<WinResultRewardCardState> rewardStates = BuildRewardStates(rewards);
            ApplyPersistentCardProgress(rewardStates);
            rewardStates.Sort(CompareRewardStates);

            for (int i = 0; i < rewardStates.Count; i++)
            {
                WinResultRewardCardView cardView = Instantiate(rewardCardTemplate, ui_container_win_result_rewards_content);
                cardView.name = "ui_item_win_result_reward_card_" + i;
                cardView.gameObject.SetActive(true);
                ApplyRewardState(cardView, rewardStates[i]);
                spawnedRewardCards.Add(cardView);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui_container_win_result_rewards_content);
            ApplyRewardScrollLayout(rewardStates.Count);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui_container_win_result_rewards_content);

            if (ui_scroll_win_result_rewards != null)
            {
                ResetRewardScrollPosition();
            }
        }
        #endregion

        #region Private Methods
        private void SubscribeButtons()
        {
            AssignReferences();

            if (ui_button_win_result_continue != null)
            {
                ui_button_win_result_continue.onClick.RemoveListener(HandleContinueButtonClicked);
                ui_button_win_result_continue.onClick.AddListener(HandleContinueButtonClicked);
            }
        }

        private void UnsubscribeButtons()
        {
            if (ui_button_win_result_continue != null)
            {
                ui_button_win_result_continue.onClick.RemoveListener(HandleContinueButtonClicked);
            }
        }

        private void HandleContinueButtonClicked()
        {
            RequestClose();
        }

        private void AssignReferences()
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

            ui_text_win_result_title_value = ui_text_win_result_title_value != null
                ? ui_text_win_result_title_value
                : Array.Find(texts, text => text.name == "ui_text_win_result_title_value");

            ui_text_button_win_result_continue_value = ui_text_button_win_result_continue_value != null
                ? ui_text_button_win_result_continue_value
                : Array.Find(texts, text => text.name == "ui_text_button_win_result_continue_value");

            ui_scroll_win_result_rewards = ui_scroll_win_result_rewards != null
                ? ui_scroll_win_result_rewards
                : Array.Find(GetComponentsInChildren<ScrollRect>(true), scroll => scroll.name == "ui_scroll_win_result_rewards");

            ui_container_win_result_rewards_content = ui_container_win_result_rewards_content != null
                ? ui_container_win_result_rewards_content
                : Array.Find(GetComponentsInChildren<RectTransform>(true), rectTransform => rectTransform.name == "ui_container_win_result_rewards_content");

            ui_button_win_result_continue = ui_button_win_result_continue != null
                ? ui_button_win_result_continue
                : Array.Find(GetComponentsInChildren<Button>(true), button => button.name == "ui_button_win_result_continue");

            rewardCardTemplate = rewardCardTemplate != null
                ? rewardCardTemplate
                : ResolveRewardCardTemplate();
        }

        private WinResultRewardCardView ResolveRewardCardTemplate()
        {
            WinResultRewardCardView[] rewardCards = GetComponentsInChildren<WinResultRewardCardView>(true);
            return rewardCards.Length > 0 ? rewardCards[0] : null;
        }

        private List<WinResultRewardCardState> BuildRewardStates(IReadOnlyList<WheelRewardData> rewards)
        {
            Dictionary<string, WinResultRewardCardState> statesByKey = new Dictionary<string, WinResultRewardCardState>();
            List<WinResultRewardCardState> states = new List<WinResultRewardCardState>();

            for (int i = 0; i < rewards.Count; i++)
            {
                WheelRewardData rewardData = rewards[i];
                if (rewardData.rewardType == WheelRewardType.None || rewardData.rewardType == WheelRewardType.Bomb)
                {
                    continue;
                }

                int amount = WheelRewardAmountValueUtility.ParseAmount(rewardData.rewardAmountValue);
                if (amount <= 0)
                {
                    continue;
                }

                Sprite rewardIcon = ResolveRewardIcon(rewardData);
                string rewardKey = ResolveRewardKey(rewardData, rewardIcon);

                if (!statesByKey.TryGetValue(rewardKey, out WinResultRewardCardState state))
                {
                    state = CreateRewardState(rewardData, rewardIcon, i);
                    statesByKey.Add(rewardKey, state);
                    states.Add(state);
                }

                state.AmountPrefix = WheelRewardAmountValueUtility.ResolveAmountPrefix(rewardData.rewardAmountValue);
                if (state.IsProgress)
                {
                    state.AddProgress(amount);
                }
                else
                {
                    state.AddAmount(amount);
                }
            }

            return states;
        }

        private WinResultRewardCardState CreateRewardState(WheelRewardData rewardData, Sprite rewardIcon, int insertionIndex)
        {
            bool isProgress = ResolveIsProgressReward(rewardData.rewardType);

            return new WinResultRewardCardState(
                ResolveRewardTitle(rewardData, rewardIcon),
                isProgress,
                ResolveRewardProgressKey(rewardData, rewardIcon),
                ResolveProgressRequiredAmount(rewardData, rewardIcon),
                ResolveDisplayOrder(rewardData.rewardType),
                insertionIndex,
                rewardIcon);
        }

        private void ApplyRewardState(WinResultRewardCardView cardView, WinResultRewardCardState state)
        {
            cardView.SetTitle(state.Title);
            cardView.SetRewardIcon(state.RewardIcon);
            cardView.SetProgressVisible(state.IsProgress);

            if (state.IsProgress)
            {
                cardView.SetProgressIcons(state.RewardIcon, state.RewardIcon);
                cardView.PlayProgressTween(
                    state.StartCompletedCount,
                    state.StartProgressValue,
                    state.CompletedCount,
                    state.ProgressValue,
                    state.ProgressRequiredAmount);
                return;
            }

            cardView.SetAmount(WheelRewardAmountValueUtility.FormatAmount(state.AmountValue, state.AmountPrefix));
        }

        private void ClearRewardCards()
        {
            for (int i = 0; i < spawnedRewardCards.Count; i++)
            {
                WinResultRewardCardView rewardCard = spawnedRewardCards[i];
                if (rewardCard == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(rewardCard.gameObject);
                }
                else
                {
                    DestroyImmediate(rewardCard.gameObject);
                }
            }

            spawnedRewardCards.Clear();
        }

        private static void ApplyPersistentCardProgress(IReadOnlyList<WinResultRewardCardState> rewardStates)
        {
            if (!Application.isPlaying || rewardStates == null)
            {
                return;
            }

            for (int i = 0; i < rewardStates.Count; i++)
            {
                WinResultRewardCardState state = rewardStates[i];
                if (state == null || !state.IsProgress)
                {
                    continue;
                }

                WheelCardRewardProgressChange progressChange = WheelCardRewardProgressPrefs.AddProgress(
                    state.ProgressKey,
                    state.AmountValue,
                    state.ProgressRequiredAmount);
                state.SetProgressAnimation(progressChange.Previous, progressChange.Current);
            }
        }

        private void ApplyRewardScrollLayout(int rewardCount)
        {
            if (ui_scroll_win_result_rewards == null || ui_container_win_result_rewards_content == null || rewardCardTemplate == null)
            {
                return;
            }

            RectTransform scrollRectTransform = ui_scroll_win_result_rewards.transform as RectTransform;
            RectTransform viewportRectTransform = ui_scroll_win_result_rewards.viewport;
            if (scrollRectTransform == null || viewportRectTransform == null)
            {
                return;
            }

            HorizontalLayoutGroup layoutGroup = ui_container_win_result_rewards_content.GetComponent<HorizontalLayoutGroup>();
            ApplyRewardViewportLayout(viewportRectTransform);
            ApplyRewardContentAlignment(rewardCount <= MaxVisibleRewardCards, layoutGroup);

            float cardWidth = ResolveRewardCardWidth();
            float spacing = layoutGroup != null ? layoutGroup.spacing : 0f;
            float visibleContentWidth = (MaxVisibleRewardCards * cardWidth) + ((MaxVisibleRewardCards - 1) * spacing);

            if (layoutGroup != null)
            {
                visibleContentWidth += layoutGroup.padding.left + layoutGroup.padding.right;
            }

            scrollRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, visibleContentWidth + DefaultViewportInset);
            ui_scroll_win_result_rewards.movementType = ScrollRect.MovementType.Clamped;
            ui_scroll_win_result_rewards.horizontal = rewardCount > 1;
        }

        private void ApplyRewardViewportLayout(RectTransform viewportRectTransform)
        {
            viewportRectTransform.anchorMin = Vector2.zero;
            viewportRectTransform.anchorMax = Vector2.one;
            viewportRectTransform.pivot = new Vector2(0.5f, 0.5f);
            viewportRectTransform.anchoredPosition = Vector2.zero;
            viewportRectTransform.sizeDelta = new Vector2(-DefaultViewportInset, -DefaultViewportInset);
        }

        private void ApplyRewardContentAlignment(bool centerRewards, HorizontalLayoutGroup layoutGroup)
        {
            if (layoutGroup != null)
            {
                layoutGroup.childAlignment = centerRewards ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            }

            ContentSizeFitter contentSizeFitter = ui_container_win_result_rewards_content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.horizontalFit = centerRewards
                    ? ContentSizeFitter.FitMode.Unconstrained
                    : ContentSizeFitter.FitMode.PreferredSize;
            }

            if (centerRewards)
            {
                ui_container_win_result_rewards_content.anchorMin = new Vector2(0f, 0f);
                ui_container_win_result_rewards_content.anchorMax = new Vector2(1f, 1f);
                ui_container_win_result_rewards_content.pivot = new Vector2(0.5f, 0.5f);
                ui_container_win_result_rewards_content.anchoredPosition = new Vector2(0f, ui_container_win_result_rewards_content.anchoredPosition.y);
                ui_container_win_result_rewards_content.sizeDelta = new Vector2(0f, ui_container_win_result_rewards_content.sizeDelta.y);
                return;
            }

            ui_container_win_result_rewards_content.anchorMin = new Vector2(0f, 0f);
            ui_container_win_result_rewards_content.anchorMax = new Vector2(0f, 1f);
            ui_container_win_result_rewards_content.pivot = new Vector2(0f, 0.5f);
        }

        private IEnumerator ResetRewardScrollPositionNextFrame()
        {
            yield return null;
            ResetRewardScrollPosition();
        }

        private void ResetRewardScrollPosition()
        {
            if (ui_scroll_win_result_rewards == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            ui_scroll_win_result_rewards.velocity = Vector2.zero;
            ui_scroll_win_result_rewards.horizontalNormalizedPosition = 0f;

            if (ui_container_win_result_rewards_content == null)
            {
                return;
            }

            Vector2 anchoredPosition = ui_container_win_result_rewards_content.anchoredPosition;
            anchoredPosition.x = 0f;
            ui_container_win_result_rewards_content.anchoredPosition = anchoredPosition;
        }

        private float ResolveRewardCardWidth()
        {
            LayoutElement layoutElement = rewardCardTemplate.GetComponent<LayoutElement>();
            if (layoutElement != null && layoutElement.preferredWidth > 0f)
            {
                return layoutElement.preferredWidth;
            }

            RectTransform rewardCardRect = rewardCardTemplate.RectTransform;
            return rewardCardRect != null ? rewardCardRect.rect.width : 0f;
        }

        private void HideTemplateRewardCards()
        {
            WinResultRewardCardView[] rewardCards = GetComponentsInChildren<WinResultRewardCardView>(true);
            for (int i = 0; i < rewardCards.Length; i++)
            {
                if (rewardCards[i] != null)
                {
                    rewardCards[i].gameObject.SetActive(false);
                }
            }
        }

        private Sprite ResolveRewardIcon(WheelRewardData rewardData)
        {
            return rewardPool != null
                ? rewardPool.ResolveIcon(rewardData.rewardType, rewardData.rewardIcon)
                : rewardData.rewardIcon;
        }

        private static string ResolveRewardKey(WheelRewardData rewardData, Sprite rewardIcon)
        {
            return rewardIcon != null
                ? rewardData.rewardType.ToString() + "_" + rewardIcon.GetInstanceID()
                : rewardData.rewardType.ToString();
        }

        private static bool ResolveIsProgressReward(WheelRewardType rewardType)
        {
            return rewardType == WheelRewardType.Cards;
        }

        private int ResolveProgressRequiredAmount(WheelRewardData rewardData, Sprite rewardIcon)
        {
            return rewardPool != null
                ? rewardPool.ResolveProgressRequiredAmount(
                    rewardData.rewardType,
                    rewardIcon,
                    DefaultCardProgressRequiredAmount)
                : DefaultCardProgressRequiredAmount;
        }

        private string ResolveRewardProgressKey(WheelRewardData rewardData, Sprite rewardIcon)
        {
            string rewardTitle = ResolveRewardTitle(rewardData, rewardIcon);
            string iconName = rewardIcon != null ? rewardIcon.name : "no_icon";
            return rewardData.rewardType + "_" + rewardTitle + "_" + iconName;
        }

        private string ResolveRewardTitle(WheelRewardData rewardData, Sprite rewardIcon)
        {
            string fallbackTitle = ResolveFallbackRewardTitle(rewardData.rewardType);
            return rewardPool != null
                ? rewardPool.ResolveName(rewardData.rewardType, rewardIcon, fallbackTitle)
                : fallbackTitle;
        }

        private static string ResolveFallbackRewardTitle(WheelRewardType rewardType)
        {
            switch (rewardType)
            {
                case WheelRewardType.Cash:
                    return "CASH";
                case WheelRewardType.Cards:
                    return "CARD";
                case WheelRewardType.Points:
                    return "PISTOL";
                case WheelRewardType.Gold:
                    return "GOLD";
                default:
                    return rewardType.ToString().ToUpperInvariant();
            }
        }

        private static int ResolveDisplayOrder(WheelRewardType rewardType)
        {
            switch (rewardType)
            {
                case WheelRewardType.Cash:
                    return 0;
                case WheelRewardType.Cards:
                    return 10;
                case WheelRewardType.Points:
                    return 20;
                case WheelRewardType.Gold:
                    return 30;
                default:
                    return 99;
            }
        }

        private static int CompareRewardStates(WinResultRewardCardState left, WinResultRewardCardState right)
        {
            int orderComparison = left.DisplayOrder.CompareTo(right.DisplayOrder);
            return orderComparison != 0
                ? orderComparison
                : left.InsertionIndex.CompareTo(right.InsertionIndex);
        }
        #endregion

        #region Nested Types
        private sealed class WinResultRewardCardState
        {
            public WinResultRewardCardState(
                string title,
                bool isProgress,
                string progressKey,
                int progressRequiredAmount,
                int displayOrder,
                int insertionIndex,
                Sprite rewardIcon)
            {
                Title = title;
                IsProgress = isProgress;
                ProgressKey = progressKey;
                ProgressRequiredAmount = Mathf.Max(1, progressRequiredAmount);
                DisplayOrder = displayOrder;
                InsertionIndex = insertionIndex;
                RewardIcon = rewardIcon;
            }

            public string Title { get; }
            public bool IsProgress { get; }
            public string ProgressKey { get; }
            public int ProgressRequiredAmount { get; }
            public int DisplayOrder { get; }
            public int InsertionIndex { get; }
            public Sprite RewardIcon { get; }
            public int AmountValue { get; private set; }
            public string AmountPrefix { get; set; } = "x";
            public int StartCompletedCount { get; private set; }
            public int StartProgressValue { get; private set; }
            public int CompletedCount { get; private set; }
            public int ProgressValue { get; private set; }

            public void AddAmount(int amount)
            {
                AmountValue += Mathf.Max(0, amount);
            }

            public void AddProgress(int amount)
            {
                AmountValue += Mathf.Max(0, amount);
                SetProgress(CompletedCount, ProgressValue + Mathf.Max(0, amount), ProgressRequiredAmount);
            }

            public void SetProgress(int completedCount, int progressValue, int progressRequiredAmount)
            {
                int safeRequiredAmount = Mathf.Max(1, progressRequiredAmount);
                int safeProgressValue = Mathf.Max(0, progressValue);

                CompletedCount = Mathf.Max(0, completedCount) + (safeProgressValue / safeRequiredAmount);
                ProgressValue = safeProgressValue % safeRequiredAmount;
                StartCompletedCount = CompletedCount;
                StartProgressValue = ProgressValue;
            }

            public void SetProgressAnimation(WheelCardRewardProgress previous, WheelCardRewardProgress current)
            {
                StartCompletedCount = previous.CompletedCount;
                StartProgressValue = previous.ProgressValue;
                CompletedCount = current.CompletedCount;
                ProgressValue = current.ProgressValue;
            }
        }
        #endregion
    }
}
