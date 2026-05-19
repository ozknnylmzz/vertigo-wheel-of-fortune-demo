using System;
using UnityEngine;

namespace Vertigo.WheelOfFortune.RoundTrack.Data
{
    [CreateAssetMenu(
        fileName = "round_track_marker_settings",
        menuName = "Vertigo/Wheel Of Fortune/Round Track Marker Settings")]
    public sealed class RoundTrackMarkerSettingsAsset : ScriptableObject
    {
        #region Nested Types
        [Serializable]
        public struct MarkerStageVisual
        {
            public Color backgroundColor;
        }
        #endregion

        #region Inspector Fields
        [Header("Stage Rules")]
        [SerializeField] [Min(1)] private int silverLevelInterval = 5;
        [SerializeField] [Min(1)] private int goldenLevelInterval = 30;

        [Header("Stage Visuals")]
        [SerializeField] private MarkerStageVisual normalStage = new MarkerStageVisual
        {
            backgroundColor = new Color(0.243f, 0.478f, 0.071f, 1f)
        };

        [SerializeField] private MarkerStageVisual silverStage = new MarkerStageVisual
        {
            backgroundColor = new Color(0.9339623f, 0.9339623f, 0.9339623f, 1f)
        };

        [SerializeField] private MarkerStageVisual goldenStage = new MarkerStageVisual
        {
            backgroundColor = new Color(0.7921569f, 0.61960787f, 0.34117648f, 1f)
        };
        #endregion

        #region Public API
        public MarkerStageVisual ResolveStageVisual(int levelValue)
        {
            int normalizedLevel = Mathf.Max(1, levelValue);
            int goldenInterval = Mathf.Max(1, goldenLevelInterval);
            int silverInterval = Mathf.Max(1, silverLevelInterval);

            if (normalizedLevel % goldenInterval == 0)
            {
                return goldenStage;
            }

            if (normalizedLevel == 1 || normalizedLevel % silverInterval == 0)
            {
                return silverStage;
            }

            return normalStage;
        }
        #endregion

        #region Validation
        private void OnValidate()
        {
            silverLevelInterval = Mathf.Max(1, silverLevelInterval);
            goldenLevelInterval = Mathf.Max(1, goldenLevelInterval);
        }
        #endregion
    }
}
