using UnityEngine;

namespace Vertigo.WheelOfFortune.SpinCenter.Data
{
    [CreateAssetMenu(
        fileName = "spin_center_spin_settings",
        menuName = "Vertigo/Wheel Of Fortune/Spin Center Spin Settings")]
    public sealed class SpinCenterSpinSettingsAsset : ScriptableObject
    {
        [Header("Spin")]
        [SerializeField] [Min(0.1f)] private float spinDurationSeconds = 2.6f;
        [SerializeField] [Min(1)] private int minSpinTurns = 4;
        [SerializeField] [Min(1)] private int maxSpinTurns = 7;
        [SerializeField] [Range(0f, 360f)] private float minExtraStopAngle = 0f;
        [SerializeField] [Range(0f, 360f)] private float maxExtraStopAngle = 360f;
        [SerializeField] private bool snapToSliceCenter = true;
        [SerializeField] [Min(2)] private int sliceCount = 8;

        [Header("Behavior")]
        [SerializeField] private bool blockButtonDuringSpin = true;
        [SerializeField] private AnimationCurve spinEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float SpinDurationSeconds => spinDurationSeconds;
        public int MinSpinTurns => minSpinTurns;
        public int MaxSpinTurns => maxSpinTurns;
        public float MinExtraStopAngle => minExtraStopAngle;
        public float MaxExtraStopAngle => maxExtraStopAngle;
        public bool SnapToSliceCenter => snapToSliceCenter;
        public int SliceCount => sliceCount;
        public bool BlockButtonDuringSpin => blockButtonDuringSpin;
        public AnimationCurve SpinEaseCurve => spinEaseCurve;

        private void OnValidate()
        {
            spinDurationSeconds = Mathf.Max(0.1f, spinDurationSeconds);
            minSpinTurns = Mathf.Max(1, minSpinTurns);
            maxSpinTurns = Mathf.Max(minSpinTurns, maxSpinTurns);
            minExtraStopAngle = Mathf.Clamp(minExtraStopAngle, 0f, 360f);
            maxExtraStopAngle = Mathf.Clamp(maxExtraStopAngle, minExtraStopAngle, 360f);
            sliceCount = Mathf.Max(2, sliceCount);

            if (spinEaseCurve == null || spinEaseCurve.length == 0)
            {
                spinEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
    }
}
