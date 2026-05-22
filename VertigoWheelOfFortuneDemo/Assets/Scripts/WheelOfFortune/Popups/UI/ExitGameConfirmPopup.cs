using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Popups.Runtime;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class ExitGameConfirmPopup : WheelPopupBase
    {
        #region Inspector Fields
        [SerializeField] private Button ui_button_exit;
        [SerializeField] private Button ui_button_go_back;
        #endregion

        #region Properties
        public override WheelPopupType PopupType => WheelPopupType.ExitGameConfirm;
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
            ui_button_exit ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_exit) ||
                          button.name == "ui_button_popup_exit_game_exit" ||
                          button.name == "ui_button_popup_exit_collect_rewards");
            ui_button_go_back ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_go_back) ||
                          button.name == "ui_button_popup_exit_game_go_back" ||
                          button.name == "ui_button_popup_exit_back");
        }
#endif
        #endregion

        #region Event Handlers
        private void HandleExitButtonClicked()
        {
            RequestClose();
            WheelGameEventBus.PublishRewardsResetRequested();
            WheelGameFlowManager.Instance?.RestartGame();
        }

        private void HandleGoBackButtonClicked()
        {
            RequestClose();
        }
        #endregion

        #region Helpers
        private void SubscribeButtons()
        {
            ui_button_exit?.onClick.RemoveListener(HandleExitButtonClicked);
            ui_button_exit?.onClick.AddListener(HandleExitButtonClicked);
            ui_button_go_back?.onClick.RemoveListener(HandleGoBackButtonClicked);
            ui_button_go_back?.onClick.AddListener(HandleGoBackButtonClicked);
        }

        private void UnsubscribeButtons()
        {
            ui_button_exit?.onClick.RemoveListener(HandleExitButtonClicked);
            ui_button_go_back?.onClick.RemoveListener(HandleGoBackButtonClicked);
        }
        #endregion
    }
}
