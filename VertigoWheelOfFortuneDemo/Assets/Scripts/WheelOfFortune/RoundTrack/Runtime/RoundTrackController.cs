using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.RoundTrack.Data;

namespace Vertigo.WheelOfFortune.RoundTrack.Runtime
{
    public sealed class RoundTrackController : MonoBehaviour
    {
        #region Constants
        private const string SlotItemPrefix = "ui_item_round_slot_";
        private const float DefaultSlotSpacing = 79f;
        private const float DefaultShiftDurationSeconds = 0.25f;
        #endregion

        #region Inspector Fields
        [Header("References")]
        [SerializeField] private RectTransform ui_container_round_slots_track;
        [SerializeField] private List<RectTransform> ui_round_slot_views = new List<RectTransform>();
        [SerializeField] private List<TMP_Text> ui_round_slot_value_texts = new List<TMP_Text>();
        [SerializeField] private RoundTrackTopSettingsAsset roundTrackTopSettings;

        [Header("Spawn Rule")]
        [SerializeField] [Min(1)] private int spawnShiftThreshold = 7;

        [SerializeField] private Ease shiftEase = Ease.OutCubic;
        #endregion

        #region Runtime State
        private readonly List<RoundSlotBinding> slotBindings = new List<RoundSlotBinding>();
        private Sequence shiftSequence;
        private float slotSpacing = DefaultSlotSpacing;
        private bool initialized;
        private bool isGameFlowSubscribed;
        private int shiftsSinceLastSpawn;
        private int currentRoundValue = 1;
        private RoundSlotBinding highlightedSlotBinding;
        #endregion

        #region Properties
        public bool IsShifting => shiftSequence != null && shiftSequence.IsActive();
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            InitializeTrack(ResolveCurrentRound());
            TrySubscribeGameFlow();
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            TrySubscribeGameFlow();
        }

        private void OnDisable()
        {
            TryUnsubscribeGameFlow();

            if (shiftSequence != null)
            {
                shiftSequence.Kill(false);
                shiftSequence = null;
            }
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshEditorBindings();
            InitializeTrack(ResolveCurrentRound());
        }

        [ContextMenu("Refresh Round Track Editor References")]
        private void RefreshRoundTrackEditorReferences()
        {
            RefreshEditorBindings();
            InitializeTrack(ResolveCurrentRound());
        }

