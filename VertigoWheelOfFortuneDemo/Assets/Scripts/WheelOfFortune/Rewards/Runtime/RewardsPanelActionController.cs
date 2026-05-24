using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public sealed class RewardsPanelActionController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Button ui_button_exit;
        #endregion

        #region Runtime State
        private bool isGameFlowSubscribed;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            SubscribeExitButton();

            if (!Application.isPlaying)
            {
                return;
            }

            TrySubscribeGameFlow();
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                TrySubscribeGameFlow();
            }
        }

        private void OnDisable()
        {
            UnsubscribeExitButton();
            ApplyExitButtonVisibility(WheelGameState.Idle);

            if (!Application.isPlaying)
            {
                return;
            }

            TryUnsubscribeGameFlow();
        }
        #endregion

        #region Event Handlers
        private void HandleGameStateChanged(WheelGameState gameState)
        {
            ApplyExitButtonVisibility(gameState);
        }

        private void HandleExitButtonClicked()
        {
            if (WheelGameFlowManager.Instance != null &&
                WheelGameFlowManager.Instance.CurrentState == WheelGameState.Spinning)
            {
                return;
            }

            if (WheelGameFlowManager.Instance != null && WheelGameFlowManager.Instance.CurrentLevel <= 1)
            {
                WheelGameEventBus.PublishExitGameConfirmRequested();
                return;
            }

            WheelGameEventBus.PublishCashOutConfirmRequested();
        }
        #endregion

        #region Helpers
        private void SubscribeExitButton()
        {
            if (ui_button_exit == null)
            {
                return;
            }

            ui_button_exit.onClick.RemoveListener(HandleExitButtonClicked);
            ui_button_exit.onClick.AddListener(HandleExitButtonClicked);
        }

        private void UnsubscribeExitButton()
        {
            if (ui_button_exit == null)
            {
                return;
            }

            ui_button_exit.onClick.RemoveListener(HandleExitButtonClicked);
        }

        private void TrySubscribeGameFlow()
        {
            if (isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                return;
            }

            WheelGameFlowManager.Instance.GameStateChanged += HandleGameStateChanged;
            ApplyExitButtonVisibility(WheelGameFlowManager.Instance.CurrentState);
            isGameFlowSubscribed = true;
        }

        private void TryUnsubscribeGameFlow()
        {
            if (!isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                isGameFlowSubscribed = false;
                return;
            }

            WheelGameFlowManager.Instance.GameStateChanged -= HandleGameStateChanged;
            isGameFlowSubscribed = false;
        }

        private void ApplyExitButtonVisibility(WheelGameState gameState)
        {
            if (ui_button_exit != null)
            {
                ui_button_exit.gameObject.SetActive(gameState != WheelGameState.Lose);
            }
        }
        #endregion
    }
}
