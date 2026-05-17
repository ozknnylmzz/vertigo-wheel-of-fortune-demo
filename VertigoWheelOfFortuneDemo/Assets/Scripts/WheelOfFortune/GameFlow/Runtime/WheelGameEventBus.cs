using System;

namespace Vertigo.WheelOfFortune.GameFlow.Runtime
{
    public static class WheelGameEventBus
    {
        #region Events
        public static event Action SpinStarted;
        public static event Action<float> SpinCompleted;
        public static event Action<int> LevelChanged;
        public static event Action<int> RoundChanged;
        public static event Action<WheelGameState> GameStateChanged;
        #endregion

        #region Last Known State
        public static int LastKnownLevel { get; private set; } = 1;
        public static int LastKnownRound { get; private set; } = 1;
        public static WheelGameState LastKnownGameState { get; private set; } = WheelGameState.Idle;
        #endregion

        #region Publishers
        public static void PublishSpinStarted()
        {
            SpinStarted?.Invoke();
        }

        public static void PublishSpinCompleted(float stopAngle)
        {
            SpinCompleted?.Invoke(stopAngle);
        }

        public static void PublishLevelChanged(int levelValue)
        {
            LastKnownLevel = levelValue < 1 ? 1 : levelValue;
            LevelChanged?.Invoke(LastKnownLevel);
        }

        public static void PublishRoundChanged(int roundValue)
        {
            LastKnownRound = roundValue < 1 ? 1 : roundValue;
            RoundChanged?.Invoke(LastKnownRound);
        }

        public static void PublishGameStateChanged(WheelGameState state)
        {
            LastKnownGameState = state;
            GameStateChanged?.Invoke(state);
        }
        #endregion
    }
}
