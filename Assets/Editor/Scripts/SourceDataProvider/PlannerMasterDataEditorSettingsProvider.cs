using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     プランナー向けマスターデータ画面のページ定義を編集します。
    /// </summary>
    internal sealed class PlannerMasterDataEditorSettingsProvider : SettingsProvider
    {
        /// <summary>
        ///     設定画面を初期化します。
        /// </summary>
        /// <param name="path"> Settings画面内のパスです。 </param>
        /// <param name="scopes"> 設定スコープです。 </param>
        /// <param name="keywords"> 検索キーワードです。 </param>
        private PlannerMasterDataEditorSettingsProvider(
            string path,
            SettingsScope scopes,
            IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        /// <summary>
        ///     設定画面を生成します。
        /// </summary>
        /// <returns> 設定プロバイダーです。 </returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new PlannerMasterDataEditorSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        /// <summary>
        ///     設定画面を描画します。
        /// </summary>
        /// <param name="searchContext"> 検索文字列です。 </param>
        public override void OnGUI(string searchContext)
        {
            PlannerMasterDataEditorSettings settings = PlannerMasterDataEditorSettings.instance;
            _ = settings.Pages.Count;

            SerializedObject serializedObject = new(settings);
            SerializedProperty pagesProperty = serializedObject.FindProperty(PAGES_PROPERTY_NAME);

            EditorGUILayout.HelpBox(
                "ここでページを定義すると、プランナーウィンドウのボタンと表示対象が切り替わります。"
                + " 新しいデータ型を追加する場合は、まずSourceDataProvider側へ登録し、その後この画面でページへ割り当ててください。",
                MessageType.Info);

            DrawPages(pagesProperty);

            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("設定を適用"))
            {
                settings.SaveSettings();
            }
        }

        /// <summary>
        ///     ページ一覧を描画します。
        /// </summary>
        /// <param name="pagesProperty"> ページ一覧のシリアライズドプロパティです。 </param>
        private static void DrawPages(SerializedProperty pagesProperty)
        {
            int removeIndex = -1;
            string[] sourceAssetKeys = GetSourceAssetKeys();
            string[] collectionKeys = GetCollectionKeys();

            for (int i = 0; i < pagesProperty.arraySize; i++)
            {
                SerializedProperty pageProperty = pagesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawPage(pageProperty, i, sourceAssetKeys, collectionKeys, ref removeIndex);
                EditorGUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                pagesProperty.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button("ページを追加"))
            {
                pagesProperty.InsertArrayElementAtIndex(pagesProperty.arraySize);
                SerializedProperty newPage = pagesProperty.GetArrayElementAtIndex(pagesProperty.arraySize - 1);
                newPage.FindPropertyRelative(DISPLAY_NAME_PROPERTY_NAME).stringValue = "New Page";
                newPage.FindPropertyRelative(SOURCE_ASSET_KEYS_PROPERTY_NAME).ClearArray();
                newPage.FindPropertyRelative(COLLECTION_CATEGORIES_PROPERTY_NAME).ClearArray();
            }
        }

        /// <summary>
        ///     1ページ分の設定を描画します。
        /// </summary>
        /// <param name="pageProperty"> ページ設定です。 </param>
        /// <param name="index"> 配列位置です。 </param>
        /// <param name="sourceAssetKeys"> 選択可能なSourceAssetキー一覧です。 </param>
        /// <param name="collectionKeys"> 選択可能なCollectionKey一覧です。 </param>
        /// <param name="removeIndex"> 削除対象の配列位置です。 </param>
        private static void DrawPage(
            SerializedProperty pageProperty,
            int index,
            string[] sourceAssetKeys,
            string[] collectionKeys,
            ref int removeIndex)
        {
            SerializedProperty displayNameProperty =
                pageProperty.FindPropertyRelative(DISPLAY_NAME_PROPERTY_NAME);
            SerializedProperty sourceAssetKeysProperty =
                pageProperty.FindPropertyRelative(SOURCE_ASSET_KEYS_PROPERTY_NAME);
            SerializedProperty collectionCategoriesProperty =
                pageProperty.FindPropertyRelative(COLLECTION_CATEGORIES_PROPERTY_NAME);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Page {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("削除", GUILayout.Width(48f)))
            {
                removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Display Name"));
            DrawSelectableStringList(
                "Source Assets",
                sourceAssetKeysProperty,
                sourceAssetKeys,
                SOURCE_ASSET_ITEM_LABEL);
            DrawSelectableStringList(
                "Collections",
                collectionCategoriesProperty,
                collectionKeys,
                COLLECTION_ITEM_LABEL);
        }

        /// <summary>
        ///     文字列選択リストを描画します。
        /// </summary>
        /// <param name="label"> セクション名です。 </param>
        /// <param name="listProperty"> 対象リストです。 </param>
        /// <param name="options"> 選択肢です。 </param>
        /// <param name="itemLabel"> 要素ラベルです。 </param>
        private static void DrawSelectableStringList(
            string label,
            SerializedProperty listProperty,
            string[] options,
            string itemLabel)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

            int removeIndex = -1;
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = listProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                DrawOptionPopup(itemProperty, options, $"{itemLabel} {i + 1}");
                if (GUILayout.Button("削除", GUILayout.Width(48f)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
            {
                listProperty.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button($"{label}を追加"))
            {
                listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                SerializedProperty itemProperty = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                itemProperty.stringValue = string.Empty;
            }
        }

        /// <summary>
        ///     文字列候補からの選択ポップアップを描画します。
        /// </summary>
        /// <param name="itemProperty"> 選択結果を保持するプロパティです。 </param>
        /// <param name="options"> 選択肢一覧です。 </param>
        /// <param name="label"> フィールドラベルです。 </param>
        private static void DrawOptionPopup(
            SerializedProperty itemProperty,
            string[] options,
            string label)
        {
            string[] labels = new string[options.Length + 1];
            labels[0] = "<未設定>";
            int selectedIndex = 0;

            for (int i = 0; i < options.Length; i++)
            {
                labels[i + 1] = options[i];
                if (string.Equals(options[i], itemProperty.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i + 1;
                }
            }

            int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
            itemProperty.stringValue = nextIndex <= 0 ? string.Empty : options[nextIndex - 1];
        }

        /// <summary>
        ///     選択可能なSourceAssetキー一覧を取得します。
        /// </summary>
        /// <returns> SourceAssetキー一覧です。 </returns>
        private static string[] GetSourceAssetKeys()
        {
            IReadOnlyList<SourceDataProviderSettings.SourceAssetMapping> mappings =
                SourceDataProviderSettings.instance.SourceAssetMappings;
            string[] results = new string[mappings.Count];
            for (int i = 0; i < mappings.Count; i++)
            {
                results[i] = mappings[i].AddressableKey;
            }

            Array.Sort(results, StringComparer.Ordinal);
            return results;
        }

        /// <summary>
        ///     選択可能なCollectionKey一覧を取得します。
        /// </summary>
        /// <returns> CollectionKey一覧です。 </returns>
        private static string[] GetCollectionKeys()
        {
            IReadOnlyList<SourceDataProviderSettings.SourceCollectionMapping> mappings =
                SourceDataProviderSettings.instance.SourceCollectionMappings;
            List<string> results = new();
            HashSet<string> visited = new(StringComparer.Ordinal);

            for (int i = 0; i < mappings.Count; i++)
            {
                string collectionKey = mappings[i].CollectionKey;
                if (string.IsNullOrWhiteSpace(collectionKey) || !visited.Add(collectionKey))
                {
                    continue;
                }

                results.Add(collectionKey);
            }

            results.Sort(StringComparer.Ordinal);
            return results.ToArray();
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "Planner Master Data";
        private const string PAGES_PROPERTY_NAME = "_pages";
        private const string DISPLAY_NAME_PROPERTY_NAME = "_displayName";
        private const string SOURCE_ASSET_KEYS_PROPERTY_NAME = "_sourceAssetAddressableKeys";
        private const string COLLECTION_CATEGORIES_PROPERTY_NAME = "_collectionCategories";
        private const string SOURCE_ASSET_ITEM_LABEL = "Source Asset";
        private const string COLLECTION_ITEM_LABEL = "Collection";
    }
}
