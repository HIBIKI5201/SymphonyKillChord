using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     DataIDの文字列入力、登録済みデータ選択、ハッシュ焼き込みを行います。
    /// </summary>
    [CustomPropertyDrawer(typeof(DataID))]
    internal sealed class DataIDPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        ///     DataIDのInspector表示を描画します。
        /// </summary>
        /// <param name="position"> 描画領域です。 </param>
        /// <param name="property"> 描画対象のDataIDです。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SourceDataCollectionAttribute collectionAttribute =
                fieldInfo.GetCustomAttribute<SourceDataCollectionAttribute>();
            string collectionKey = collectionAttribute?.CollectionKey;
            SerializedProperty idProperty = property.FindPropertyRelative(
                SourceDataProviderRepositoryResolver.ID_PROPERTY_NAME);
            SerializedProperty hashProperty = property.FindPropertyRelative(
                SourceDataProviderRepositoryResolver.HASH_PROPERTY_NAME);

            Rect idRect = new(
                position.x,
                position.y,
                position.width - JUMP_BUTTON_WIDTH - EditorGUIUtility.standardVerticalSpacing,
                EditorGUIUtility.singleLineHeight);
            Rect jumpRect = new(
                idRect.xMax + EditorGUIUtility.standardVerticalSpacing,
                position.y,
                JUMP_BUTTON_WIDTH,
                EditorGUIUtility.singleLineHeight);
            Rect hashRect = new(
                position.x,
                idRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight);
            Rect warningRect = new(
                position.x,
                hashRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUIUtility.singleLineHeight * WARNING_LINE_COUNT);

            IReadOnlyList<SourceDataIDOption> options = string.IsNullOrWhiteSpace(collectionKey)
                ? Array.Empty<SourceDataIDOption>()
                : SourceDataProviderRepositoryResolver.GetOptions(collectionKey);
            bool isAuthoring = string.IsNullOrWhiteSpace(collectionKey)
                || SourceDataProviderRepositoryResolver.IsAuthoringProperty(
                    collectionKey,
                    property.serializedObject.targetObject,
                    property.propertyPath);

            EditorGUI.BeginChangeCheck();
            if (isAuthoring || options.Count == 0)
            {
                EditorGUI.PropertyField(idRect, idProperty, label);
            }
            else
            {
                DrawSelector(idRect, label, idProperty, hashProperty, options);
            }

            if (EditorGUI.EndChangeCheck() && (isAuthoring || options.Count == 0))
            {
                hashProperty.intValue = DataIDHasher.Compute(collectionKey, idProperty.stringValue);
            }

            using (new EditorGUI.DisabledScope(
                string.IsNullOrWhiteSpace(collectionKey) || string.IsNullOrWhiteSpace(idProperty.stringValue)))
            {
                if (GUI.Button(jumpRect, JUMP_LABEL, EditorStyles.miniButton)
                    && PlannerMasterDataWindow.TryGetOrOpenWindow(out PlannerMasterDataWindow window))
                {
                    window.NavigateToCollectionItem(collectionKey, idProperty.stringValue);
                }
            }

            DrawHash(hashRect, hashProperty);

            string warning = BuildWarning(
                collectionKey,
                idProperty.stringValue,
                hashProperty.intValue,
                options,
                isAuthoring);
            if (!string.IsNullOrEmpty(warning))
            {
                EditorGUI.HelpBox(warningRect, warning, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        ///     DataID表示に必要な高さを取得します。
        /// </summary>
        /// <param name="property"> 描画対象のDataIDです。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        /// <returns> 描画に必要な高さです。 </returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (2f + WARNING_LINE_COUNT)
                + EditorGUIUtility.standardVerticalSpacing * 2f;
        }

        /// <summary>
        ///     登録済みIDを選択するポップアップを描画します。
        /// </summary>
        /// <param name="position"> 描画領域です。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        /// <param name="idProperty"> 文字列IDプロパティです。 </param>
        /// <param name="hashProperty"> 数値IDプロパティです。 </param>
        /// <param name="options"> 登録済みID一覧です。 </param>
        private static void DrawSelector(
            Rect position,
            GUIContent label,
            SerializedProperty idProperty,
            SerializedProperty hashProperty,
            IReadOnlyList<SourceDataIDOption> options)
        {
            string[] labels = new string[options.Count + 1];
            labels[0] = UNASSIGNED_LABEL;
            int selectedIndex = 0;

            for (int i = 0; i < options.Count; i++)
            {
                labels[i + 1] = options[i].Id;
                if (string.Equals(options[i].Id, idProperty.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i + 1;
                }
            }

            int nextIndex = EditorGUI.Popup(position, label.text, selectedIndex, labels);
            if (nextIndex == selectedIndex)
            {
                return;
            }

            if (nextIndex <= 0)
            {
                idProperty.stringValue = string.Empty;
                hashProperty.intValue = 0;
                return;
            }

            SourceDataIDOption option = options[nextIndex - 1];
            idProperty.stringValue = option.Id;
            hashProperty.intValue = option.HashId;
        }

        /// <summary>
        ///     読み取り専用ハッシュとコピーボタンを描画します。
        /// </summary>
        /// <param name="position"> 描画領域です。 </param>
        /// <param name="hashProperty"> 数値IDプロパティです。 </param>
        private static void DrawHash(Rect position, SerializedProperty hashProperty)
        {
            Rect valueRect = new(
                position.x,
                position.y,
                position.width - COPY_BUTTON_WIDTH - EditorGUIUtility.standardVerticalSpacing,
                position.height);
            Rect buttonRect = new(
                valueRect.xMax + EditorGUIUtility.standardVerticalSpacing,
                position.y,
                COPY_BUTTON_WIDTH,
                position.height);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.IntField(valueRect, HASH_LABEL, hashProperty.intValue);
            }

            if (GUI.Button(buttonRect, COPY_LABEL))
            {
                GUIUtility.systemCopyBuffer = hashProperty.intValue.ToString();
            }
        }

        /// <summary>
        ///     DataID設定に対する警告文を生成します。
        /// </summary>
        /// <param name="collectionKey"> DataIDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <param name="hashId"> 焼き込み済み数値IDです。 </param>
        /// <param name="options"> 同一カテゴリの登録済みID一覧です。 </param>
        /// <param name="isAuthoring"> 生成側フィールドの場合はtrueです。 </param>
        /// <returns> 警告がある場合は警告文、それ以外は空文字列です。 </returns>
        private static string BuildWarning(
            string collectionKey,
            string id,
            int hashId,
            IReadOnlyList<SourceDataIDOption> options,
            bool isAuthoring)
        {
            if (string.IsNullOrWhiteSpace(collectionKey))
            {
                return "SourceDataCollection属性が設定されていません。";
            }

            if (!SourceDataProviderRepositoryResolver.ContainsCollectionKey(collectionKey))
            {
                return $"SourceDataProviderにCollectionKey「{collectionKey}」が登録されていません。";
            }

            int expectedHash = DataIDHasher.Compute(collectionKey, id);
            if (hashId != expectedHash)
            {
                return $"ハッシュが未更新です。Expected: {expectedHash}";
            }

            if (isAuthoring)
            {
                return DataIDCollisionDetector.FindWarning(id, hashId, options);
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (string.Equals(options[i].Id, id, StringComparison.Ordinal))
                    {
                        return string.Empty;
                    }
                }
            }

            return "選択したIDがcollectionへ登録されていません。";
        }

        private const float COPY_BUTTON_WIDTH = 52f;
        private const float JUMP_BUTTON_WIDTH = 56f;
        private const float WARNING_LINE_COUNT = 2f;
        private const string HASH_LABEL = "Hash";
        private const string COPY_LABEL = "Copy";
        private const string JUMP_LABEL = "Planner";
        private const string UNASSIGNED_LABEL = "<未設定>";
    }
}
