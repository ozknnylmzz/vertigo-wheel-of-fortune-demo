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
        #region Constants
        public const int MaxRoundValue = 60;
        #endregion

        #region Inspector Fields
        [SerializeField] [Min(1)] private int level_value = 1;
        [SerializeField] [Min(1)] private int round_value = 1;
        [SerializeField] [Min(1)] private int max_level_value = 60;
        [SerializeField] private WheelGameState game_state = WheelGameState.Idle;
        #endregion

        #region Singleton
        public static WheelGameFlowManager Instance { get; private set; }
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
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple WheelGameFlowManager instances found. Keeping the first instance.", this);
                return;
            }

            Instance = this;
        }

        private void OnValidate()
        {
            max_level_value = Mathf.Max(1, max_level_value);
            level_value = Mathf.Clamp(level_value, 1, max_level_value);
            round_value = Mathf.Clamp(round_value, 1, MaxRoundValue);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Instance != this)
            {
                return;
            }

            PublishCurrentState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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
            return level_value;
        }

        public void SetRound(int round)
        {
            int normalizedRound = Mathf.Clamp(round, 1, MaxRoundValue);
            if (normalizedRound == round_value)
            {
                return;
            }

            round_value = normalizedRound;
            RoundChanged?.Invoke(round_value);
        }

        public int IncrementRound()
        {
            int updatedRound = Mathf.Min(round_value + 1, MaxRoundValue);
            if (updatedRound == round_value)
            {
                return round_value;
            }

            round_value = updatedRound;
            RoundChanged?.Invoke(round_value);
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
        }

        public void BeginSpin()
        {
            if (game_state == WheelGameState.Win)
            {
                return;
            }

            SetGameState(WheelGameState.Spinning);
        }

        public void CompleteRewardCollection()
        {
            if (game_state != WheelGameState.Spinning)
            {
                return;
            }

            if (round_value >= MaxRoundValue)
            {
                CompleteSpinResult();
                return;
            }

            IncrementRound();
            IncrementLevel();
            if (IsStageRewardLevel(level_value) && WheelGameEventBus.PublishStageRewardPopupRequested(level_value))
            {
                return;
            }

            CompleteSpinResult();
        }

        public void CompleteStageReward()
        {
            if (game_state == WheelGameState.Spinning)
            {
                SetGameState(WheelGameState.Idle);
            }
        }

        public void HitBomb()
        {
            SetGameState(WheelGameState.Lose);
        }

        public void ContinueAfterBomb()
        {
            if (game_state == WheelGameState.Lose)
            {
                SetGameState(WheelGameState.Idle);
            }
        }

        public void RestartGame()
        {
            SetRound(1);
            SetLevel(1);
            SetGameState(WheelGameState.Idle);
        }
        #endregion

        #region Helpers
        private void PublishCurrentState()
        {
            LevelChanged?.Invoke(level_value);
            RoundChanged?.Invoke(round_value);
            GameStateChanged?.Invoke(game_state);
        }

        private void CompleteSpinResult()
        {
            SetGameState(round_value >= MaxRoundValue ? WheelGameState.Win : WheelGameState.Idle);
        }

        private static bool IsStageRewardLevel(int level)
        {
            return level == 30 || level == MaxRoundValue;
        }
        #endregion
    }
}
