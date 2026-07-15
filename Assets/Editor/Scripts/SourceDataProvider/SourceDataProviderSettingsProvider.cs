using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderのカテゴリとリポジトリ対応を編集します。
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
            SerializedObject serializedObject = new(settings);
            SerializedProperty mappings = serializedObject.FindProperty(REPOSITORY_MAPPINGS_PROPERTY);

            EditorGUILayout.HelpBox(
                "DataCategoryごとにAddressableリポジトリと個別データ配列を登録します。",
                MessageType.Info);

            int removeIndex = -1;
            for (int i = 0; i < mappings.arraySize; i++)
            {
                SerializedProperty mapping = mappings.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawMapping(mapping, i, ref removeIndex);
                EditorGUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                mappings.DeleteArrayElementAtIndex(removeIndex);
            }

            if (GUILayout.Button("カテゴリを追加"))
            {
                mappings.InsertArrayElementAtIndex(mappings.arraySize);
                SerializedProperty mapping = mappings.GetArrayElementAtIndex(mappings.arraySize - 1);
                mapping.FindPropertyRelative(CATEGORY_PROPERTY).stringValue = string.Empty;
                mapping.FindPropertyRelative(ADDRESSABLE_KEY_PROPERTY).stringValue = string.Empty;
                mapping.FindPropertyRelative(ARRAY_PROPERTY_PATH_PROPERTY).stringValue = string.Empty;
            }

            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("設定を適用"))
            {
                settings.SaveSettings();
            }
        }

        /// <summary>
        ///     1件分のリポジトリ対応設定を描画します。
        /// </summary>
        /// <param name="mapping"> 描画対象の設定です。 </param>
        /// <param name="index"> 設定の配列位置です。 </param>
        /// <param name="removeIndex"> 削除する配列位置です。 </param>
        private static void DrawMapping(
            SerializedProperty mapping,
            int index,
            ref int removeIndex)
        {
            SerializedProperty category = mapping.FindPropertyRelative(CATEGORY_PROPERTY);
            SerializedProperty addressableKey = mapping.FindPropertyRelative(ADDRESSABLE_KEY_PROPERTY);
            SerializedProperty arrayPropertyPath = mapping.FindPropertyRelative(ARRAY_PROPERTY_PATH_PROPERTY);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Category {index + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("削除", GUILayout.Width(48f)))
            {
                removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(category, new GUIContent("Category"));
            EditorGUILayout.PropertyField(addressableKey, new GUIContent("Addressable Key"));

            if (!SourceDataProviderRepositoryResolver.TryResolveRepository(
                addressableKey.stringValue,
                out UnityEngine.Object repository))
            {
                EditorGUILayout.HelpBox("Addressableキーからリポジトリを解決できません。", MessageType.Warning);
                EditorGUILayout.PropertyField(arrayPropertyPath, new GUIContent("Array Property Path"));
                return;
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Repository", repository, repository.GetType(), false);
            }
            if (GUILayout.Button("Ping", GUILayout.Width(48f)))
            {
                EditorGUIUtility.PingObject(repository);
            }
            EditorGUILayout.EndHorizontal();

            string[] arrayPaths = SourceDataProviderRepositoryResolver.GetArrayPropertyPaths(repository);
            DrawArrayPathSelector(arrayPropertyPath, arrayPaths);
        }

        /// <summary>
        ///     リポジトリ内の配列プロパティ選択欄を描画します。
        /// </summary>
        /// <param name="arrayPropertyPath"> 選択結果を保存するプロパティです。 </param>
        /// <param name="arrayPaths"> 選択可能な配列プロパティパスです。 </param>
        private static void DrawArrayPathSelector(
            SerializedProperty arrayPropertyPath,
            string[] arrayPaths)
        {
            string[] labels = new string[arrayPaths.Length + 1];
            labels[0] = "<アセット自体>";
            int selectedIndex = 0;

            for (int i = 0; i < arrayPaths.Length; i++)
            {
                labels[i + 1] = arrayPaths[i];
                if (string.Equals(arrayPaths[i], arrayPropertyPath.stringValue, StringComparison.Ordinal))
                {
                    selectedIndex = i + 1;
                }
            }

            int nextIndex = EditorGUILayout.Popup("Data Array", selectedIndex, labels);
            arrayPropertyPath.stringValue = nextIndex <= 0
                ? string.Empty
                : arrayPaths[nextIndex - 1];
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "Source Data Provider";
        private const string REPOSITORY_MAPPINGS_PROPERTY = "_repositoryMappings";
        private const string CATEGORY_PROPERTY = "_category";
        private const string ADDRESSABLE_KEY_PROPERTY = "_addressableKey";
        private const string ARRAY_PROPERTY_PATH_PROPERTY = "_arrayPropertyPath";
    }
}
