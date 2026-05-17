using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelOfFortune.SpinCenter.Data;

namespace Vertigo.WheelOfFortune.SpinCenter.UI
{
    public sealed class SpinCenterView : MonoBehaviour
    {
        private const string WheelBaseName = "ui_image_spin_base";
        private const string WheelIndicatorName = "ui_image_spin_indicator";
        private const string TitleValueName = "ui_text_spin_title";
        private const string SubtitleValueName = "ui_text_spin_subtitle";
        private const string SliceContainerName = "ui_container_wheel_slices";

        [Header("Main UI References")]
        [SerializeField] private Image ui_image_spin_base;
        [SerializeField] private Image ui_image_spin_indicator;
        [SerializeField] private TMP_Text ui_text_spin_title;
        [SerializeField] private TMP_Text ui_text_spin_subtitle;
        [SerializeField] private Transform ui_container_wheel_slices;

        [Header("Slice Views")]
        [SerializeField] private List<SpinCenterSliceView> ui_wheel_slice_views = new List<SpinCenterSliceView>();

        private void OnValidate()
        {
            AutoAssignMainReferences();
            CollectSliceViews();
        }

        public void ApplyVisualData(SpinCenterTierVisualData visualData)
        {
            if (visualData == null)
            {
                throw new ArgumentNullException(nameof(visualData));
            }

            if (ui_image_spin_base != null)
            {
                ui_image_spin_base.sprite = visualData.wheelBaseSprite;
                ui_image_spin_base.enabled = visualData.wheelBaseSprite != null;
            }

            if (ui_image_spin_indicator != null)
            {
                ui_image_spin_indicator.sprite = visualData.wheelIndicatorSprite;
                ui_image_spin_indicator.enabled = visualData.wheelIndicatorSprite != null;
            }

            if (ui_text_spin_title != null)
            {
                ui_text_spin_title.text = visualData.titleValue;
            }

            if (ui_text_spin_subtitle != null)
            {
                ui_text_spin_subtitle.text = visualData.subtitleValue;
            }

            int viewCount = ui_wheel_slice_views.Count;
            int dataCount = visualData.slices != null ? visualData.slices.Count : 0;
            int minCount = Mathf.Min(viewCount, dataCount);

            for (int i = 0; i < minCount; i++)
            {
                ui_wheel_slice_views[i].ApplyVisualData(visualData.slices[i]);
            }

            for (int i = minCount; i < viewCount; i++)
            {
                ui_wheel_slice_views[i].ApplyVisualData(null);
            }
        }

        private void AutoAssignMainReferences()
        {
            AutoAssignByName(ref ui_image_spin_base, WheelBaseName);
            AutoAssignByName(ref ui_image_spin_indicator, WheelIndicatorName);
            AutoAssignByName(ref ui_text_spin_title, TitleValueName);
            AutoAssignByName(ref ui_text_spin_subtitle, SubtitleValueName);

            if (ui_container_wheel_slices == null)
            {
                Transform candidate = FindChildRecursive(transform, SliceContainerName);
                if (candidate != null)
                {
                    ui_container_wheel_slices = candidate;
                }
            }
        }

        private void CollectSliceViews()
        {
            ui_wheel_slice_views.Clear();

            if (ui_container_wheel_slices == null)
            {
                return;
            }

            SpinCenterSliceView[] sliceViews = ui_container_wheel_slices.GetComponentsInChildren<SpinCenterSliceView>(true);
            if (sliceViews == null || sliceViews.Length == 0)
            {
                return;
            }

            ui_wheel_slice_views.AddRange(sliceViews);
            ui_wheel_slice_views.Sort((a, b) => GetSortOrder(a.transform.name).CompareTo(GetSortOrder(b.transform.name)));
        }

        private static int GetSortOrder(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return int.MaxValue;
            }

            int value = 0;
            bool foundDigit = false;

            for (int i = 0; i < objectName.Length; i++)
            {
                char c = objectName[i];
                if (c < '0' || c > '9')
                {
                    if (foundDigit)
                    {
                        break;
                    }

                    continue;
                }

                foundDigit = true;
                value = value * 10 + (c - '0');
            }

            return foundDigit ? value : int.MaxValue;
        }

        private void AutoAssignByName<T>(ref T field, string objectName) where T : Component
        {
            if (field != null)
            {
                return;
            }

            Transform candidate = FindChildRecursive(transform, objectName);
            if (candidate == null)
            {
                return;
            }

            field = candidate.GetComponent<T>();
        }

        private static Transform FindChildRecursive(Transform root, string targetName)
        {
            if (root.name == targetName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Transform result = FindChildRecursive(child, targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
