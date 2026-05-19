using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.Rewards.Data;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public sealed class RewardFlyGroupController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private RewardFlyAnimationSettingsAsset animationSettings;
        [SerializeField] private RectTransform animationRoot;
        [SerializeField] private Image[] flyIcons;
        #endregion

        #region Runtime State
        private readonly Queue<RewardFlyRequest> requests = new Queue<RewardFlyRequest>();
        private Sequence activeSequence;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (animationRoot == null)
            {
                animationRoot = transform as RectTransform;
            }

            HideIcons();
        }

        private void OnDisable()
        {
            activeSequence?.Kill(false);
            activeSequence = null;
            requests.Clear();
            HideIcons();
        }
        #endregion

        #region Public API
        public void Play(
            Sprite rewardIcon,
            RectTransform startPoint,
            RectTransform targetPoint,
            int rewardAmount,
            Action<int> rewardStepCompleted,
            Action rewardAnimationCompleted = null)
        {
            requests.Enqueue(new RewardFlyRequest(
                rewardIcon,
                startPoint,
                targetPoint,
                rewardAmount,
                rewardStepCompleted,
                rewardAnimationCompleted));

            if (activeSequence == null || !activeSequence.IsActive())
            {
                PlayNext();
            }
        }
        #endregion

        #region Animation
        private void PlayNext()
        {
            if (requests.Count == 0)
            {
                activeSequence = null;
                return;
            }

            RewardFlyRequest request = requests.Dequeue();
            if (request.StartPoint == null || request.TargetPoint == null || flyIcons == null || flyIcons.Length == 0)
            {
                request.StepCompleted?.Invoke(request.Amount);
                request.AnimationCompleted?.Invoke();
                PlayNext();
                return;
            }

            int iconCount = ResolveIconCount();
            if (iconCount == 0)
            {
                request.StepCompleted?.Invoke(request.Amount);
                request.AnimationCompleted?.Invoke();
                PlayNext();
                return;
            }

            int incrementIndex = 0;
            int[] increments = ResolveIncrements(request.Amount, iconCount);
            Vector3 startPosition = request.StartPoint.position;
            Vector3 targetPosition = request.TargetPoint.position;

            activeSequence = DOTween.Sequence().SetTarget(this);
            for (int i = 0; i < flyIcons.Length; i++)
            {
                Image icon = flyIcons[i];
                if (icon == null)
                {
                    continue;
                }

                int increment = increments[incrementIndex++];
                RectTransform iconRect = icon.rectTransform;
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * ScatterRadius;
                Vector3 scatterPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

                icon.sprite = request.Icon;
                icon.enabled = request.Icon != null;
                icon.gameObject.SetActive(true);
                iconRect.position = startPosition;
                iconRect.localScale = Vector3.zero;

                Sequence iconSequence = DOTween.Sequence()
                    .Append(iconRect.DOScale(1f, ScatterDurationSeconds).SetEase(Ease.OutBack))
                    .Join(iconRect.DOMove(scatterPosition, ScatterDurationSeconds).SetEase(Ease.OutQuad))
                    .Append(iconRect.DOMove(targetPosition, FlyDurationSeconds).SetEase(Ease.InQuad))
                    .Join(iconRect.DOScale(0f, FlyDurationSeconds).SetEase(Ease.InBack))
                    .AppendCallback(() =>
                    {
                        icon.gameObject.SetActive(false);
                        if (increment > 0)
                        {
                            request.StepCompleted?.Invoke(increment);
                        }
                    });

                activeSequence.Insert(i * IconDelaySeconds, iconSequence);
            }

            activeSequence
                .OnComplete(() =>
                {
                    HideIcons();
                    request.AnimationCompleted?.Invoke();
                    activeSequence = null;
                    PlayNext();
                });
        }

        private int ResolveIconCount()
        {
            int iconCount = 0;
            for (int i = 0; i < flyIcons.Length; i++)
            {
                if (flyIcons[i] != null)
                {
                    iconCount++;
                }
            }

            return iconCount;
        }

        private void HideIcons()
        {
            if (flyIcons == null)
            {
                return;
            }

            for (int i = 0; i < flyIcons.Length; i++)
            {
                if (flyIcons[i] != null)
                {
                    flyIcons[i].gameObject.SetActive(false);
                }
            }
        }

        private static int[] ResolveIncrements(int amount, int count)
        {
            int[] increments = new int[count];
            if (amount <= 0 || count <= 0)
            {
                return increments;
            }

            int baseValue = amount / count;
            int remainder = amount % count;
            for (int i = 0; i < count; i++)
            {
                increments[i] = baseValue + (i >= count - remainder ? 1 : 0);
            }

            return increments;
        }

        private float ScatterRadius => animationSettings != null ? animationSettings.ScatterRadius : 40f;
        private float ScatterDurationSeconds => animationSettings != null ? animationSettings.ScatterDurationSeconds : 0.2f;
        private float FlyDurationSeconds => animationSettings != null ? animationSettings.FlyDurationSeconds : 0.45f;
        private float IconDelaySeconds => animationSettings != null ? animationSettings.IconDelaySeconds : 0.04f;
        #endregion

        #region Nested Types
        private readonly struct RewardFlyRequest
        {
            public RewardFlyRequest(
                Sprite icon,
                RectTransform startPoint,
                RectTransform targetPoint,
                int amount,
                Action<int> stepCompleted,
                Action animationCompleted)
            {
                Icon = icon;
                StartPoint = startPoint;
                TargetPoint = targetPoint;
                Amount = amount;
                StepCompleted = stepCompleted;
                AnimationCompleted = animationCompleted;
            }

            public Sprite Icon { get; }
            public RectTransform StartPoint { get; }
            public RectTransform TargetPoint { get; }
            public int Amount { get; }
            public Action<int> StepCompleted { get; }
            public Action AnimationCompleted { get; }
        }
        #endregion
    }
}
