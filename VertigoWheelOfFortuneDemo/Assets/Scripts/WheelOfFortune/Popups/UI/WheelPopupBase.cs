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
        [SerializeField] [ColorUsage(false)] private Color blockerColor = Color.black;
        [SerializeField] [Range(0f, 1f)] private float blockerAlpha = 0.75f;
        #endregion

        #region Properties
        public abstract WheelPopupType PopupType { get; }
        public int BlockerSortingOrder => blockerSortingOrder;
        public Color BlockerColor => blockerColor;
        public float BlockerAlpha => blockerAlpha;
        #endregion

        #region Events
        public event Action<WheelPopupBase> CloseRequested;
        #endregion

        #region Editor
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
        }
#endif
        #endregion

        #region Public API
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region Helpers
        protected void RequestClose()
        {
            CloseRequested?.Invoke(this);
        }
        #endregion
    }
}
