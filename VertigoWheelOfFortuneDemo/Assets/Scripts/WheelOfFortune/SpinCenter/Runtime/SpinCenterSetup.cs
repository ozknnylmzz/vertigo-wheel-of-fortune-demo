using System;
using UnityEngine;
using Vertigo.WheelOfFortune.SpinCenter.Data;
using Vertigo.WheelOfFortune.SpinCenter.UI;

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

        private void Awake()
        {
            ApplyCurrentSelection();
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

            if (!isActiveAndEnabled)
            {
                return;
            }

            ApplyCurrentSelection();
        }

        [ContextMenu("Apply Current Selection")]
        public void ApplyCurrentSelection()
        {
            if (spinCenterConfig == null || spinCenterView == null)
            {
                return;
            }

            SpinCenterTierVisualData visualData = spinCenterConfig.ResolveByLevelOrThrow(levelValue);
            spinCenterView.gameObject.SetActive(true);
            spinCenterView.ApplyVisualData(visualData);
            SpinCenterVisualApplied?.Invoke(visualData);
        }

        public void SetLevel(int level)
        {
            levelValue = Mathf.Clamp(level, 1, SpinCenterConfigAsset.MaxLevel);
            ApplyCurrentSelection();
        }

        public void SetSelection(int level, SpinCenterTier tier)
        {
            _ = tier;
            SetLevel(level);
        }

        public void SetSelectionByLevelRule(int level)
        {
            SetLevel(level);
        }
    }
}
