using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.RoundTrack.Data;

namespace Vertigo.WheelOfFortune.RoundTrack.Runtime
{
    public sealed class RoundCurrentMarkerController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Marker References")]
        [SerializeField] private Image ui_image_round_current_marker_bg;

        [SerializeField] private RoundTrackMarkerSettingsAsset markerSettings;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            ApplyVisual(WheelGameEventBus.LastKnownLevel);

            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.LevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.LevelChanged -= HandleLevelChanged;
        }
        #endregion

        #region Event Handlers
        private void HandleLevelChanged(int levelValue)
        {
            ApplyVisual(levelValue);
        }
        #endregion

        #region Visuals
        private void ApplyVisual(int levelValue)
        {
            if (markerSettings == null)
            {
                return;
            }

            RoundTrackMarkerSettingsAsset.MarkerStageVisual stageVisual =
                markerSettings.ResolveStageVisual(levelValue);

            if (ui_image_round_current_marker_bg != null)
            {
                ui_image_round_current_marker_bg.color = stageVisual.backgroundColor;
            }
        }
        #endregion
    }
}
