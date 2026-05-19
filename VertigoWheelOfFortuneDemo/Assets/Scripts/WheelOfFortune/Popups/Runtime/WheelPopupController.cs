using System;
using System.Collections.Generic;
using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Popups.UI;

namespace Vertigo.WheelOfFortune.Popups.Runtime
{
    public sealed class WheelPopupController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private GameObject popupBlocker;
        [SerializeField] private Canvas popupBlockerCanvas;
        [SerializeField] private List<PopupEntry> popups = new List<PopupEntry>();
        [SerializeField] private bool hideOnAwake = true;
        [SerializeField] private bool showResultPopupsFromGameState = true;
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
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.ExitConfirmRequested += ShowExitConfirm;
            if (showResultPopupsFromGameState)
            {
                WheelGameEventBus.GameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.ExitConfirmRequested -= ShowExitConfirm;
            WheelGameEventBus.GameStateChanged -= HandleGameStateChanged;
        }
        #endregion

        #region Public API
        public void Show(WheelPopupType popupType)
        {
            HidePopups();

            PopupEntry popup = ResolvePopup(popupType);
            if (popup == null || popup.View == null)
            {
                SetPopupShellActive(false);
                return;
            }

            SetPopupShellActive(true);
            ApplyBlockerSortingOrder(popup);
            popup.View.Show();
        }

        public void ShowBombResult()
        {
            Show(WheelPopupType.BombResult);
        }

        public void ShowWinResult()
        {
            Show(WheelPopupType.WinResult);
        }

        public void ShowExitConfirm()
        {
            Show(WheelPopupType.ExitConfirm);
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
        private PopupEntry ResolvePopup(WheelPopupType popupType)
        {
            for (int i = 0; i < popups.Count; i++)
            {
                PopupEntry popup = popups[i];
                if (popup.Type == popupType)
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
                if (popups[i].View != null)
                {
                    popups[i].View.Hide();
                }
            }
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

        private void ApplyBlockerSortingOrder(PopupEntry popup)
        {
            if (popupBlockerCanvas != null)
            {
                popupBlockerCanvas.sortingOrder = popup.BlockerSortingOrder;
            }
        }
        #endregion

        #region Nested Types
        [Serializable]
        private sealed class PopupEntry
        {
            [SerializeField] private WheelPopupType type;
            [SerializeField] private WheelPopupView view;
            [SerializeField] private int blockerSortingOrder = 100;

            public WheelPopupType Type => type;
            public WheelPopupView View => view;
            public int BlockerSortingOrder => blockerSortingOrder;
        }
        #endregion
    }
}
