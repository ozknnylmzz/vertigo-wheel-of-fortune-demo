using UnityEngine;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class WheelPopupView : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private CanvasGroup canvasGroup;
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
#endif
        #endregion

        #region Public API
        public void Show()
        {
            gameObject.SetActive(true);
            SetCanvasState(1f, true);
        }

        public void Hide()
        {
            SetCanvasState(0f, false);
            gameObject.SetActive(false);
        }
        #endregion

        #region Helpers
        private void SetCanvasState(float alpha, bool isInteractive)
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
