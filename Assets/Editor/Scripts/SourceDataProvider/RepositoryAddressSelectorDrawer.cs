using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderへ登録されたリポジトリキーの選択UIを描画します。
    /// </summary>
    [CustomPropertyDrawer(typeof(RepositoryAddressSelectorAttribute))]
    internal sealed class RepositoryAddressSelectorDrawer : PropertyDrawer
    {
        /// <summary>
        ///     リポジトリキー選択UIを描画します。
        /// </summary>
        /// <param name="position"> 描画領域です。 </param>
        /// <param name="property"> 描画対象プロパティです。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "RepositoryAddressSelectorはstringフィールド専用です。", MessageType.Error);
                return;
            }

            IReadOnlyList<SourceDataProviderSettings.RepositoryMapping> mappings =
                SourceDataProviderSettings.instance.RepositoryMappings;
            string[] labels = new string[mappings.Count + 1];
            labels[0] = UNASSIGNED_LABEL;
            int selectedIndex = 0;

            for (int i = 0; i < mappings.Count; i++)
            {
                SourceDataProviderSettings.RepositoryMapping mapping = mappings[i];
                labels[i + 1] = BuildLabel(mapping);
                if (string.Equals(mapping.AddressableKey, property.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i + 1;
                }
            }

            Rect popupRect = new(
                position.x,
                position.y,
                position.width - PING_BUTTON_WIDTH - EditorGUIUtility.standardVerticalSpacing,
                position.height);
            Rect pingRect = new(
                popupRect.xMax + EditorGUIUtility.standardVerticalSpacing,
                position.y,
                PING_BUTTON_WIDTH,
                position.height);

            EditorGUI.BeginProperty(position, label, property);
            int nextIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, labels);
            property.stringValue = nextIndex <= 0
                ? string.Empty
                : mappings[nextIndex - 1].AddressableKey;

            using (new EditorGUI.DisabledScope(nextIndex <= 0))
            {
                if (GUI.Button(pingRect, PING_LABEL)
                    && SourceDataProviderRepositoryResolver.TryResolveRepository(
                        property.stringValue,
                        out UnityEngine.Object repository))
                {
                    EditorGUIUtility.PingObject(repository);
                }
            }
            EditorGUI.EndProperty();
        }

        /// <summary>
        ///     リポジトリ設定の表示名を生成します。
        /// </summary>
        /// <param name="mapping"> 対象のリポジトリ設定です。 </param>
        /// <returns> セレクターへ表示する名前です。 </returns>
        private static string BuildLabel(SourceDataProviderSettings.RepositoryMapping mapping)
        {
            if (SourceDataProviderRepositoryResolver.TryResolveRepository(
                mapping.AddressableKey,
                out UnityEngine.Object repository))
            {
                return $"{repository.GetType().Name} ({mapping.Category})";
            }

            return $"{mapping.AddressableKey} ({mapping.Category})";
        }

        private const float PING_BUTTON_WIDTH = 48f;
        private const string PING_LABEL = "Ping";
        private const string UNASSIGNED_LABEL = "<未設定>";
    }
}
