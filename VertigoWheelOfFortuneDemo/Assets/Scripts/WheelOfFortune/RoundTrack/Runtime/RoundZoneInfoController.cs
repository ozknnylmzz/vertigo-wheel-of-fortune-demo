using TMPro;
using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.RoundTrack.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vertigo.WheelOfFortune.RoundTrack.Runtime
{
    public sealed class RoundZoneInfoController : MonoBehaviour
    {
        #region Constants
        private const string SafeValueTextObjectName = "ui_text_zone_safe_value";
        private const string SuperValueTextObjectName = "ui_text_zone_super_value";
        private const string DefaultSettingsPath =
            "Assets/Scripts/WheelOfFortune/RoundTrack/Data/SO/round_zone_info_settings.asset";
        #endregion

        #region Inspector Fields
        [Header("Settings")]
        [SerializeField] private RoundZoneInfoSettingsAsset zoneSettings;

        [Header("Zone Value Texts")]
        [SerializeField] private TMP_Text ui_text_zone_safe_value;
        [SerializeField] private TMP_Text ui_text_zone_super_value;
        #endregion

        #region Runtime State
        private bool isGameFlowSubscribed;
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            zoneSettings ??= AssetDatabase.LoadAssetAtPath<RoundZoneInfoSettingsAsset>(DefaultSettingsPath);
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
            ApplyZoneTexts(ResolveCurrentProgressValue());
            TrySubscribeGameFlow();
        }

        private void Start()
        {
            ApplyZoneTexts(ResolveCurrentProgressValue());
            TrySubscribeGameFlow();
        }

        private void OnDisable()
        {
            TryUnsubscribeGameFlow();
        }
        #endregion

        #region Event Handlers
        private void HandleRoundChanged(int _)
        {
            ApplyZoneTexts(ResolveCurrentProgressValue());
        }

        private void HandleLevelChanged(int _)
        {
            ApplyZoneTexts(ResolveCurrentProgressValue());
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
            int nextSafeRound = ResolveNextSafeZoneRound(normalizedRound);
            int nextSuperRound = ResolveNextSuperZoneRound(normalizedRound);

            if (ui_text_zone_safe_value != null)
            {
                ui_text_zone_safe_value.text = nextSafeRound.ToString();
            }

            if (ui_text_zone_super_value != null)
            {
                ui_text_zone_super_value.text = nextSuperRound.ToString();
            }
        }

        private static int ResolveCurrentProgressValue()
        {
            if (WheelGameFlowManager.Instance == null)
            {
                return 1;
            }

            return Mathf.Max(WheelGameFlowManager.Instance.CurrentRound, WheelGameFlowManager.Instance.CurrentLevel);
        }

        private int ResolveNextSafeZoneRound(int roundValue)
        {
            if (zoneSettings != null)
            {
                return zoneSettings.ResolveNextSafeZoneRound(roundValue);
            }

            int safeInterval = 5;
            int superInterval = 30;
            int maxSafeRound = Mathf.Max(safeInterval, WheelGameFlowManager.MaxRoundValue - safeInterval);
            int nextSafeRound = ((roundValue / safeInterval) + 1) * safeInterval;
            if (nextSafeRound < WheelGameFlowManager.MaxRoundValue && nextSafeRound % superInterval == 0)
            {
                nextSafeRound += safeInterval;
            }

            return Mathf.Min(nextSafeRound, maxSafeRound);
        }

        private int ResolveNextSuperZoneRound(int roundValue)
        {
            if (zoneSettings != null)
            {
                return zoneSettings.ResolveNextSuperZoneRound(roundValue);
            }

            int superInterval = 30;
            int nextSuperRound = ((roundValue / superInterval) + 1) * superInterval;
            return Mathf.Min(nextSuperRound, WheelGameFlowManager.MaxRoundValue);
        }

        private void TrySubscribeGameFlow()
        {
            if (!Application.isPlaying || isGameFlowSubscribed || WheelGameFlowManager.Instance == null)
            {
                return;
            }

            WheelGameFlowManager.Instance.RoundChanged += HandleRoundChanged;
            WheelGameFlowManager.Instance.LevelChanged += HandleLevelChanged;
            isGameFlowSubscribed = true;
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
    }
}
