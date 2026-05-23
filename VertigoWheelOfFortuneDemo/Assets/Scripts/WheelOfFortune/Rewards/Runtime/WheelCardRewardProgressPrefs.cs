using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelOfFortune.Rewards.Runtime
{
    public static class WheelCardRewardProgressPrefs
    {
        #region Constants
        private const string PlayerPrefsKey = "Vertigo.WheelOfFortune.CardRewardProgress";
        #endregion

        #region Public API
        public static WheelCardRewardProgressChange AddProgress(string cardKey, int amount, int requiredAmount)
        {
            int safeRequiredAmount = Mathf.Max(1, requiredAmount);
            WheelCardRewardProgressSaveData saveData = Load();
            WheelCardRewardProgressEntry entry = saveData.GetOrCreate(cardKey);
            int previousTotalAmount = Mathf.Max(0, entry.totalAmount);
            entry.totalAmount = previousTotalAmount + Mathf.Max(0, amount);
            Save(saveData);

            return new WheelCardRewardProgressChange(
                WheelCardRewardProgress.FromTotalAmount(previousTotalAmount, safeRequiredAmount),
                WheelCardRewardProgress.FromTotalAmount(entry.totalAmount, safeRequiredAmount));
        }
        #endregion

        #region Private Methods
        private static WheelCardRewardProgressSaveData Load()
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new WheelCardRewardProgressSaveData();
            }

            try
            {
                WheelCardRewardProgressSaveData saveData = JsonUtility.FromJson<WheelCardRewardProgressSaveData>(json);
                return saveData ?? new WheelCardRewardProgressSaveData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to load wheel card reward progress prefs. Resetting stored progress. " + exception);
                return new WheelCardRewardProgressSaveData();
            }
        }

        private static void Save(WheelCardRewardProgressSaveData saveData)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
        }
        #endregion

        #region Nested Types
        [Serializable]
        private sealed class WheelCardRewardProgressSaveData
        {
            public List<WheelCardRewardProgressEntry> cards = new List<WheelCardRewardProgressEntry>();

            public WheelCardRewardProgressEntry GetOrCreate(string cardKey)
            {
                if (cards == null)
                {
                    cards = new List<WheelCardRewardProgressEntry>();
                }

                string safeCardKey = string.IsNullOrWhiteSpace(cardKey) ? "unknown" : cardKey;
                for (int i = 0; i < cards.Count; i++)
                {
                    WheelCardRewardProgressEntry entry = cards[i];
                    if (entry != null && entry.key == safeCardKey)
                    {
                        return entry;
                    }
                }

                WheelCardRewardProgressEntry newEntry = new WheelCardRewardProgressEntry
                {
                    key = safeCardKey
                };
                cards.Add(newEntry);
                return newEntry;
            }
        }

        [Serializable]
        private sealed class WheelCardRewardProgressEntry
        {
            public string key;
            public int totalAmount;
        }
        #endregion
    }

    public readonly struct WheelCardRewardProgress
    {
        public WheelCardRewardProgress(int completedCount, int progressValue, int requiredAmount)
        {
            CompletedCount = Mathf.Max(0, completedCount);
            ProgressValue = Mathf.Max(0, progressValue);
            RequiredAmount = Mathf.Max(1, requiredAmount);
        }

        public int CompletedCount { get; }
        public int ProgressValue { get; }
        public int RequiredAmount { get; }

        public static WheelCardRewardProgress FromTotalAmount(int totalAmount, int requiredAmount)
        {
            int safeRequiredAmount = Mathf.Max(1, requiredAmount);
            int safeTotalAmount = Mathf.Max(0, totalAmount);

            return new WheelCardRewardProgress(
                safeTotalAmount / safeRequiredAmount,
                safeTotalAmount % safeRequiredAmount,
                safeRequiredAmount);
        }
    }

    public readonly struct WheelCardRewardProgressChange
    {
        public WheelCardRewardProgressChange(WheelCardRewardProgress previous, WheelCardRewardProgress current)
        {
            Previous = previous;
            Current = current;
        }

        public WheelCardRewardProgress Previous { get; }
        public WheelCardRewardProgress Current { get; }
    }
}
