using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderのSourceAsset設定とcollection設定を編集します。
    /// </summary>
    internal sealed class SourceDataProviderSettingsProvider : SettingsProvider
    {
        /// <summary>
        ///     設定画面を初期化します。
        /// </summary>
        /// <param name="path"> Settings画面内のパスです。 </param>
        /// <param name="scopes"> 設定のスコープです。 </param>
        /// <param name="keywords"> 検索キーワードです。 </param>
        private SourceDataProviderSettingsProvider(
            string path,
            SettingsScope scopes,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        /// <summary>
        ///     SourceDataProvider設定画面を生成します。
        /// </summary>
        /// <returns> 生成した設定画面です。 </returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SourceDataProviderSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        /// <summary>
        ///     SourceDataProvider設定画面を描画します。
        /// </summary>
        /// <param name="searchContext"> Settings画面の検索文字列です。 </param>
        public override void OnGUI(string searchContext)
        {
            SourceDataProviderSettings settings = SourceDataProviderSettings.instance;
            _ = settings.SourceAssetMappings.Count;
            _ = settings.SourceCollectionMappings.Count;
            SerializedObject serializedObject = new(settings);
            SerializedProperty sourceAssets = serializedObject.FindProperty(SOURCE_ASSET_MAPPINGS_PROPERTY);
            SerializedProperty collections = serializedObject.FindProperty(SOURCE_COLLECTION_MAPPINGS_PROPERTY);

            EditorGUILayout.HelpBox(
                "SourceAssetとcollectionを分離して管理します。"
                + " SourceAssetにはAddressable ScriptableObjectのみを登録し、collection側でどの配列をリポジトリとして扱うかを設定します。",
                MessageType.Info);

            DrawSourceAssetSection(sourceAssets);
            EditorGUILayout.Space();
            DrawCollectionSection(collections, sourceAssets);

            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("設定を適用"))
            {
                settings.SaveSettings();
            }
        }

        /// <summary>
        ///     SourceAsset設定セクションを描画します。
        /// </summary>
        /// <param name="sourceAssets"> SourceAsset設定配列です。 </param>
        private static void DrawSourceAssetSection(SerializedProperty sourceAssets)
        {
            EditorGUILayout.LabelField("Source Assets", EditorStyles.boldLabel);

            int removeIndex = -1;
            for (int i = 0; i < sourceAssets.arraySize; i++)
            {
                SerializedProperty mapping = sourceAssets.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSourceAssetMapping(mapping, i, ref removeIndex);
                EditorGUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                sourceAssets.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button("Source Assetを追加"))
            {
                sourceAssets.InsertArrayElementAtIndex(sourceAssets.arraySize);
                SerializedProperty mapping = sourceAssets.GetArrayElementAtIndex(sourceAssets.arraySize - 1);
                mapping.FindPropertyRelative(SOURCE_ASSET_ADDRESSABLE_KEY_PROPERTY).stringValue = string.Empty;
            }
        }

        /// <summary>
        ///     collection設定セクションを描画します。
        /// </summary>
        /// <param name="collections"> collection設定配列です。 </param>
        /// <param name="sourceAssets"> SourceAsset設定配列です。 </param>
        private static void DrawCollectionSection(
            SerializedProperty collections,
            SerializedProperty sourceAssets)
        {
            EditorGUILayout.LabelField("Collections", EditorStyles.boldLabel);

            int removeIndex = -1;
            for (int i = 0; i < collections.arraySize; i++)
            {
                SerializedProperty mapping = collections.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawCollectionMapping(mapping, sourceAssets, i, ref removeIndex);
                EditorGUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                collections.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button("Collectionを追加"))
            {
                collections.InsertArrayElementAtIndex(collections.arraySize);
                SerializedProperty mapping = collections.GetArrayElementAtIndex(collections.arraySize - 1);
                mapping.FindPropertyRelative(COLLECTION_KEY_PROPERTY).stringValue = string.Empty;
                mapping.FindPropertyRelative(COLLECTION_SOURCE_ASSET_KEY_PROPERTY).stringValue = string.Empty;
                mapping.FindPropertyRelative(COLLECTION_PROPERTY_PATH_PROPERTY).stringValue = string.Empty;
            }
        }

        /// <summary>
        ///     1件分のSourceAsset設定を描画します。
        /// </summary>
        /// <param name="mapping"> 描画対象の設定です。 </param>
        /// <param name="index"> 設定の配列位置です。 </param>
        /// <param name="removeIndex"> 削除する配列位置です。 </param>
        private static void DrawSourceAssetMapping(
            SerializedProperty mapping,
            int index,
            ref int removeIndex)
        {
            SerializedProperty addressableKey = mapping.FindPropertyRelative(SOURCE_ASSET_ADDRESSABLE_KEY_PROPERTY);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Source Asset {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("削除", GUILayout.Width(48f)))
            {
                removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(addressableKey, new GUIContent("Addressable Key"));
            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                addressableKey.stringValue,
                out ScriptableObject sourceAsset))
            {
                EditorGUILayout.HelpBox("AddressableキーからScriptableObjectを解決できません。", MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Source Asset", sourceAsset, sourceAsset.GetType(), false);
            }

            string[] availablePaths = SourceDataProviderRepositoryResolver.GetCollectionPropertyPaths(sourceAsset);
            if (availablePaths.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    $"配列 / List 候補: {string.Join(", ", availablePaths)}",
                    MessageType.None);
            }
        }

        /// <summary>
        ///     1件分のcollection設定を描画します。
        /// </summary>
        /// <param name="mapping"> 描画対象の設定です。 </param>
        /// <param name="sourceAssets"> SourceAsset設定配列です。 </param>
        /// <param name="index"> 設定の配列位置です。 </param>
        /// <param name="removeIndex"> 削除する配列位置です。 </param>
        private static void DrawCollectionMapping(
            SerializedProperty mapping,
            SerializedProperty sourceAssets,
            int index,
            ref int removeIndex)
        {
            SerializedProperty collectionKey = mapping.FindPropertyRelative(COLLECTION_KEY_PROPERTY);
            SerializedProperty sourceAssetKey = mapping.FindPropertyRelative(COLLECTION_SOURCE_ASSET_KEY_PROPERTY);
            SerializedProperty propertyPath = mapping.FindPropertyRelative(COLLECTION_PROPERTY_PATH_PROPERTY);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Collection {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("削除", GUILayout.Width(48f)))
            {
                removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            DrawSourceAssetSelector(sourceAssetKey, sourceAssets);
            EditorGUILayout.PropertyField(collectionKey, new GUIContent("Collection Key"));

            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                sourceAssetKey.stringValue,
                out ScriptableObject sourceAsset))
            {
                EditorGUILayout.HelpBox("選択中のSourceAssetを解決できません。", MessageType.Warning);
                EditorGUILayout.PropertyField(propertyPath, new GUIContent("Collection Property Path"));
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Resolved Source Asset", sourceAsset, sourceAsset.GetType(), false);
            }

            string[] availablePaths = SourceDataProviderRepositoryResolver.GetCollectionPropertyPaths(sourceAsset);
            if (availablePaths.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    $"配列 / List 候補: {string.Join(", ", availablePaths)}",
                    MessageType.None);
            }

            DrawCollectionPathSelector(propertyPath, availablePaths);
        }

        /// <summary>
        ///     SourceAsset選択欄を描画します。
        /// </summary>
        /// <param name="sourceAssetKey"> 選択結果を保存するプロパティです。 </param>
        /// <param name="sourceAssets"> SourceAsset設定配列です。 </param>
        private static void DrawSourceAssetSelector(
            SerializedProperty sourceAssetKey,
            SerializedProperty sourceAssets)
        {
            string[] labels = new string[sourceAssets.arraySize + 1];
            labels[0] = "<未設定>";
            int selectedIndex = 0;

            for (int i = 0; i < sourceAssets.arraySize; i++)
            {
                SerializedProperty mapping = sourceAssets.GetArrayElementAtIndex(i);
                SerializedProperty addressableKey = mapping.FindPropertyRelative(SOURCE_ASSET_ADDRESSABLE_KEY_PROPERTY);
                labels[i + 1] = string.IsNullOrWhiteSpace(addressableKey.stringValue)
                    ? $"Source Asset {i + 1}"
                    : addressableKey.stringValue;
                if (string.Equals(addressableKey.stringValue, sourceAssetKey.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i + 1;
                }
            }

            int nextIndex = EditorGUILayout.Popup("Source Asset", selectedIndex, labels);
            sourceAssetKey.stringValue = nextIndex <= 0
                ? string.Empty
                : sourceAssets
                    .GetArrayElementAtIndex(nextIndex - 1)
                    .FindPropertyRelative(SOURCE_ASSET_ADDRESSABLE_KEY_PROPERTY)
                    .stringValue;
        }

        /// <summary>
        ///     collectionプロパティ選択欄を描画します。
        /// </summary>
        /// <param name="propertyPath"> 選択結果を保存するプロパティです。 </param>
        /// <param name="availablePaths"> 選択可能な配列プロパティパスです。 </param>
        private static void DrawCollectionPathSelector(
            SerializedProperty propertyPath,
            string[] availablePaths)
        {
            string[] labels = new string[availablePaths.Length + 1];
            labels[0] = "<未設定>";
            int selectedIndex = 0;

            for (int i = 0; i < availablePaths.Length; i++)
            {
                labels[i + 1] = availablePaths[i];
                if (string.Equals(availablePaths[i], propertyPath.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i + 1;
                }
            }

            int nextIndex = EditorGUILayout.Popup("Collection Property Path", selectedIndex, labels);
            propertyPath.stringValue = nextIndex <= 0
                ? string.Empty
                : availablePaths[nextIndex - 1];
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "Source Data Provider";
        private const string SOURCE_ASSET_MAPPINGS_PROPERTY = "_sourceAssetMappings";
        private const string SOURCE_COLLECTION_MAPPINGS_PROPERTY = "_sourceCollectionMappings";
        private const string SOURCE_ASSET_ADDRESSABLE_KEY_PROPERTY = "_addressableKey";
        private const string COLLECTION_KEY_PROPERTY = "_collectionKey";
        private const string COLLECTION_SOURCE_ASSET_KEY_PROPERTY = "_sourceAssetAddressableKey";
        private const string COLLECTION_PROPERTY_PATH_PROPERTY = "_propertyPath";
    }
}
