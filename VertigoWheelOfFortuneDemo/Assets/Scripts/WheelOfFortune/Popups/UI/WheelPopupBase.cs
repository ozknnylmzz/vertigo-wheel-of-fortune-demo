using System;
using UnityEngine;
using Vertigo.WheelOfFortune.Popups.Runtime;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public abstract class WheelPopupBase : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Popup Settings")]
        [SerializeField] private int blockerSortingOrder = 100;
        [SerializeField] [Range(0f, 1f)] private float blockerAlpha = 0.75f;

        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        #endregion

        #region Properties
        public abstract WheelPopupType PopupType { get; }
        public int BlockerSortingOrder => blockerSortingOrder;
        public float BlockerAlpha => blockerAlpha;
        #endregion

        #region Events
        public event Action<WheelPopupBase> CloseRequested;
        #endregion

        #region Editor
#if UNITY_EDITOR
        protected virtual void Reset()
        {
            TryAutoAssignReferences();
        }

        protected virtual void OnValidate()
        {
            TryAutoAssignReferences();
        }

        private void TryAutoAssignReferences()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
#endif
        #endregion

        #region Public API
        public virtual void Show()
        {
            gameObject.SetActive(true);
            SetCanvasState(1f, true);
        }

        public virtual void Hide()
        {
            SetCanvasState(0f, false);
            gameObject.SetActive(false);
        }
        #endregion

        #region Helpers
        protected void RequestClose()
        {
            CloseRequested?.Invoke(this);
        }

        protected void SetCanvasState(float alpha, bool isInteractive)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
            canvasGroup.interactable = isInteractive;
            canvasGroup.blocksRaycasts = isInteractive;
        }
        #endregion
    }
}
