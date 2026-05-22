using System;
using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Rewards.Data;
using Vertigo.WheelOfFortune.SpinCenter.Data;
using Vertigo.WheelOfFortune.SpinCenter.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vertigo.WheelOfFortune.SpinCenter.Runtime
{
    public sealed class SpinCenterSetup : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private SpinCenterConfigAsset spinCenterConfig;

        [Header("Current Selection")]
        [SerializeField] [Range(1, SpinCenterConfigAsset.MaxLevel)] private int levelValue = 1;

        [Header("View")]
        [SerializeField] private SpinCenterView spinCenterView;

        public event Action<SpinCenterTierVisualData> SpinCenterVisualApplied;

#if UNITY_EDITOR
        private bool editorPreviewApplyQueued;
#endif

        private void Awake()
        {
            ApplyCurrentSelection();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (WheelGameFlowManager.Instance == null)
            {
                return;
            }

            WheelGameFlowManager.Instance.LevelChanged += HandleLevelChanged;
            HandleLevelChanged(WheelGameFlowManager.Instance.CurrentLevel);
        }

        private void OnDisable()
        {
            if (Application.isPlaying && WheelGameFlowManager.Instance != null)
            {
                WheelGameFlowManager.Instance.LevelChanged -= HandleLevelChanged;
            }

#if UNITY_EDITOR
            if (editorPreviewApplyQueued)
            {
                EditorApplication.delayCall -= ApplyPreviewFromDelayCall;
                editorPreviewApplyQueued = false;
            }
#endif
        }

        private void OnValidate()
        {
            if (spinCenterView == null)
            {
                spinCenterView = GetComponent<SpinCenterView>();
                if (spinCenterView == null)
                {
                    spinCenterView = GetComponentInChildren<SpinCenterView>(true);
                }
            }

            levelValue = Mathf.Clamp(levelValue, 1, SpinCenterConfigAsset.MaxLevel);

            if (!isActiveAndEnabled || spinCenterConfig == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueEditorPreviewApply();
                return;
            }
#endif

            ApplyCurrentSelection();
        }

        [ContextMenu("Apply Current Selection")]
        public void ApplyCurrentSelection()
        {
            if (spinCenterConfig == null || spinCenterView == null)
            {
                return;
            }

            SpinCenterTierVisualData visualData = spinCenterConfig.GenerateSlicesForLevel(levelValue, spinCenterView.SliceViewCount);
            ResolveSliceAmounts(visualData, levelValue);
            spinCenterView.gameObject.SetActive(true);
            spinCenterView.ApplyVisualData(visualData);
            SpinCenterVisualApplied?.Invoke(visualData);
        }

        public void SetLevel(int level)
        {
            levelValue = Mathf.Clamp(level, 1, SpinCenterConfigAsset.MaxLevel);
            ApplyCurrentSelection();
        }

        private void HandleLevelChanged(int level)
        {
            SetLevel(level);
        }

        private static void ResolveSliceAmounts(SpinCenterTierVisualData visualData, int level)
        {
            if (visualData.slices == null)
            {
                return;
            }

            for (int i = 0; i < visualData.slices.Count; i++)
            {
                SpinCenterSliceVisualData sliceData = visualData.slices[i];
                if (sliceData != null)
                {
                    sliceData.selectedRewardAmountValue =
                        WheelRewardAmountResolver.Resolve(level, sliceData.rewardType);
                }
            }
        }

#if UNITY_EDITOR
        private void QueueEditorPreviewApply()
        {
            if (editorPreviewApplyQueued)
            {
                return;
            }

            editorPreviewApplyQueued = true;
            EditorApplication.delayCall += ApplyPreviewFromDelayCall;
        }

        private void ApplyPreviewFromDelayCall()
        {
            EditorApplication.delayCall -= ApplyPreviewFromDelayCall;
            editorPreviewApplyQueued = false;

            if (this == null || !isActiveAndEnabled || spinCenterConfig == null)
            {
                return;
            }

            ApplyCurrentSelection();
        }
#endif
    }
}
