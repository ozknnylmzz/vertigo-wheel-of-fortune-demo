using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Popups.Runtime;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class CashOutConfirmPopup : WheelPopupBase
    {
        #region Inspector Fields
        [SerializeField] private Button ui_button_collect_rewards;
        [SerializeField] private Button ui_button_go_back;
        #endregion

        #region Properties
        public override WheelPopupType PopupType => WheelPopupType.CashOutConfirm;
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
            ui_button_collect_rewards ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_collect_rewards) ||
                          button.name == "ui_button_popup_cash_out_collect_rewards" ||
                          button.name == "ui_button_popup_exit_collect_rewards");
            ui_button_go_back ??= Array.Find(buttons,
                button => button.name == nameof(ui_button_go_back) ||
                          button.name == "ui_button_popup_cash_out_go_back" ||
                          button.name == "ui_button_popup_exit_back");
        }
#endif
        #endregion

        #region Event Handlers
        private void HandleCollectRewardsButtonClicked()
        {
            RequestClose();

            WheelGameFlowManager flowManager = WheelGameFlowManager.Instance;
            flowManager?.SetGameState(WheelGameState.Win);
            WheelGameEventBus.PublishRewardsResetRequested();
            flowManager?.RestartGame();
        }

        private void HandleGoBackButtonClicked()
        {
            RequestClose();
        }
        #endregion

        #region Helpers
        private void SubscribeButtons()
        {
            ui_button_collect_rewards?.onClick.RemoveListener(HandleCollectRewardsButtonClicked);
            ui_button_collect_rewards?.onClick.AddListener(HandleCollectRewardsButtonClicked);
            ui_button_go_back?.onClick.RemoveListener(HandleGoBackButtonClicked);
            ui_button_go_back?.onClick.AddListener(HandleGoBackButtonClicked);
        }

        private void UnsubscribeButtons()
        {
            ui_button_collect_rewards?.onClick.RemoveListener(HandleCollectRewardsButtonClicked);
            ui_button_go_back?.onClick.RemoveListener(HandleGoBackButtonClicked);
        }
        #endregion
    }
}
