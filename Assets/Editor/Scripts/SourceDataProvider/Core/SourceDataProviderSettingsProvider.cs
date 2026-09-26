using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     SourceDataProviderのDataAsset設定とcollection設定を編集します。
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
            _ = settings.DataAssetMappings.Count;
            _ = settings.SourceCollectionMappings.Count;
            SerializedObject serializedObject = new(settings);
            SerializedProperty dataAssets = serializedObject.FindProperty(DATA_ASSET_MAPPINGS_PROPERTY);
            SerializedProperty collections = serializedObject.FindProperty(SOURCE_COLLECTION_MAPPINGS_PROPERTY);

            EditorGUILayout.HelpBox(
                "データアセットとcollectionを分離して管理します。"
                + " データアセットにはAddressable ScriptableObjectのみを登録し、collection側でどの配列をリポジトリとして扱うかを設定します。",
                MessageType.Info);

            DrawDataAssetSection(dataAssets);
            EditorGUILayout.Space();
            DrawCollectionSection(collections, dataAssets);

            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("設定を適用"))
            {
                settings.SaveSettings();
            }
        }

        /// <summary>
        ///     設定画面表示開始時にDataAsset一覧を同期します。
        /// </summary>
        /// <param name="searchContext"> 検索文字列です。 </param>
        /// <param name="rootElement"> ルートGUI要素です。 </param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            SourceDataProviderSettings.instance.RefreshDataAssetsFromAddressables();
        }

        /// <summary>
        ///     DataAsset設定セクションを描画します。
        /// </summary>
        /// <param name="dataAssets"> DataAsset設定配列です。 </param>
        private static void DrawDataAssetSection(SerializedProperty dataAssets)
        {
            EditorGUILayout.LabelField("Data Assets", EditorStyles.boldLabel);

            int removeIndex = -1;
            for (int i = 0; i < dataAssets.arraySize; i++)
            {
                SerializedProperty mapping = dataAssets.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawDataAssetMapping(mapping, i, ref removeIndex);
                EditorGUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                dataAssets.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button("データアセットを追加"))
            {
                dataAssets.InsertArrayElementAtIndex(dataAssets.arraySize);
                SerializedProperty mapping = dataAssets.GetArrayElementAtIndex(dataAssets.arraySize - 1);
                mapping.FindPropertyRelative(DATA_ASSET_ADDRESSABLE_KEY_PROPERTY).stringValue = string.Empty;
            }
        }

        /// <summary>
        ///     collection設定セクションを描画します。
        /// </summary>
        /// <param name="collections"> collection設定配列です。 </param>
        /// <param name="dataAssets"> DataAsset設定配列です。 </param>
        private static void DrawCollectionSection(
            SerializedProperty collections,
            SerializedProperty dataAssets)
        {
            EditorGUILayout.LabelField("Collections", EditorStyles.boldLabel);

            int removeIndex = -1;
            for (int i = 0; i < collections.arraySize; i++)
            {
                SerializedProperty mapping = collections.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawCollectionMapping(mapping, dataAssets, i, ref removeIndex);
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
                mapping.FindPropertyRelative(COLLECTION_DATA_ASSET_KEY_PROPERTY).stringValue = string.Empty;
                mapping.FindPropertyRelative(COLLECTION_PROPERTY_PATH_PROPERTY).stringValue = string.Empty;
                mapping.FindPropertyRelative(COLLECTION_ASSET_CREATION_DIRECTORY_PROPERTY).stringValue = string.Empty;
            }
        }

        /// <summary>
        ///     1件分のDataAsset設定を描画します。
        /// </summary>
        /// <param name="mapping"> 描画対象の設定です。 </param>
        /// <param name="index"> 設定の配列位置です。 </param>
        /// <param name="removeIndex"> 削除する配列位置です。 </param>
        private static void DrawDataAssetMapping(
            SerializedProperty mapping,
            int index,
            ref int removeIndex)
        {
            SerializedProperty addressableKey = mapping.FindPropertyRelative(DATA_ASSET_ADDRESSABLE_KEY_PROPERTY);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Data Asset {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("削除", GUILayout.Width(48f)))
            {
                removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(addressableKey, new GUIContent("Addressable Key"));
            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                addressableKey.stringValue,
                out ScriptableObject dataAsset))
            {
                EditorGUILayout.HelpBox("AddressableキーからScriptableObjectを解決できません。", MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Data Asset", dataAsset, dataAsset.GetType(), false);
            }

            string[] availablePaths = SourceDataProviderRepositoryResolver.GetCollectionPropertyPaths(dataAsset);
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
        /// <param name="dataAssets"> DataAsset設定配列です。 </param>
        /// <param name="index"> 設定の配列位置です。 </param>
        /// <param name="removeIndex"> 削除する配列位置です。 </param>
        private static void DrawCollectionMapping(
            SerializedProperty mapping,
            SerializedProperty dataAssets,
            int index,
            ref int removeIndex)
        {
            SerializedProperty collectionKey = mapping.FindPropertyRelative(COLLECTION_KEY_PROPERTY);
            SerializedProperty dataAssetKey = mapping.FindPropertyRelative(COLLECTION_DATA_ASSET_KEY_PROPERTY);
            SerializedProperty propertyPath = mapping.FindPropertyRelative(COLLECTION_PROPERTY_PATH_PROPERTY);
            SerializedProperty assetCreationDirectory =
                mapping.FindPropertyRelative(COLLECTION_ASSET_CREATION_DIRECTORY_PROPERTY);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Collection {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("削除", GUILayout.Width(48f)))
            {
                removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            DrawDataAssetSelector(dataAssetKey, dataAssets);
            EditorGUILayout.PropertyField(collectionKey, new GUIContent("Collection Key"));

            if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                dataAssetKey.stringValue,
                out ScriptableObject dataAsset))
            {
                EditorGUILayout.HelpBox("選択中のデータアセットを解決できません。", MessageType.Warning);
                EditorGUILayout.PropertyField(propertyPath, new GUIContent("Collection Property Path"));
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Resolved Data Asset", dataAsset, dataAsset.GetType(), false);
            }

            string[] availablePaths = SourceDataProviderRepositoryResolver.GetCollectionPropertyPaths(dataAsset);
            if (availablePaths.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    $"配列 / List 候補: {string.Join(", ", availablePaths)}",
                    MessageType.None);
            }

            DrawCollectionPathSelector(propertyPath, availablePaths);
            DrawAssetCreationDirectory(assetCreationDirectory);
        }

        /// <summary>
        ///     Collectionへ追加するScriptableObjectの生成先ディレクトリを描画します。
        /// </summary>
        /// <param name="directoryProperty"> 生成先ディレクトリを保持するプロパティです。 </param>
        private static void DrawAssetCreationDirectory(SerializedProperty directoryProperty)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(directoryProperty, new GUIContent("Asset Creation Directory"));
            if (GUILayout.Button("選択", GUILayout.Width(48f)))
            {
                string selectedDirectory = EditorUtility.OpenFolderPanel(
                    "アセット生成先を選択",
                    Application.dataPath,
                    string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedDirectory))
                {
                    directoryProperty.stringValue = FileUtil.GetProjectRelativePath(selectedDirectory);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrWhiteSpace(directoryProperty.stringValue)
                && (!directoryProperty.stringValue.StartsWith("Assets/", StringComparison.Ordinal)
                    || !AssetDatabase.IsValidFolder(directoryProperty.stringValue)))
            {
                EditorGUILayout.HelpBox(
                    "生成先には存在するAssets配下のフォルダを指定してください。",
                    MessageType.Warning);
            }
        }

        /// <summary>
        ///     DataAsset選択欄を描画します。
        /// </summary>
        /// <param name="dataAssetKey"> 選択結果を保存するプロパティです。 </param>
        /// <param name="dataAssets"> DataAsset設定配列です。 </param>
        private static void DrawDataAssetSelector(
            SerializedProperty dataAssetKey,
            SerializedProperty dataAssets)
        {
            List<string> labels = new() { "<未設定>" };
            List<string> values = new() { string.Empty };
            int selectedIndex = 0;

            for (int i = 0; i < dataAssets.arraySize; i++)
            {
                SerializedProperty mapping = dataAssets.GetArrayElementAtIndex(i);
                SerializedProperty addressableKey = mapping.FindPropertyRelative(DATA_ASSET_ADDRESSABLE_KEY_PROPERTY);
                string value = addressableKey.stringValue;
                labels.Add(string.IsNullOrWhiteSpace(value)
                    ? $"Data Asset {i + 1}"
                    : value);
                values.Add(value);
                if (string.Equals(value, dataAssetKey.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = values.Count - 1;
                }
            }

            if (!string.IsNullOrWhiteSpace(dataAssetKey.stringValue) && selectedIndex == 0)
            {
                labels.Add($"Missing: {dataAssetKey.stringValue}");
                values.Add(dataAssetKey.stringValue);
                selectedIndex = values.Count - 1;
            }

            EditorGUI.BeginChangeCheck();
            int nextIndex = EditorGUILayout.Popup("Data Asset", selectedIndex, labels.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                dataAssetKey.stringValue = values[nextIndex];
            }
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
            List<string> labels = new() { "<未設定>" };
            List<string> values = new() { string.Empty };
            int selectedIndex = 0;

            for (int i = 0; i < availablePaths.Length; i++)
            {
                labels.Add(availablePaths[i]);
                values.Add(availablePaths[i]);
                if (string.Equals(availablePaths[i], propertyPath.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = values.Count - 1;
                }
            }

            if (!string.IsNullOrWhiteSpace(propertyPath.stringValue) && selectedIndex == 0)
            {
                labels.Add($"Missing: {propertyPath.stringValue}");
                values.Add(propertyPath.stringValue);
                selectedIndex = values.Count - 1;
            }

            EditorGUI.BeginChangeCheck();
            int nextIndex = EditorGUILayout.Popup("Collection Property Path", selectedIndex, labels.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                propertyPath.stringValue = values[nextIndex];
            }
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "Source Data Provider";
        private const string DATA_ASSET_MAPPINGS_PROPERTY = "_dataAssetMappings";
        private const string SOURCE_COLLECTION_MAPPINGS_PROPERTY = "_sourceCollectionMappings";
        private const string DATA_ASSET_ADDRESSABLE_KEY_PROPERTY = "_addressableKey";
        private const string COLLECTION_KEY_PROPERTY = "_collectionKey";
        private const string COLLECTION_DATA_ASSET_KEY_PROPERTY = "_dataAssetAddressableKey";
        private const string COLLECTION_PROPERTY_PATH_PROPERTY = "_propertyPath";
        private const string COLLECTION_ASSET_CREATION_DIRECTORY_PROPERTY = "_assetCreationDirectory";
    }
}
