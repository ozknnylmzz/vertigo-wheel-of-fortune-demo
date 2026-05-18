using System;
using UnityEngine;

namespace Vertigo.WheelOfFortune.GameFlow.Runtime
{
    public enum WheelGameState
    {
        Idle = 0,
        Spinning = 1,
        Win = 2,
        Lose = 3
    }

    public sealed class WheelGameFlowManager : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] [Min(1)] private int level_value = 1;
        [SerializeField] [Min(1)] private int round_value = 1;
        [SerializeField] [Min(1)] private int max_level_value = 60;
        [SerializeField] private WheelGameState game_state = WheelGameState.Idle;
        #endregion

        #region Properties
        public int CurrentLevel => level_value;
        public int CurrentRound => round_value;
        public WheelGameState CurrentState => game_state;
        #endregion

        #region Events
        public event Action<int> LevelChanged;
        public event Action<int> RoundChanged;
        public event Action<WheelGameState> GameStateChanged;
        #endregion

        #region Unity Lifecycle
        private void OnValidate()
        {
            max_level_value = Mathf.Max(1, max_level_value);
            level_value = Mathf.Clamp(level_value, 1, max_level_value);
            round_value = Mathf.Clamp(round_value, 1, WheelGameEventBus.MaxRoundValue);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.SpinStarted += HandleSpinStarted;
            WheelGameEventBus.SpinCompleted += HandleSpinCompleted;
            WheelGameEventBus.PublishLevelChanged(level_value);
            WheelGameEventBus.PublishRoundChanged(round_value);
            WheelGameEventBus.PublishGameStateChanged(game_state);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            WheelGameEventBus.SpinStarted -= HandleSpinStarted;
            WheelGameEventBus.SpinCompleted -= HandleSpinCompleted;
        }
        #endregion

        #region Public API
        public void SetLevel(int level)
        {
            int normalizedLevel = Mathf.Clamp(level, 1, max_level_value);
            if (normalizedLevel == level_value)
            {
                return;
            }

            level_value = normalizedLevel;
            LevelChanged?.Invoke(level_value);
            WheelGameEventBus.PublishLevelChanged(level_value);
        }

        public int IncrementLevel()
        {
            int updatedLevel = level_value < max_level_value ? level_value + 1 : max_level_value;
            if (updatedLevel == level_value)
            {
                return level_value;
            }

            level_value = updatedLevel;
            LevelChanged?.Invoke(level_value);
            WheelGameEventBus.PublishLevelChanged(level_value);
            return level_value;
        }

        public void SetRound(int round)
        {
            int normalizedRound = Mathf.Clamp(round, 1, WheelGameEventBus.MaxRoundValue);
            if (normalizedRound == round_value)
            {
                return;
            }

            round_value = normalizedRound;
            RoundChanged?.Invoke(round_value);
            WheelGameEventBus.PublishRoundChanged(round_value);
        }

        public int IncrementRound()
        {
            int updatedRound = Mathf.Min(round_value + 1, WheelGameEventBus.MaxRoundValue);
            if (updatedRound == round_value)
            {
                return round_value;
            }

            round_value = updatedRound;
            RoundChanged?.Invoke(round_value);
            WheelGameEventBus.PublishRoundChanged(round_value);
            return round_value;
        }

        public void SetGameState(WheelGameState state)
        {
            if (game_state == state)
            {
                return;
            }

            game_state = state;
            GameStateChanged?.Invoke(game_state);
            WheelGameEventBus.PublishGameStateChanged(game_state);
        }
        #endregion

        #region Event Bus Handlers
        private void HandleSpinStarted()
        {
            if (round_value >= WheelGameEventBus.MaxRoundValue)
            {
                SetGameState(WheelGameState.Win);
                return;
            }

            IncrementLevel();
            IncrementRound();
            SetGameState(WheelGameState.Spinning);
        }

        private void HandleSpinCompleted(float _)
        {
            SetGameState(round_value >= WheelGameEventBus.MaxRoundValue ? WheelGameState.Win : WheelGameState.Idle);
        }
        #endregion
    }
}
