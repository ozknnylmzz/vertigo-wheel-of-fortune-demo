using System;
using Vertigo.WheelOfFortune.Rewards.Runtime;

namespace Vertigo.WheelOfFortune.GameFlow.Runtime
{
    public static class WheelGameEventBus
    {
        #region Events
        public static event Action<float> SpinCompleted;
        public static event Action<WheelRewardData> RewardWon;
        public static event Action<WheelRewardData> RewardWonObserved;
        public static event Action RewardsResetRequested;
        public static event Action CashOutConfirmRequested;
        public static event Action ExitGameConfirmRequested;
        public static event Action<int> StageRewardPopupRequested;
        #endregion

        #region Publishers
        public static void PublishSpinCompleted(float stopAngle)
        {
            SpinCompleted?.Invoke(stopAngle);
        }

        public static bool PublishRewardWon(WheelRewardData rewardData)
        {
            bool hasRewardHandler = RewardWon != null;

            RewardWon?.Invoke(rewardData);
            RewardWonObserved?.Invoke(rewardData);
            return hasRewardHandler;
        }

        public static void PublishCashOutConfirmRequested()
        {
            CashOutConfirmRequested?.Invoke();
        }

        public static void PublishExitGameConfirmRequested()
        {
            ExitGameConfirmRequested?.Invoke();
        }

        public static bool PublishStageRewardPopupRequested(int level)
        {
            if (StageRewardPopupRequested == null)
            {
                return false;
            }

            StageRewardPopupRequested.Invoke(level);
            return true;
        }

        public static void PublishRewardsResetRequested()
        {
            RewardsResetRequested?.Invoke();
        }
        #endregion
    }
}
