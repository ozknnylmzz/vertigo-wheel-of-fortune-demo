using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelOfFortune.Popups.Data
{
    [CreateAssetMenu(
        fileName = "stage_reward_popup_settings",
        menuName = "Vertigo/Wheel Of Fortune/Stage Reward Popup Settings")]
    public sealed class StageRewardPopupSettingsAsset : ScriptableObject
    {
        #region Constants
        private const float MinRewardIconSpinDurationSeconds = 0.01f;
        #endregion

        #region Inspector Fields
        [SerializeField] [Min(MinRewardIconSpinDurationSeconds)] private float rewardIconSpinDurationSeconds = 1.25f;
        [SerializeField] private List<StageRewardPopupData> rewards = new List<StageRewardPopupData>();
        #endregion

        #region Properties
        public float RewardIconSpinDurationSeconds => Mathf.Max(MinRewardIconSpinDurationSeconds, rewardIconSpinDurationSeconds);
        #endregion

        #region Public API
        public bool TryGetReward(int level, out StageRewardPopupData rewardData)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                StageRewardPopupData data = rewards[i];
                if (data != null && data.Level == level)
                {
                    rewardData = data;
                    return true;
                }
            }

            rewardData = null;
            return false;
        }
        #endregion

        #region Validation
        private void OnValidate()
        {
            rewardIconSpinDurationSeconds = Mathf.Max(MinRewardIconSpinDurationSeconds, rewardIconSpinDurationSeconds);

            for (int i = 0; i < rewards.Count; i++)
            {
                rewards[i]?.Validate();
            }
        }
        #endregion
    }

    [Serializable]
    public sealed class StageRewardPopupData
    {
        #region Inspector Fields
        [SerializeField] [Min(1)] private int level = 1;
        [SerializeField] private Sprite rewardIcon;
        [SerializeField] private string message = "Stage reward kazandin!";
        [SerializeField] private string buttonText = "Claim";
        #endregion

        #region Properties
        public int Level => level;
        public Sprite RewardIcon => rewardIcon;
        public string Message => message;
        public string ButtonText => buttonText;
        #endregion

        #region Validation
        public void Validate()
        {
            level = Mathf.Max(1, level);

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Stage reward kazandin!";
            }

            if (string.IsNullOrWhiteSpace(buttonText))
            {
                buttonText = "Claim";
            }
        }
        #endregion
    }
}
