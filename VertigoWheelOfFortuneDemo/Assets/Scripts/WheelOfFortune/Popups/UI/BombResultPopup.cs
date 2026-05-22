using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Popups.Runtime;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class BombResultPopup : WheelPopupBase
    {
        #region Inspector Fields
        [SerializeField] private Button ui_button_give_up;
        [SerializeField] private Button ui_button_revive;
        #endregion

        #region Properties
        public override WheelPopupType PopupType => WheelPopupType.BombResult;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            SubscribeButtons();
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            Button[] buttons = GetComponentsInChildren<Button>(true);
            ui_button_give_up ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_give_up) || button.name == "ui_button_popup_bomb_give_up");
            ui_button_revive ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_revive) || button.name == "ui_button_popup_bomb_revive");
        }
#endif
        #endregion

        #region Event Handlers
        private void HandleGiveUpButtonClicked()
        {
            RequestClose();
            WheelGameEventBus.PublishRewardsResetRequested();
            WheelGameFlowManager.Instance?.RestartGame();
        }

        private void HandleReviveButtonClicked()
        {
            RequestClose();
            WheelGameFlowManager.Instance?.ContinueAfterBomb();
        }
        #endregion

        #region Helpers
        private void SubscribeButtons()
        {
            ui_button_give_up?.onClick.RemoveListener(HandleGiveUpButtonClicked);
            ui_button_give_up?.onClick.AddListener(HandleGiveUpButtonClicked);
            ui_button_revive?.onClick.RemoveListener(HandleReviveButtonClicked);
            ui_button_revive?.onClick.AddListener(HandleReviveButtonClicked);
        }

        private void UnsubscribeButtons()
        {
            ui_button_give_up?.onClick.RemoveListener(HandleGiveUpButtonClicked);
            ui_button_revive?.onClick.RemoveListener(HandleReviveButtonClicked);
        }
        #endregion
    }
}
