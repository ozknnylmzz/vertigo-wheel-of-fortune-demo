using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Popups.UI;

namespace Vertigo.WheelOfFortune.Popups.Runtime
{
    public sealed class WheelPopupManager : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private GameObject popupBlocker;
        [SerializeField] private Canvas popupBlockerCanvas;
        [SerializeField] private Image popupBlockerImage;
        [SerializeField] private List<WheelPopupBase> popups = new List<WheelPopupBase>();
        [SerializeField] private bool hideOnAwake = true;
        [SerializeField] private bool showResultPopupsFromGameState = true;
        #endregion

        #region Runtime State
        private bool isGameFlowSubscribed;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (hideOnAwake)
            {
                HideAll();
            }
        }

        private void OnEnable()
        {
            SubscribePopupCloseRequests();

            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.CashOutConfirmRequested += ShowCashOutConfirm;
            WheelGameEventBus.ExitGameConfirmRequested += ShowExitGameConfirm;
            WheelGameEventBus.StageRewardPopupRequested += ShowStageReward;
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
            UnsubscribePopupCloseRequests();

            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.CashOutConfirmRequested -= ShowCashOutConfirm;
            WheelGameEventBus.ExitGameConfirmRequested -= ShowExitGameConfirm;
            WheelGameEventBus.StageRewardPopupRequested -= ShowStageReward;
            TryUnsubscribeGameFlow();
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (popupRoot == null)
            {
                popupRoot = gameObject;
            }

            if (popupBlockerCanvas == null && popupBlocker != null)
            {
                popupBlockerCanvas = popupBlocker.GetComponent<Canvas>();
            }

            if (popupBlockerImage == null && popupBlocker != null)
            {
                popupBlockerImage = popupBlocker.GetComponent<Image>();
            }

            RefreshPopupReferences();
        }

        [ContextMenu("Refresh Popup References")]
        private void RefreshPopupReferences()
        {
            popups.Clear();
            popups.AddRange(GetComponentsInChildren<WheelPopupBase>(true));
        }
#endif
        #endregion

        #region Public API
        public void Show(WheelPopupType popupType)
        {
            HidePopups();

            WheelPopupBase popup = ResolvePopup(popupType);
            if (popup == null)
            {
                SetPopupShellActive(false);
                return;
            }

            SetPopupShellActive(true);
            ApplyBlockerVisual(popup);
            popup.Show();
        }

        public void ShowBombResult()
        {
            Show(WheelPopupType.BombResult);
        }

        public void ShowWinResult()
        {
            Show(WheelPopupType.WinResult);
        }

        public void ShowCashOutConfirm()
        {
            Show(WheelPopupType.CashOutConfirm);
        }

        public void ShowExitGameConfirm()
        {
            Show(WheelPopupType.ExitGameConfirm);
        }

        public void ShowStageReward()
        {
            ShowStageReward(WheelGameFlowManager.Instance != null ? WheelGameFlowManager.Instance.CurrentLevel : 0);
        }

        public void ShowStageReward(int level)
        {
            HidePopups();

            StageRewardPopup popup = ResolvePopup(WheelPopupType.StageReward) as StageRewardPopup;
            if (popup == null)
            {
                SetPopupShellActive(false);
                WheelGameFlowManager.Instance?.CompleteStageReward();
                return;
            }

            popup.Setup(level);
            SetPopupShellActive(true);
            ApplyBlockerVisual(popup);
            popup.Show();
        }

        public void HideAll()
        {
            HidePopups();
            SetPopupShellActive(false);
        }
        #endregion

        #region Event Handlers
        private void HandleGameStateChanged(WheelGameState gameState)
        {
            if (gameState == WheelGameState.Lose)
            {
                ShowBombResult();
                return;
            }

            if (gameState == WheelGameState.Win)
            {
                ShowWinResult();
            }
        }
        #endregion

        #region Helpers
        private WheelPopupBase ResolvePopup(WheelPopupType popupType)
        {
            for (int i = 0; i < popups.Count; i++)
            {
                WheelPopupBase popup = popups[i];
                if (popup != null && popup.PopupType == popupType)
                {
                    return popup;
                }
            }

            return null;
        }

        private void HidePopups()
        {
            for (int i = 0; i < popups.Count; i++)
            {
                if (popups[i] != null)
                {
                    popups[i].Hide();
                }
            }
        }

        private void SubscribePopupCloseRequests()
        {
            for (int i = 0; i < popups.Count; i++)
            {
                if (popups[i] != null)
                {
                    popups[i].CloseRequested -= HandlePopupCloseRequested;
                    popups[i].CloseRequested += HandlePopupCloseRequested;
                }
            }
        }

        private void UnsubscribePopupCloseRequests()
        {
            for (int i = 0; i < popups.Count; i++)
            {
                if (popups[i] != null)
                {
                    popups[i].CloseRequested -= HandlePopupCloseRequested;
                }
            }
        }

        private void HandlePopupCloseRequested(WheelPopupBase _)
        {
            HideAll();
        }

        private void TrySubscribeGameFlow()
        {
            if (isGameFlowSubscribed || !showResultPopupsFromGameState || WheelGameFlowManager.Instance == null)
            {
                return;
            }

            WheelGameFlowManager.Instance.GameStateChanged += HandleGameStateChanged;
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

        private void SetPopupShellActive(bool isActive)
        {
            if (popupRoot != null && popupRoot != gameObject)
            {
                popupRoot.SetActive(isActive);
            }

            if (popupBlocker != null)
            {
                popupBlocker.SetActive(isActive);
            }
        }

        private void ApplyBlockerVisual(WheelPopupBase popup)
        {
            if (popupBlockerCanvas != null)
            {
                popupBlockerCanvas.sortingOrder = popup.BlockerSortingOrder;
            }

            if (popupBlockerImage != null)
            {
                Color blockerColor = popupBlockerImage.color;
                blockerColor.a = popup.BlockerAlpha;
                popupBlockerImage.color = blockerColor;
            }
        }
        #endregion
    }
}