        private void RefreshEditorBindings()
        {
            if (ui_container_round_slots_track == null)
            {
                return;
            }

            ui_round_slot_views.Clear();
            ui_round_slot_value_texts.Clear();

            for (int i = 0; i < ui_container_round_slots_track.childCount; i++)
            {
                Transform child = ui_container_round_slots_track.GetChild(i);
                if (!(child is RectTransform slotRect))
                {
                    continue;
                }

                if (!slotRect.name.StartsWith(SlotItemPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                ui_round_slot_views.Add(slotRect);
            }

            ui_round_slot_views.Sort((a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));

            for (int i = 0; i < ui_round_slot_views.Count; i++)
            {
                TMP_Text valueText = ui_round_slot_views[i].GetComponentInChildren<TMP_Text>(true);
                ui_round_slot_value_texts.Add(valueText);
            }
        }
#endif
        #endregion

        #region Event Handlers
        private void HandleRoundChanged(int roundValue)
        {
            int normalizedRound = Mathf.Clamp(roundValue, 1, WheelGameFlowManager.MaxRoundValue);
            if (normalizedRound == currentRoundValue)
            {
                return;
            }

            if (normalizedRound < currentRoundValue)
            {
                InitializeTrack(normalizedRound);
                return;
            }

            currentRoundValue = normalizedRound;
            ShiftLeftOnce();
        }

        private void HandleLevelChanged(int levelValue)
        {
            ApplyCurrentSlotTextColor(levelValue, IsShifting);
        }
        #endregion

        #region Track Setup
        private void InitializeTrack(int currentRound)
        {
            BuildBindingsFromSerializedReferences();
            if (slotBindings.Count == 0)
            {
                initialized = false;
                return;
            }

            spawnShiftThreshold = Mathf.Max(1, spawnShiftThreshold);
            shiftsSinceLastSpawn = 0;
            slotSpacing = ResolveSlotSpacing();
            highlightedSlotBinding = null;

            int centerIndex = ResolveCenterIndex();
            int normalizedRound = Mathf.Clamp(currentRound, 1, WheelGameFlowManager.MaxRoundValue);
            currentRoundValue = normalizedRound;

            for (int i = 0; i < slotBindings.Count; i++)
            {
                slotBindings[i].RoundValue = normalizedRound + (i - centerIndex);
                ApplySlotVisual(slotBindings[i]);
            }

            initialized = true;
            ApplyCurrentSlotTextColor(ResolveCurrentLevel(), false);
        }

        private void BuildBindingsFromSerializedReferences()
        {
            slotBindings.Clear();

            for (int i = 0; i < ui_round_slot_views.Count; i++)
            {
                RectTransform slotRect = ui_round_slot_views[i];
                TMP_Text slotText = i < ui_round_slot_value_texts.Count ? ui_round_slot_value_texts[i] : null;

                if (slotRect == null)
                {
                    continue;
                }

                slotBindings.Add(new RoundSlotBinding(slotRect, slotText));
            }

            SortSlotsByPosition();
        }
        #endregion

        #region Shift Flow
        private void ShiftLeftOnce()
        {
            if (!initialized)
            {
                return;
            }

            if (slotBindings.Count < 2)
            {
                return;
            }

            if (IsShifting)
            {
                return;
            }

            shiftSequence = DOTween.Sequence().SetTarget(this);

            for (int i = 0; i < slotBindings.Count; i++)
            {
                RectTransform slotRect = slotBindings[i].Rect;
                Vector2 targetPosition = slotRect.anchoredPosition;
                targetPosition.x -= slotSpacing;

                shiftSequence.Join(slotRect
                    .DOAnchorPos(targetPosition, GetShiftDurationSeconds())
                    .SetEase(shiftEase));
            }

            shiftSequence
                .OnComplete(() =>
                {
                    shiftsSinceLastSpawn++;

                    if (shiftsSinceLastSpawn >= spawnShiftThreshold)
                    {
                        RecycleLeftMostSlot();
                    }

                    shiftSequence = null;
                })
                .OnKill(() =>
                {
                    shiftSequence = null;
                });
        }

        private void RecycleLeftMostSlot()
        {
            SortSlotsByPosition();

            RoundSlotBinding leavingSlot = slotBindings[0];
            RoundSlotBinding rightMostSlot = slotBindings[slotBindings.Count - 1];

            int nextRoundValue = rightMostSlot.RoundValue + 1;

            Vector2 recycledPosition = leavingSlot.Rect.anchoredPosition;
            recycledPosition.x = ResolveRecycleSpawnX(rightMostSlot);
            leavingSlot.Rect.anchoredPosition = recycledPosition;

            leavingSlot.RoundValue = nextRoundValue;
            ApplySlotVisual(leavingSlot);

            SortSlotsByPosition();
        }
        #endregion

        #region Layout Helpers
        private void SortSlotsByPosition()
        {
            slotBindings.Sort((a, b) => a.Rect.anchoredPosition.x.CompareTo(b.Rect.anchoredPosition.x));
        }

        private float ResolveSlotSpacing()
        {
            if (slotBindings.Count < 2)
            {
                return DefaultSlotSpacing;
            }

            float minSpacing = float.MaxValue;
            for (int i = 1; i < slotBindings.Count; i++)
            {
                float diff = slotBindings[i].Rect.anchoredPosition.x - slotBindings[i - 1].Rect.anchoredPosition.x;
                if (diff > 0.01f && diff < minSpacing)
                {
                    minSpacing = diff;
                }
            }

            return minSpacing < float.MaxValue ? minSpacing : DefaultSlotSpacing;
        }

        private float ResolveRecycleSpawnX(RoundSlotBinding rightMostSlot)
        {
            return rightMostSlot.Rect.anchoredPosition.x + slotSpacing;
        }

        private int ResolveCenterIndex()
        {
            int centerIndex = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < slotBindings.Count; i++)
            {
                float distance = Mathf.Abs(slotBindings[i].Rect.anchoredPosition.x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    centerIndex = i;
                }
            }

            return centerIndex;
        }

        private float GetShiftDurationSeconds()
        {
            if (roundTrackTopSettings != null)
            {
                return Mathf.Max(0.05f, roundTrackTopSettings.ShiftDurationSeconds);
            }

            return DefaultShiftDurationSeconds;
        }
        #endregion

        #region Visual Helpers
        private void ApplySlotVisual(RoundSlotBinding slotBinding)
        {
            if (slotBinding.Text == null)
            {
                return;
            }

            slotBinding.Text.text = ResolveSlotText(slotBinding.RoundValue);
            slotBinding.Text.color = ResolveDefaultSlotTextColor(slotBinding.RoundValue);
        }

        private string ResolveSlotText(int roundValue)
        {
            if (roundValue < 1 || roundValue > WheelGameFlowManager.MaxRoundValue)
            {
                return string.Empty;
            }

            if (roundTrackTopSettings == null || string.IsNullOrWhiteSpace(roundTrackTopSettings.SlotTextFormat))
            {
                return roundValue.ToString();
            }

            try
            {
                return string.Format(roundTrackTopSettings.SlotTextFormat, roundValue);
            }
            catch (FormatException)
            {
                return roundValue.ToString();
            }
        }

        private void ApplyCurrentSlotTextColor(int levelValue, bool useIncomingSlot)
        {
            if (!initialized || slotBindings.Count == 0)
            {
                return;
            }

            SortSlotsByPosition();
            int targetIndex = ResolveCenterIndex();
            if (useIncomingSlot && slotBindings.Count > 1)
            {
                targetIndex = Mathf.Min(slotBindings.Count - 1, targetIndex + 1);
            }

            RoundSlotBinding currentSlotBinding = slotBindings[targetIndex];

            if (highlightedSlotBinding != null &&
                highlightedSlotBinding != currentSlotBinding &&
                highlightedSlotBinding.Text != null)
            {
                highlightedSlotBinding.Text.color = ResolveDefaultSlotTextColor(highlightedSlotBinding.RoundValue);
            }

            if (currentSlotBinding.Text != null)
            {
                currentSlotBinding.Text.color = ResolveCurrentSlotTextColor(levelValue);
            }

            highlightedSlotBinding = currentSlotBinding;
        }

        private Color ResolveDefaultSlotTextColor(int roundValue)
        {
            if (roundTrackTopSettings == null)
            {
                return Color.white;
            }

            int normalizedRound = Mathf.Max(1, roundValue);
            int goldenInterval = Mathf.Max(1, roundTrackTopSettings.GoldenRoundInterval);
            int silverInterval = Mathf.Max(1, roundTrackTopSettings.SilverRoundInterval);

            if (normalizedRound % goldenInterval == 0)
            {
                return roundTrackTopSettings.GoldenSlotTextColor;
            }

            if (normalizedRound == 1 || normalizedRound % silverInterval == 0)
            {
                return roundTrackTopSettings.SilverSlotTextColor;
            }

            return roundTrackTopSettings.BronzeSlotTextColor;
        }

        private Color ResolveCurrentSlotTextColor(int levelValue)
        {
            if (roundTrackTopSettings == null)
            {
                return Color.white;
            }

            int normalizedLevel = Mathf.Max(1, levelValue);
            int goldenInterval = Mathf.Max(1, roundTrackTopSettings.GoldenLevelInterval);
            int silverInterval = Mathf.Max(1, roundTrackTopSettings.SilverLevelInterval);

            if (normalizedLevel % goldenInterval == 0)
            {
                return roundTrackTopSettings.GoldenCurrentSlotTextColor;
            }

            if (normalizedLevel == 1 || normalizedLevel % silverInterval == 0)
            {
                return roundTrackTopSettings.SilverCurrentSlotTextColor;
            }

            return roundTrackTopSettings.NormalCurrentSlotTextColor;
        }

        private static int ResolveCurrentRound()
        {
            return WheelGameFlowManager.Instance != null ? WheelGameFlowManager.Instance.CurrentRound : 1;
        }

        private static int ResolveCurrentLevel()
        {
            return WheelGameFlowManager.Instance != null ? WheelGameFlowManager.Instance.CurrentLevel : 1;
        }

        private void TrySubscribeGameFlow()
        {
            if (isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                return;
            }

            WheelGameFlowManager.Instance.RoundChanged += HandleRoundChanged;
            WheelGameFlowManager.Instance.LevelChanged += HandleLevelChanged;
            isGameFlowSubscribed = true;

            InitializeTrack(WheelGameFlowManager.Instance.CurrentRound);
        }

        private void TryUnsubscribeGameFlow()
        {
            if (!Application.isPlaying || !isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                isGameFlowSubscribed = false;
                return;
            }

            WheelGameFlowManager.Instance.RoundChanged -= HandleRoundChanged;
            WheelGameFlowManager.Instance.LevelChanged -= HandleLevelChanged;
            isGameFlowSubscribed = false;
        }
        #endregion

        #region Nested Types
        private sealed class RoundSlotBinding
        {
            public RoundSlotBinding(RectTransform rect, TMP_Text text)
            {
                Rect = rect;
                Text = text;
            }

            public RectTransform Rect { get; }
            public TMP_Text Text { get; }
            public int RoundValue { get; set; }
        }
        #endregion
    }
}
