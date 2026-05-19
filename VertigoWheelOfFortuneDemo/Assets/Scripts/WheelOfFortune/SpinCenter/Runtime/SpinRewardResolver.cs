using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Rewards.Runtime;
using Vertigo.WheelOfFortune.SpinCenter.Data;

namespace Vertigo.WheelOfFortune.SpinCenter.Runtime
{
    public sealed class SpinRewardResolver : MonoBehaviour
    {
        #region Constants
        private const float IndicatorAngle = 90f;
        private const float FirstSliceAngle = 180f;
        #endregion

        #region Inspector Fields
        [SerializeField] private SpinCenterConfigAsset spinCenterConfig;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.SpinCompleted += HandleSpinCompleted;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.SpinCompleted -= HandleSpinCompleted;
        }
        #endregion

        #region Event Handlers
        private void HandleSpinCompleted(float stopAngle)
        {
            SpinCenterSliceVisualData rewardData = ResolveRewardData(stopAngle);
            if (rewardData == null)
            {
                return;
            }

            WheelGameEventBus.PublishRewardWon(new WheelRewardData(
                ResolveRewardKey(rewardData),
                rewardData.rewardIcon,
                rewardData.rewardAmountValue));
        }
        #endregion

        #region Reward Resolve
        private SpinCenterSliceVisualData ResolveRewardData(float stopAngle)
        {
            if (spinCenterConfig == null)
            {
                return null;
            }

            SpinCenterTierVisualData tierData =
                spinCenterConfig.ResolveByLevelOrThrow(WheelGameEventBus.LastKnownLevel);

            if (tierData.slices == null || tierData.slices.Count == 0)
            {
                return null;
            }

            int sliceIndex = ResolveSliceIndex(stopAngle, tierData.slices.Count);
            return tierData.slices[sliceIndex];
        }

        private static int ResolveSliceIndex(float stopAngle, int sliceCount)
        {
            float sliceAngle = 360f / Mathf.Max(1, sliceCount);
            float winningLocalAngle = Mathf.Repeat(IndicatorAngle - stopAngle, 360f);
            float indexAngle = Mathf.Repeat(FirstSliceAngle - winningLocalAngle, 360f);

            return Mathf.RoundToInt(indexAngle / sliceAngle) % sliceCount;
        }

        private static string ResolveRewardKey(SpinCenterSliceVisualData rewardData)
        {
            if (rewardData.rewardIcon != null)
            {
                return rewardData.rewardIcon.name;
            }

            return rewardData.sliceId;
        }
        #endregion
    }
}
