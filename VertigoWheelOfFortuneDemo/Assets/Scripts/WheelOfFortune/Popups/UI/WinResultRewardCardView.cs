using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.WheelOfFortune.Popups.UI
{
    public sealed class WinResultRewardCardView : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private TMP_Text ui_text_card_title_value;
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TMP_Text ui_text_reward_amount_value;
        [SerializeField] private GameObject ui_group_progress;
        [SerializeField] private Image ui_image_progress_background;
        [SerializeField] private Image ui_image_progress_fill;
        [SerializeField] private Image ui_image_progress_source_icon;
        [SerializeField] private Image ui_image_progress_reward_icon;
        [SerializeField] private TMP_Text ui_text_reward_count_value;
        [SerializeField] private TMP_Text ui_text_progress_value;

        [Header("Progress Animation")]
        [SerializeField] [Min(0f)] private float progressTweenDurationPerFill = 0.45f;
        [SerializeField] [Min(0f)] private float progressTweenResetDelay = 0.05f;
        [SerializeField] private Ease progressTweenEase = Ease.OutQuad;
        #endregion

        #region Runtime State
        private Sequence progressTweenSequence;
        #endregion

        #region Properties
        public RectTransform RectTransform => transform as RectTransform;
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            AssignReferences();
        }
#endif
        #endregion

        #region Unity Methods
        private void OnDisable()
        {
            KillProgressTween();
        }
        #endregion

        #region Public API
        public void SetTitle(string title)
        {
            if (ui_text_card_title_value != null)
            {
                ui_text_card_title_value.text = title;
            }
        }

        public void SetRewardIcon(Sprite rewardIcon)
        {
            if (ui_image_reward_icon != null)
            {
                SetImageSprite(ui_image_reward_icon, rewardIcon, false);
            }
        }

        public void SetAmount(string amount)
        {
            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.gameObject.SetActive(true);
                ui_text_reward_amount_value.text = amount;
            }
        }

        public void SetProgressIcons(Sprite sourceIcon, Sprite rewardIcon)
        {
            SetImageSprite(ui_image_progress_source_icon, sourceIcon, false);
            SetImageSprite(ui_image_progress_reward_icon, rewardIcon, false);
        }

        public void SetProgress(int completedCount, int progressValue, int requiredAmount)
        {
            KillProgressTween();
            ApplyProgress(completedCount, progressValue, requiredAmount);
        }

        public void PlayProgressTween(
            int startCompletedCount,
            int startProgressValue,
            int endCompletedCount,
            int endProgressValue,
            int requiredAmount)
        {
            int safeRequiredAmount = Mathf.Max(1, requiredAmount);
            int safeStartCompletedCount = Mathf.Max(0, startCompletedCount);
            int safeEndCompletedCount = Mathf.Max(safeStartCompletedCount, endCompletedCount);
            int safeStartProgressValue = Mathf.Clamp(startProgressValue, 0, safeRequiredAmount);
            int safeEndProgressValue = Mathf.Clamp(endProgressValue, 0, safeRequiredAmount);

            KillProgressTween();
            ApplyProgress(safeStartCompletedCount, safeStartProgressValue, safeRequiredAmount);

            if (!Application.isPlaying
                || progressTweenDurationPerFill <= 0f
                || (safeStartCompletedCount == safeEndCompletedCount && safeStartProgressValue == safeEndProgressValue))
            {
                ApplyProgress(safeEndCompletedCount, safeEndProgressValue, safeRequiredAmount);
                return;
            }

            progressTweenSequence = DOTween.Sequence().SetTarget(this);

            int currentCompletedCount = safeStartCompletedCount;
            int currentProgressValue = safeStartProgressValue;
            while (currentCompletedCount < safeEndCompletedCount)
            {
                AppendProgressTween(currentCompletedCount, currentProgressValue, safeRequiredAmount, safeRequiredAmount);

                int completedCountAfterFill = currentCompletedCount + 1;
                int resetCompletedCount = completedCountAfterFill;
                progressTweenSequence.AppendCallback(() => ApplyProgress(resetCompletedCount, 0, safeRequiredAmount));
                if (progressTweenResetDelay > 0f)
                {
                    progressTweenSequence.AppendInterval(progressTweenResetDelay);
                }

                currentCompletedCount = completedCountAfterFill;
                currentProgressValue = 0;
            }

            AppendProgressTween(currentCompletedCount, currentProgressValue, safeEndProgressValue, safeRequiredAmount);
            progressTweenSequence.OnComplete(() =>
            {
                ApplyProgress(safeEndCompletedCount, safeEndProgressValue, safeRequiredAmount);
                progressTweenSequence = null;
            });
        }

        public void SetProgressVisible(bool isVisible)
        {
            if (ui_group_progress != null)
            {
                ui_group_progress.SetActive(isVisible);
            }

            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.gameObject.SetActive(!isVisible);
            }
        }
        #endregion

        #region Private Methods
        private void KillProgressTween()
        {
            if (progressTweenSequence == null)
            {
                return;
            }

            progressTweenSequence.Kill();
            progressTweenSequence = null;
        }

        private void AppendProgressTween(
            int completedCount,
            int startProgressValue,
            int endProgressValue,
            int requiredAmount)
        {
            if (startProgressValue == endProgressValue)
            {
                progressTweenSequence.AppendCallback(() => ApplyProgress(completedCount, endProgressValue, requiredAmount));
                return;
            }

            float duration = ResolveProgressTweenDuration(startProgressValue, endProgressValue, requiredAmount);
            progressTweenSequence.Append(DOTween.To(
                    () => (float)startProgressValue,
                    value => ApplyProgress(completedCount, Mathf.RoundToInt(value), requiredAmount),
                    endProgressValue,
                    duration)
                .SetEase(progressTweenEase));
        }

        private float ResolveProgressTweenDuration(int startProgressValue, int endProgressValue, int requiredAmount)
        {
            float normalizedDistance = Mathf.Abs(endProgressValue - startProgressValue) / (float)Mathf.Max(1, requiredAmount);
            return Mathf.Max(0.08f, progressTweenDurationPerFill * normalizedDistance);
        }

        private void ApplyProgress(int completedCount, int progressValue, int requiredAmount)
        {
            int safeRequiredAmount = Mathf.Max(1, requiredAmount);
            int safeProgressValue = Mathf.Clamp(progressValue, 0, safeRequiredAmount);

            if (ui_text_reward_count_value != null)
            {
                ui_text_reward_count_value.text = Mathf.Max(0, completedCount).ToString();
            }

            if (ui_text_progress_value != null)
            {
                ui_text_progress_value.text = safeProgressValue + "/" + safeRequiredAmount;
            }

            ApplyProgressFill(safeProgressValue / (float)safeRequiredAmount);
        }

        private void AssignReferences()
        {
            ui_text_card_title_value = Resolve(ui_text_card_title_value, "ui_text_card_title_value");
            ui_image_reward_icon = Resolve(ui_image_reward_icon, "ui_image_reward_icon");
            ui_text_reward_amount_value = Resolve(ui_text_reward_amount_value, "ui_text_reward_amount_value");
            ui_image_progress_background = Resolve(ui_image_progress_background, "ui_image_progress_background");
            ui_image_progress_fill = Resolve(ui_image_progress_fill, "ui_image_progress_fill");
            ui_image_progress_source_icon = Resolve(ui_image_progress_source_icon, "ui_image_progress_source_icon");
            ui_image_progress_reward_icon = Resolve(ui_image_progress_reward_icon, "ui_image_progress_reward_icon");
            ui_text_reward_count_value = Resolve(ui_text_reward_count_value, "ui_text_reward_count_value");
            ui_text_progress_value = Resolve(ui_text_progress_value, "ui_text_progress_value");

            if (ui_group_progress == null)
            {
                RectTransform progressGroup = Resolve<RectTransform>(null, "ui_group_progress");
                ui_group_progress = progressGroup != null ? progressGroup.gameObject : null;
            }
        }

        private void ApplyProgressFill(float normalizedProgress)
        {
            if (ui_image_progress_fill == null)
            {
                return;
            }

            RectTransform fillRect = ui_image_progress_fill.rectTransform;
            if (fillRect != null)
            {
                Vector2 anchorMax = fillRect.anchorMax;
                anchorMax.x = Mathf.Clamp01(normalizedProgress);
                fillRect.anchorMax = anchorMax;
                fillRect.sizeDelta = new Vector2(0f, fillRect.sizeDelta.y);
            }

            ui_image_progress_fill.fillAmount = Mathf.Clamp01(normalizedProgress);
        }

        private static void SetImageSprite(Image image, Sprite sprite, bool hideWhenNull)
        {
            if (image == null)
            {
                return;
            }

            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
                image.enabled = true;
                return;
            }

            if (hideWhenNull)
            {
                image.enabled = false;
            }
        }

        private T Resolve<T>(T currentValue, string targetName) where T : Component
        {
            if (currentValue != null)
            {
                return currentValue;
            }

            return Array.Find(GetComponentsInChildren<T>(true), component => component.name == targetName);
        }
        #endregion
    }
}
