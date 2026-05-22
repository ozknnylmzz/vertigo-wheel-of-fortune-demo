using TMPro;
using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;

namespace Vertigo.WheelOfFortune.RoundTrack.Runtime
{
    public sealed class RoundZoneInfoController : MonoBehaviour
    {
        #region Constants
        private const string SafeValueTextObjectName = "ui_text_zone_safe_value";
        private const string SuperValueTextObjectName = "ui_text_zone_super_value";
        #endregion

        #region Inspector Fields
        [Header("Zone Value Texts")]
        [SerializeField] private TMP_Text ui_text_zone_safe_value;
        [SerializeField] private TMP_Text ui_text_zone_super_value;

        [Header("Rules")]
        [SerializeField] [Min(1)] private int safeZoneInterval = 5;
        [SerializeField] [Min(1)] private int superZoneInterval = 30;
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            safeZoneInterval = Mathf.Max(1, safeZoneInterval);
            superZoneInterval = Mathf.Max(1, superZoneInterval);
            TryAutoAssignZoneValueTexts();
        }

        private void TryAutoAssignZoneValueTexts()
        {
            if (ui_text_zone_safe_value != null && ui_text_zone_super_value != null)
            {
                return;
            }

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text == null)
                {
                    continue;
                }

                if (ui_text_zone_safe_value == null && text.name == SafeValueTextObjectName)
                {
                    ui_text_zone_safe_value = text;
                    continue;
                }

                if (ui_text_zone_super_value == null && text.name == SuperValueTextObjectName)
                {
                    ui_text_zone_super_value = text;
                }
            }
        }
#endif
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                TryAutoAssignZoneValueTexts();
            }
#endif
            ApplyZoneTexts(ResolveCurrentRound());

            if (!Application.isPlaying)
            {
                return;
            }

            if (WheelGameFlowManager.Instance != null)
            {
                WheelGameFlowManager.Instance.RoundChanged += HandleRoundChanged;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (WheelGameFlowManager.Instance != null)
            {
                WheelGameFlowManager.Instance.RoundChanged -= HandleRoundChanged;
            }
        }
        #endregion

        #region Event Handlers
        private void HandleRoundChanged(int roundValue)
        {
            ApplyZoneTexts(roundValue);
        }
        #endregion

        #region Visuals
        private void ApplyZoneTexts(int roundValue)
        {
            if (roundValue > WheelGameFlowManager.MaxRoundValue)
            {
                return;
            }

            int normalizedRound = Mathf.Clamp(roundValue, 0, WheelGameFlowManager.MaxRoundValue);
            int safeInterval = Mathf.Max(1, safeZoneInterval);
            int superInterval = Mathf.Max(1, superZoneInterval);
            int maxSafeRound = Mathf.Max(safeInterval, WheelGameFlowManager.MaxRoundValue - safeInterval);

            int nextSafeRound = ((normalizedRound / safeInterval) + 1) * safeInterval;
            if (nextSafeRound < WheelGameFlowManager.MaxRoundValue && nextSafeRound % superInterval == 0)
            {
                nextSafeRound += safeInterval;
            }

            int nextSuperRound = ((normalizedRound / superInterval) + 1) * superInterval;
            nextSafeRound = Mathf.Min(nextSafeRound, maxSafeRound);
            nextSuperRound = Mathf.Min(nextSuperRound, WheelGameFlowManager.MaxRoundValue);

            if (ui_text_zone_safe_value != null)
            {
                ui_text_zone_safe_value.text = nextSafeRound.ToString();
            }

            if (ui_text_zone_super_value != null)
            {
                ui_text_zone_super_value.text = nextSuperRound.ToString();
            }
        }

        private static int ResolveCurrentRound()
        {
            return WheelGameFlowManager.Instance != null ? WheelGameFlowManager.Instance.CurrentRound : 1;
        }
        #endregion
    }
}
