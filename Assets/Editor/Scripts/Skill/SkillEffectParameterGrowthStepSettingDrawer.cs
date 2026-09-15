using System;
using System.Collections.Generic;
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

            if (currentType != previousType)
            {
                // 成長方式を切り替えたときは、切替後の方式に応じた分かりやすい既定値へ入れ替える。
                growthValueProperty.doubleValue = GetDefaultGrowthValue(currentType);
                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = true;
            }
            else if (growthValueProperty.doubleValue == 0d && IsNewlyAddedElement(property, out int elementIndex))
            {
                // 配列サイズが直前の描画より増えている(=「+」で新規追加された)末尾要素にのみ既定値を投入する。
                // 既存要素は成長値0のままにしていても、ここでは一切触らない。
                double defaultValue = GetDefaultGrowthValue(currentType);
                growthValueProperty.doubleValue = defaultValue;
                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = true;

                // 配列サイズの増減検知はエディタセッション内の一時状態のため、スクリプト再コンパイル直後に
                // 既存アセットを初めて開いた場合など、まれに新規追加と誤判定される可能性がある。
                // その場合に成長値0の意図的な設定が上書きされていないか気付けるよう警告を出す。
                Debug.LogWarning(
                    $"[{nameof(SkillEffectParameterGrowthStepSettingDrawer)}] "
                    + $"新規追加と判定した成長ステップ(インデックス{elementIndex})に既定の成長値({defaultValue})を自動設定しました。"
                    + "成長値0を意図的に設定していた既存の要素だった場合は、Undo(Ctrl+Z)で戻すか、再度0を入力してください。",
                    property.serializedObject.targetObject);
            }

            EditorGUI.PropertyField(growthValueRect, growthValueProperty, GROWTH_VALUE_LABEL);

            EditorGUI.EndProperty();
        }

        /// <summary>
        ///     指定したプロパティが成長ステップ配列の末尾要素であり、直前の描画時点より配列サイズが
        ///     増えている(=新規追加された可能性が高い)かどうかを判定します。
        ///     配列サイズの記録はエディタセッション内(ドメインリロードまで)のみ保持する一時状態です。
        /// </summary>
        /// <param name="property"> 判定対象の成長ステップです。 </param>
        /// <param name="elementIndex"> 判定対象の配列インデックス。判定できない場合は-1。 </param>
        /// <returns> 新規追加された末尾要素と判定できた場合はtrue。 </returns>
        private static bool IsNewlyAddedElement(SerializedProperty property, out int elementIndex)
        {
            elementIndex = -1;

            string propertyPath = property.propertyPath;
            int markerIndex = propertyPath.LastIndexOf(ARRAY_DATA_MARKER, StringComparison.Ordinal);
            if (markerIndex < 0 || !propertyPath.EndsWith("]", StringComparison.Ordinal))
            {
                return false;
            }

            string arrayPropertyPath = propertyPath.Substring(0, markerIndex);
            int indexStart = markerIndex + ARRAY_DATA_MARKER.Length;
            string indexText = propertyPath.Substring(indexStart, propertyPath.Length - indexStart - 1);
            if (!int.TryParse(indexText, out elementIndex))
            {
                return false;
            }

            SerializedProperty arrayProperty = property.serializedObject.FindProperty(arrayPropertyPath);
            if (arrayProperty == null)
            {
                return false;
            }

            int currentSize = arrayProperty.arraySize;
            if (elementIndex != currentSize - 1)
            {
                // 末尾以外の要素は、追加操作で動くことがないため対象外とする。
                return false;
            }

            string key = property.serializedObject.targetObject.GetInstanceID() + "|" + arrayPropertyPath;
            LastKnownArraySizes.TryGetValue(key, out int previousSize);
            LastKnownArraySizes[key] = currentSize;
            return currentSize > previousSize;
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
        private const string ARRAY_DATA_MARKER = ".Array.data[";
        private static readonly GUIContent GROWTH_TYPE_LABEL = new("成長方式");
        private static readonly GUIContent GROWTH_VALUE_LABEL = new("成長値");

        /// <summary> 配列(オブジェクトインスタンスID+配列プロパティパス)ごとに、直近確認した配列サイズを保持します。 </summary>
        private static readonly Dictionary<string, int> LastKnownArraySizes = new();
    }
}
