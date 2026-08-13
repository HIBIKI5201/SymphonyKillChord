using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderへ登録されたAddressable ScriptableObjectキーの選択UIを描画します。
    /// </summary>
    [CustomPropertyDrawer(typeof(SourceDataAddressAttribute))]
    internal sealed class SourceDataAddressSelectorDrawer : PropertyDrawer
    {
        private static int _cachedMappingsHash;
        private static string[] _cachedKeys = Array.Empty<string>();
        private static string[] _cachedLabels = Array.Empty<string>();

        /// <summary>
        ///     Addressable ScriptableObjectキー選択UIを描画します。
        /// </summary>
        /// <param name="position"> 描画領域です。 </param>
        /// <param name="property"> 描画対象プロパティです。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "SourceDataAddressはstringフィールド専用です。", MessageType.Error);
                return;
            }

            IReadOnlyList<SourceDataProviderSettings.SourceAssetMapping> mappings =
                SourceDataProviderSettings.instance.SourceAssetMappings;
            RebuildCacheIfNeeded(mappings);

            List<string> labels = new() { UNASSIGNED_LABEL };
            List<string> values = new() { string.Empty };
            int selectedIndex = 0;

            for (int i = 0; i < _cachedKeys.Length; i++)
            {
                labels.Add(_cachedLabels[i]);
                values.Add(_cachedKeys[i]);
                if (string.Equals(_cachedKeys[i], property.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = values.Count - 1;
                }
            }

            if (!string.IsNullOrWhiteSpace(property.stringValue) && selectedIndex == 0)
            {
                labels.Add($"Missing: {property.stringValue}");
                values.Add(property.stringValue);
                selectedIndex = values.Count - 1;
            }

            Rect popupRect = new(
                position.x,
                position.y,
                position.width - PING_BUTTON_WIDTH - JUMP_BUTTON_WIDTH - 2f * EditorGUIUtility.standardVerticalSpacing,
                position.height);
            Rect pingRect = new(
                popupRect.xMax + EditorGUIUtility.standardVerticalSpacing,
                position.y,
                PING_BUTTON_WIDTH,
                position.height);
            Rect jumpRect = new(
                pingRect.xMax + EditorGUIUtility.standardVerticalSpacing,
                position.y,
                JUMP_BUTTON_WIDTH,
                position.height);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int nextIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, labels.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = values[nextIndex];
            }

            using (new EditorGUI.DisabledScope(nextIndex <= 0))
            {
                if (GUI.Button(pingRect, PING_LABEL)
                    && SourceDataProviderRepositoryResolver.TryResolveAsset(
                        property.stringValue,
                        out ScriptableObject sourceAsset))
                {
                    EditorGUIUtility.PingObject(sourceAsset);
                }

                if (GUI.Button(jumpRect, JUMP_LABEL, EditorStyles.miniButton)
                    && PlannerMasterDataWindow.TryGetOrOpenWindow(out PlannerMasterDataWindow window))
                {
                    window.NavigateToSourceAsset(property.stringValue);
                }
            }
            EditorGUI.EndProperty();
        }

        /// <summary>
        ///     SourceAsset候補キャッシュを必要時のみ再構築します。
        /// </summary>
        /// <param name="mappings"> 現在のSourceAsset設定一覧です。 </param>
        private static void RebuildCacheIfNeeded(IReadOnlyList<SourceDataProviderSettings.SourceAssetMapping> mappings)
        {
            int hash = 17;
            for (int i = 0; i < mappings.Count; i++)
            {
                hash = (hash * 31) + (mappings[i]?.AddressableKey?.GetHashCode() ?? 0);
            }

            if (hash == _cachedMappingsHash && _cachedKeys.Length == mappings.Count)
            {
                return;
            }

            _cachedMappingsHash = hash;
            _cachedKeys = new string[mappings.Count];
            _cachedLabels = new string[mappings.Count];
            for (int i = 0; i < mappings.Count; i++)
            {
                SourceDataProviderSettings.SourceAssetMapping mapping = mappings[i];
                _cachedKeys[i] = mapping?.AddressableKey ?? string.Empty;
                _cachedLabels[i] = BuildLabel(mapping);
            }
        }

        /// <summary>
        ///     SourceAsset設定の表示名を生成します。
        /// </summary>
        /// <param name="mapping"> 対象のリポジトリ設定です。 </param>
        /// <returns> セレクターへ表示する名前です。 </returns>
        private static string BuildLabel(SourceDataProviderSettings.SourceAssetMapping mapping)
        {
            if (mapping == null || string.IsNullOrWhiteSpace(mapping.AddressableKey))
            {
                return "<空のSourceAsset>";
            }

            if (SourceDataProviderRepositoryResolver.TryResolveAsset(
                mapping.AddressableKey,
                out ScriptableObject sourceAsset))
            {
                return $"{sourceAsset.GetType().Name} ({mapping.AddressableKey})";
            }

            return $"{mapping.AddressableKey}";
        }

        private const float PING_BUTTON_WIDTH = 48f;
        private const float JUMP_BUTTON_WIDTH = 56f;
        private const string PING_LABEL = "Ping";
        private const string JUMP_LABEL = "Planner";
        private const string UNASSIGNED_LABEL = "<未設定>";
    }
}
