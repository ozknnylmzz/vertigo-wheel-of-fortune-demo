using UnityEditor;
using UnityEngine;
using Vertigo.WheelOfFortune.Rewards.Data;

namespace Vertigo.WheelOfFortune.Rewards.Editor
{
    [CustomPropertyDrawer(typeof(WheelRewardPoolEntry))]
    public sealed class WheelRewardPoolEntryDrawer : PropertyDrawer
    {
        private const float FieldSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                SerializedProperty rewardTypeProperty = property.FindPropertyRelative("rewardType");
                SerializedProperty rewardNameProperty = property.FindPropertyRelative("rewardName");
                SerializedProperty rewardIconProperty = property.FindPropertyRelative("rewardIcon");
                SerializedProperty progressRequiredAmountProperty = property.FindPropertyRelative("progressRequiredAmount");

                float y = position.y + EditorGUIUtility.singleLineHeight + FieldSpacing;
                DrawProperty(position, ref y, rewardTypeProperty);
                DrawProperty(position, ref y, rewardNameProperty);
                DrawProperty(position, ref y, rewardIconProperty);

                if (IsCardsReward(rewardTypeProperty))
                {
                    DrawProperty(position, ref y, progressRequiredAmountProperty);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            SerializedProperty rewardTypeProperty = property.FindPropertyRelative("rewardType");
            int visibleFieldCount = IsCardsReward(rewardTypeProperty) ? 4 : 3;
            return EditorGUIUtility.singleLineHeight
                   + FieldSpacing
                   + (visibleFieldCount * EditorGUIUtility.singleLineHeight)
                   + ((visibleFieldCount - 1) * FieldSpacing);
        }

        private static void DrawProperty(Rect position, ref float y, SerializedProperty property)
        {
            if (property == null)
            {
                return;
            }

            Rect rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, property);
            y += EditorGUIUtility.singleLineHeight + FieldSpacing;
        }

        private static bool IsCardsReward(SerializedProperty rewardTypeProperty)
        {
            if (rewardTypeProperty == null || rewardTypeProperty.enumValueIndex < 0)
            {
                return false;
            }

            string[] enumNames = rewardTypeProperty.enumNames;
            return rewardTypeProperty.enumValueIndex < enumNames.Length
                   && enumNames[rewardTypeProperty.enumValueIndex] == nameof(WheelRewardType.Cards);
        }
    }
}
