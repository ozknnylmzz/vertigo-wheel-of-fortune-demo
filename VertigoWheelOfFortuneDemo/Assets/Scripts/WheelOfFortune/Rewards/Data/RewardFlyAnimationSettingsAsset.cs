using UnityEngine;

namespace Vertigo.WheelOfFortune.Rewards.Data
{
    [CreateAssetMenu(
        fileName = "reward_fly_animation_settings",
        menuName = "Vertigo/Wheel Of Fortune/Reward Fly Animation Settings")]
    public sealed class RewardFlyAnimationSettingsAsset : ScriptableObject
    {
        #region Inspector Fields
        [SerializeField] [Min(0f)] private float scatterRadius = 40f;
        [SerializeField] [Min(0.01f)] private float scatterDurationSeconds = 0.2f;
        [SerializeField] [Min(0.01f)] private float flyDurationSeconds = 0.45f;
        [SerializeField] [Min(0f)] private float iconDelaySeconds = 0.04f;
        #endregion

        #region Properties
        public float ScatterRadius => scatterRadius;
        public float ScatterDurationSeconds => scatterDurationSeconds;
        public float FlyDurationSeconds => flyDurationSeconds;
        public float IconDelaySeconds => iconDelaySeconds;
        #endregion

        #region Validation
        private void OnValidate()
        {
            scatterRadius = Mathf.Max(0f, scatterRadius);
            scatterDurationSeconds = Mathf.Max(0.01f, scatterDurationSeconds);
            flyDurationSeconds = Mathf.Max(0.01f, flyDurationSeconds);
            iconDelaySeconds = Mathf.Max(0f, iconDelaySeconds);
        }
        #endregion
    }
}
