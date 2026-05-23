using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;

namespace Vertigo.WheelOfFortune.RoundTrack.Data
{
    [CreateAssetMenu(
        fileName = "round_zone_info_settings",
        menuName = "Vertigo/Wheel Of Fortune/Round Zone Info Settings")]
    public sealed class RoundZoneInfoSettingsAsset : ScriptableObject
    {
        #region Inspector Fields
        [Header("Rules")]
        [SerializeField] [Min(1)] private int safeZoneInterval = 5;
        [SerializeField] [Min(1)] private int superZoneInterval = 30;
        #endregion

        #region Properties
        public int SafeZoneInterval => Mathf.Max(1, safeZoneInterval);
        public int SuperZoneInterval => Mathf.Max(1, superZoneInterval);
        #endregion

        #region Public API
        public int ResolveNextSafeZoneRound(int roundValue)
        {
            int normalizedRound = Mathf.Clamp(roundValue, 0, WheelGameFlowManager.MaxRoundValue);
            int safeInterval = SafeZoneInterval;
            int superInterval = SuperZoneInterval;
            int maxSafeRound = Mathf.Max(safeInterval, WheelGameFlowManager.MaxRoundValue - safeInterval);

            int nextSafeRound = ((normalizedRound / safeInterval) + 1) * safeInterval;
            if (nextSafeRound < WheelGameFlowManager.MaxRoundValue && nextSafeRound % superInterval == 0)
            {
                nextSafeRound += safeInterval;
            }

            return Mathf.Min(nextSafeRound, maxSafeRound);
        }

        public int ResolveNextSuperZoneRound(int roundValue)
        {
            int normalizedRound = Mathf.Clamp(roundValue, 0, WheelGameFlowManager.MaxRoundValue);
            int nextSuperRound = ((normalizedRound / SuperZoneInterval) + 1) * SuperZoneInterval;
            return Mathf.Min(nextSuperRound, WheelGameFlowManager.MaxRoundValue);
        }
        #endregion

        #region Validation
        private void OnValidate()
        {
            safeZoneInterval = Mathf.Max(1, safeZoneInterval);
            superZoneInterval = Mathf.Max(1, superZoneInterval);
        }
        #endregion
    }
}
