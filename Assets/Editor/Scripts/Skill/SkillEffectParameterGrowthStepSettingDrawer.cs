using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.InfraStructure.Player;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Skill
{
    /// <summary>
    ///     成長ステップの成長方式に応じて、成長値の初期値を自動設定します。
    /// </summary>
    [CustomPropertyDrawer(typeof(SkillTemplateAsset.SkillEffectParameterGrowthStepSetting))]
    internal sealed class SkillEffectParameterGrowthStepSettingDrawer : PropertyDrawer
    {
        /// <summary>
        ///     成長ステップのInspector表示を描画します。
        /// </summary>
        /// <param name="position"> 描画領域です。 </param>
        /// <param name="property"> 描画対象の成長ステップです。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty growthTypeProperty = property.FindPropertyRelative(GROWTH_TYPE_FIELD_NAME);
            SerializedProperty growthValueProperty = property.FindPropertyRelative(GROWTH_VALUE_FIELD_NAME);

            Rect growthTypeRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect growthValueRect = new(
                position.x,
                growthTypeRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight);

            SkillEffectParameterGrowthType previousType = (SkillEffectParameterGrowthType)growthTypeProperty.enumValueIndex;
            EditorGUI.PropertyField(growthTypeRect, growthTypeProperty, GROWTH_TYPE_LABEL);
            SkillEffectParameterGrowthType currentType = (SkillEffectParameterGrowthType)growthTypeProperty.enumValueIndex;

            if (currentType != previousType || growthValueProperty.doubleValue == 0d)
            {
                growthValueProperty.doubleValue = GetDefaultGrowthValue(currentType);
                // 直接代入した変更はここで確定させないと、描画タイミング次第で破棄されることがある。
                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = true;
            }

            EditorGUI.PropertyField(growthValueRect, growthValueProperty, GROWTH_VALUE_LABEL);

            EditorGUI.EndProperty();
        }

        /// <summary>
        ///     成長ステップ表示に必要な高さを取得します。
        /// </summary>
        /// <param name="property"> 描画対象の成長ステップです。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        /// <returns> 描画に必要な高さです。 </returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        ///     成長方式に応じた成長値の既定値を取得します。
        /// </summary>
        /// <param name="growthType"> 成長方式です。 </param>
        /// <returns> 既定の成長値です。 </returns>
        private static double GetDefaultGrowthValue(SkillEffectParameterGrowthType growthType)
        {
            return growthType == SkillEffectParameterGrowthType.Multiplicative
                ? DEFAULT_MULTIPLICATIVE_VALUE
                : DEFAULT_ADDITIVE_VALUE;
        }

        private const string GROWTH_TYPE_FIELD_NAME = "_growthType";
        private const string GROWTH_VALUE_FIELD_NAME = "_growthValue";
        private const double DEFAULT_MULTIPLICATIVE_VALUE = 1.1d;
        private const double DEFAULT_ADDITIVE_VALUE = 25d;
        private static readonly GUIContent GROWTH_TYPE_LABEL = new("成長方式");
        private static readonly GUIContent GROWTH_VALUE_LABEL = new("成長値");
    }
}
