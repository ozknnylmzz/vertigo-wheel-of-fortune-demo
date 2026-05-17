using UnityEngine;

namespace Vertigo.WheelOfFortune.RoundTrack.Data
{
    [CreateAssetMenu(
        fileName = "round_track_top_settings",
        menuName = "Vertigo/Wheel Of Fortune/Round Track Top Settings")]
    public sealed class RoundTrackTopSettingsAsset : ScriptableObject
    {
        #region Inspector Fields
        [Header("Shift")]
        [SerializeField] [Min(0.05f)] private float shiftDurationSeconds = 0.25f;

        [Header("Slot Text")]
        [SerializeField] private string slotTextFormat = "{0}";

        [Header("Default Slot Text Rules")]
        [SerializeField] [Min(1)] private int silverRoundInterval = 5;
        [SerializeField] [Min(1)] private int goldenRoundInterval = 30;

        [Header("Default Slot Text Colors")]
        [SerializeField] private Color bronzeSlotTextColor = Color.white;
        [SerializeField] private Color silverSlotTextColor = Color.white;
        [SerializeField] private Color goldenSlotTextColor = Color.white;

        [Header("Current Slot Text Rules")]
        [SerializeField] [Min(1)] private int silverLevelInterval = 5;
        [SerializeField] [Min(1)] private int goldenLevelInterval = 30;

        [Header("Current Slot Text Colors")]
        [SerializeField] private Color normalCurrentSlotTextColor = Color.white;
        [SerializeField] private Color silverCurrentSlotTextColor = Color.white;
        [SerializeField] private Color goldenCurrentSlotTextColor = Color.white;
        #endregion

        #region Properties
        public float ShiftDurationSeconds => shiftDurationSeconds;
        public string SlotTextFormat => slotTextFormat;
        public int SilverRoundInterval => silverRoundInterval;
        public int GoldenRoundInterval => goldenRoundInterval;
        public Color BronzeSlotTextColor => bronzeSlotTextColor;
        public Color SilverSlotTextColor => silverSlotTextColor;
        public Color GoldenSlotTextColor => goldenSlotTextColor;
        public int SilverLevelInterval => silverLevelInterval;
        public int GoldenLevelInterval => goldenLevelInterval;
        public Color NormalCurrentSlotTextColor => normalCurrentSlotTextColor;
        public Color SilverCurrentSlotTextColor => silverCurrentSlotTextColor;
        public Color GoldenCurrentSlotTextColor => goldenCurrentSlotTextColor;
        #endregion

        #region Validation
        private void OnValidate()
        {
            shiftDurationSeconds = Mathf.Max(0.05f, shiftDurationSeconds);
            silverRoundInterval = Mathf.Max(1, silverRoundInterval);
            goldenRoundInterval = Mathf.Max(1, goldenRoundInterval);
            silverLevelInterval = Mathf.Max(1, silverLevelInterval);
            goldenLevelInterval = Mathf.Max(1, goldenLevelInterval);

            if (string.IsNullOrWhiteSpace(slotTextFormat))
            {
                slotTextFormat = "{0}";
            }
        }
        #endregion
    }
}
