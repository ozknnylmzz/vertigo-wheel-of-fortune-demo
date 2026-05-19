using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.WheelOfFortune.Rewards.UI
{
    public sealed class RewardListItemView : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TMP_Text ui_text_reward_amount_value;
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (ui_image_reward_icon == null)
            {
                ui_image_reward_icon = GetComponentInChildren<Image>(true);
            }

            if (ui_text_reward_amount_value == null)
            {
                ui_text_reward_amount_value = GetComponentInChildren<TMP_Text>(true);
            }
        }
#endif
        #endregion

        #region Public API
        public void Apply(Sprite rewardIcon, string rewardAmountValue)
        {
            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = rewardIcon;
                ui_image_reward_icon.enabled = rewardIcon != null;
            }

            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.text = rewardAmountValue;
            }
        }
        #endregion
    }
}
