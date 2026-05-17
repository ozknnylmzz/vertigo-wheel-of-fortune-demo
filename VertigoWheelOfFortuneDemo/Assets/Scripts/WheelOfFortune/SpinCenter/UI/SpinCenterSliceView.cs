using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.SpinCenter.Data;

namespace Vertigo.WheelOfFortune.SpinCenter.UI
{
    public sealed class SpinCenterSliceView : MonoBehaviour
    {
        [SerializeField] private Image ui_image_wheel_slice_icon;
        [SerializeField] private TMP_Text ui_text_wheel_slice_amount_value;

        private void OnValidate()
        {
            if (ui_image_wheel_slice_icon == null)
            {
                ui_image_wheel_slice_icon = GetComponentInChildren<Image>(true);
            }

            if (ui_text_wheel_slice_amount_value == null)
            {
                ui_text_wheel_slice_amount_value = GetComponentInChildren<TMP_Text>(true);
            }
        }

        public void ApplyVisualData(SpinCenterSliceVisualData data)
        {
            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (ui_image_wheel_slice_icon != null)
            {
                ui_image_wheel_slice_icon.sprite = data.rewardIcon;
                ui_image_wheel_slice_icon.enabled = data.rewardIcon != null;
            }

            if (ui_text_wheel_slice_amount_value != null)
            {
                ui_text_wheel_slice_amount_value.text = data.rewardAmountValue;
                ui_text_wheel_slice_amount_value.gameObject.SetActive(!string.IsNullOrWhiteSpace(data.rewardAmountValue));
            }
        }
    }
}
