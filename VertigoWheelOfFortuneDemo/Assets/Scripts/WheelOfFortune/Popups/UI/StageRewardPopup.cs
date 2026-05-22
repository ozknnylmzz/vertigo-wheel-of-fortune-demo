using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Popups.Data;
using Vertigo.WheelOfFortune.Popups.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class StageRewardPopup : WheelPopupBase
    {
        #region Constants
        private const string DefaultSettingsPath =
            "Assets/Scripts/WheelOfFortune/Popups/Data/SO/stage_reward_popup_settings.asset";
        #endregion

        #region Inspector Fields
        [SerializeField] private StageRewardPopupSettingsAsset settings;
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TMP_Text ui_text_message_value;
        [SerializeField] private TMP_Text ui_text_button_value;
        [SerializeField] private Button ui_button_claim;
        #endregion

        #region Runtime State
        private Tween rewardIconSpinTween;
        private Vector3 rewardIconInitialLocalEulerAngles;
        private bool rewardIconInitialRotationCached;
        private WheelGameState stateBeforeShow = WheelGameState.Idle;
        #endregion

        #region Properties
        public override WheelPopupType PopupType => WheelPopupType.StageReward;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            TryAutoAssignStageRewardReferences();
        }

        private void OnEnable()
        {
            SubscribeButtons();
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
            StopRewardIconSpin(true);
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            settings ??= AssetDatabase.LoadAssetAtPath<StageRewardPopupSettingsAsset>(DefaultSettingsPath);
            TryAutoAssignStageRewardReferences();
        }
#endif
        #endregion

        #region Public API
        public override void Show()
        {
            stateBeforeShow = WheelGameFlowManager.Instance != null
                ? WheelGameFlowManager.Instance.CurrentState
                : WheelGameState.Idle;

            base.Show();
            StartRewardIconSpin();
        }

        public override void Hide()
        {
            StopRewardIconSpin(true);
            base.Hide();
        }

        public void Setup(int level)
        {
            TryAutoAssignStageRewardReferences();

            if (settings != null && settings.TryGetReward(level, out StageRewardPopupData rewardData))
            {
                Apply(rewardData);
                return;
            }

            SetMessage("Stage reward kazandin!");
        }
        #endregion

        #region Event Handlers
        private void HandleClaimButtonClicked()
        {
            WheelGameFlowManager flowManager = WheelGameFlowManager.Instance;

            RequestClose();
            if (flowManager == null)
            {
                return;
            }

            if (stateBeforeShow == WheelGameState.Spinning || flowManager.CurrentState == WheelGameState.Spinning)
            {
                flowManager.CompleteStageReward();
                return;
            }

            flowManager.SetGameState(stateBeforeShow);
        }
        #endregion

        #region Helpers
        private void Apply(StageRewardPopupData rewardData)
        {
            TryAutoAssignStageRewardReferences();

            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = rewardData.RewardIcon;
                ui_image_reward_icon.preserveAspect = true;
            }

            SetMessage(rewardData.Message);

            if (ui_text_button_value != null)
            {
                ui_text_button_value.text = rewardData.ButtonText;
            }
        }

        private void StartRewardIconSpin()
        {
            TryAutoAssignStageRewardReferences();

            RectTransform rewardIconRect = ResolveRewardIconRect();
            if (rewardIconRect == null)
            {
                return;
            }

            CacheRewardIconInitialRotation(rewardIconRect);
            StopRewardIconSpin(false);
            rewardIconRect.localEulerAngles = rewardIconInitialLocalEulerAngles;

            rewardIconSpinTween = rewardIconRect
                .DOLocalRotate(new Vector3(0f, 360f, 0f), GetRewardIconSpinDurationSeconds(), RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental)
                .SetTarget(rewardIconRect);
        }

        private void StopRewardIconSpin(bool resetRotation)
        {
            if (rewardIconSpinTween != null)
            {
                rewardIconSpinTween.Kill(false);
                rewardIconSpinTween = null;
            }

            if (!resetRotation || !rewardIconInitialRotationCached)
            {
                return;
            }

            RectTransform rewardIconRect = ResolveRewardIconRect();
            if (rewardIconRect != null)
            {
                rewardIconRect.localEulerAngles = rewardIconInitialLocalEulerAngles;
            }
        }

        private void CacheRewardIconInitialRotation(RectTransform rewardIconRect)
        {
            if (rewardIconInitialRotationCached)
            {
                return;
            }

            rewardIconInitialLocalEulerAngles = rewardIconRect.localEulerAngles;
            rewardIconInitialRotationCached = true;
        }

        private RectTransform ResolveRewardIconRect()
        {
            return ui_image_reward_icon != null ? ui_image_reward_icon.rectTransform : null;
        }

        private float GetRewardIconSpinDurationSeconds()
        {
            return settings != null ? settings.RewardIconSpinDurationSeconds : 1.25f;
        }

        private void TryAutoAssignStageRewardReferences()
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            ui_image_reward_icon ??= Array.Find(images,
                image => image.name == nameof(ui_image_reward_icon) ||
                         image.name == "ui_image_popup_stage_reward_icon" ||
                         image.name == "Image" && image.GetComponent<Button>() == null);

            TMP_Text[] labels = GetComponentsInChildren<TMP_Text>(true);
            ui_text_message_value ??= Array.Find(labels,
                label => label.name == nameof(ui_text_message_value) ||
                         label.name == "ui_text_popup_stage_reward_message_value" ||
                         label.name == "ui_text_popup_cash_out_message_value");
            ui_text_button_value ??= Array.Find(labels,
                label => label.name == nameof(ui_text_button_value) ||
                         label.transform.parent != null &&
                         (label.transform.parent.name == "ui_button_popup_stage_reward_claim" ||
                          label.transform.parent.name == "ui_button_popup_cash_out_collect_rewards"));

            Button[] buttons = GetComponentsInChildren<Button>(true);
            ui_button_claim ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_claim) ||
                          button.name == "ui_button_popup_stage_reward_claim" ||
                          button.name == "ui_button_popup_cash_out_collect_rewards");
        }

        private void SetMessage(string message)
        {
            if (ui_text_message_value != null)
            {
                ui_text_message_value.text = message;
            }
        }

        private void SubscribeButtons()
        {
            ui_button_claim?.onClick.RemoveListener(HandleClaimButtonClicked);
            ui_button_claim?.onClick.AddListener(HandleClaimButtonClicked);
        }

        private void UnsubscribeButtons()
        {
            ui_button_claim?.onClick.RemoveListener(HandleClaimButtonClicked);
        }
        #endregion
    }
}
