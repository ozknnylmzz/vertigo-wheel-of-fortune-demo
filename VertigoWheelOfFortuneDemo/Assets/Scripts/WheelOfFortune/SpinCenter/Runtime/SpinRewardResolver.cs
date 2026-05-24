using UnityEngine;
using Vertigo.WheelOfFortune.GameFlow.Runtime;
using Vertigo.WheelOfFortune.Rewards.Data;
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
            SpinCenterTierVisualData tierData = ResolveTierData();
            if (tierData == null || tierData.slices == null || tierData.slices.Count == 0)
            {
                CompleteRewardCollection();
                return;
            }

            int sliceIndex = ResolveSliceIndex(stopAngle, tierData.slices.Count);
            SpinCenterSliceVisualData rewardData = tierData.slices[sliceIndex];
            if (rewardData == null)
            {
                CompleteRewardCollection();
                return;
            }

            string rewardAmountValue = ResolveRewardAmountValue(rewardData);
            if (rewardData.rewardType == WheelRewardType.Bomb)
            {
                WheelGameFlowManager.Instance?.HitBomb();
                return;
            }

            bool rewardHandled = WheelGameEventBus.PublishRewardWon(new WheelRewardData(
                rewardData.rewardType,
                rewardData.rewardIcon,
                rewardAmountValue));

            if (!rewardHandled)
            {
                CompleteRewardCollection();
            }
        }
        #endregion

        #region Reward Resolve
        private SpinCenterTierVisualData ResolveTierData()
        {
            if (spinCenterConfig == null)
            {
                return null;
            }

            int level = ResolveCurrentLevel();
            SpinCenterTierVisualData tierData = spinCenterConfig.ResolveByLevelOrThrow(level);
            return tierData.slices != null && tierData.slices.Count > 0
                ? tierData
                : spinCenterConfig.GenerateSlicesForLevel(level);
        }

        private static int ResolveSliceIndex(float stopAngle, int sliceCount)
        {
            float sliceAngle = 360f / Mathf.Max(1, sliceCount);
            float winningLocalAngle = Mathf.Repeat(IndicatorAngle - stopAngle, 360f);
            float indexAngle = Mathf.Repeat(FirstSliceAngle - winningLocalAngle, 360f);

            return Mathf.RoundToInt(indexAngle / sliceAngle) % sliceCount;
        }

        private static string ResolveRewardAmountValue(SpinCenterSliceVisualData rewardData)
        {
            return !string.IsNullOrWhiteSpace(rewardData.selectedRewardAmountValue)
                ? rewardData.selectedRewardAmountValue
                : WheelRewardAmountResolver.Resolve(ResolveCurrentLevel(), rewardData.rewardType);
        }

        private static int ResolveCurrentLevel()
        {
            return WheelGameFlowManager.Instance != null ? WheelGameFlowManager.Instance.CurrentLevel : 1;
        }

        private static void CompleteRewardCollection()
        {
            WheelGameFlowManager.Instance?.CompleteRewardCollection();
        }
        #endregion
    }
}
