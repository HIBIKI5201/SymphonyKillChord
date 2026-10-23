using System;
using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    ///     Project Settings UI の Drive Import ページを提供する。
    /// </summary>
    internal static class DriveImportSettingsProvider
    {
        /// <summary>
        ///     Project Settings に Drive Import ページを登録する。
        /// </summary>
        /// <returns> 設定ページプロバイダー。 </returns>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider(ProviderConst.PROJECT_PATH + nameof(DriveImportSettings),
                SettingsScope.Project)
            {
                label = "Drive Import",
                guiHandler = (_) =>
                {
                    DrawSecretsSection();
                    EditorGUILayout.Space();
                    DrawSharedSettingsSection();
                },
                keywords = new HashSet<string>(new[] { "Drive", "Import", "Google", "Sync" })
            };
            return provider;
        }

        /// <summary>
        ///     機密情報セクション (Service Account JSON Key と取得元フォルダ) を描画する。
        ///     JSON 鍵は共通秘密設定ファイルへ、取得元フォルダは DriveImportSecrets へ保存する。
        /// </summary>
        private static void DrawSecretsSection()
        {
            var secrets = DriveImportSecrets.instance;
            var serialized = new SerializedObject(secrets);
            serialized.Update();

            EditorGUILayout.LabelField("認証・取得元 (機密情報 / UserSettings管理・Git対象外)", EditorStyles.boldLabel);
            DrawJsonKeySection(secrets);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                serialized.FindProperty("sourceFolders"),
                new GUIContent("Source Folders"), true);
            if (EditorGUI.EndChangeCheck())
            {
                serialized.ApplyModifiedProperties();
                secrets.Persist();
            }
            
            EditorGUILayout.Space();

            // 機密情報を他のマシンへ受け渡すためのコピー・貼り付けボタン。
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy Secrets"))
                {
                    // JSON 鍵は共通秘密設定ファイルにあるため、コピー時だけ埋めて渡す。
                    // 保存はせず、シリアライズする間だけ値を差し替える。
                    string legacyJsonKey = secrets.serviceAccountJsonKey;
                    secrets.serviceAccountJsonKey = secrets.GetServiceAccountJsonKey();
                    string json = JsonUtility.ToJson(secrets);
                    secrets.serviceAccountJsonKey = legacyJsonKey;
                    EditorGUIUtility.systemCopyBuffer = json;

                    EditorUtility.DisplayDialog(
                        "Drive Import",
                        "DriveImportSecretsをクリップボードへコピーしました。",
                        "OK");
                }

                if (GUILayout.Button("Paste Secrets"))
                {
                    try
                    {
                        JsonUtility.FromJsonOverwrite(EditorGUIUtility.systemCopyBuffer, secrets);

                        // 貼り付けた JSON 鍵は共通秘密設定ファイルへ書き、この設定には残さない。
                        if (!string.IsNullOrEmpty(secrets.serviceAccountJsonKey))
                        {
                            SharedSecrets.SetValue(
                                SharedSecrets.GOOGLE_SERVICE_ACCOUNT_JSON_KEY,
                                secrets.serviceAccountJsonKey);
                            secrets.serviceAccountJsonKey = "";
                        }

                        secrets.Persist();

                        GUI.FocusControl(null);

                        EditorUtility.DisplayDialog(
                            "Drive Import",
                            "DriveImportSecretsを読み込みました。",
                            "OK");
                    }
                    catch(ArgumentException)
                    {
                        EditorUtility.DisplayDialog(
                            "Drive Import",
                            "クリップボードの内容を読み込めませんでした。",
                            "OK");
                    }
                }
            }
        }

        /// <summary>
        ///     Service Account JSON Key の入力欄を描画する。値は共通秘密設定ファイルへ保存する。
        ///     移行前の値が DriveImportSecrets に残っている場合は、移行ボタンを出す。
        /// </summary>
        /// <param name="secrets"> 取得元フォルダなどを保持する設定。 </param>
        private static void DrawJsonKeySection(DriveImportSecrets secrets)
        {
            EditorGUILayout.LabelField("Service Account JSON Key");
            EditorGUILayout.LabelField($"保存先: {SharedSecrets.FullPath}", EditorStyles.miniLabel);

            if (!string.IsNullOrEmpty(secrets.serviceAccountJsonKey))
            {
                EditorGUILayout.HelpBox(
                    "移行前の設定に JSON 鍵が残っています。共通ファイルへ移すと、SinfoniaOperator の CLI と同じファイルで管理できます。",
                    MessageType.Warning);
                if (GUILayout.Button("共通ファイルへ移す") && !secrets.MigrateToSharedSecrets())
                {
                    EditorUtility.DisplayDialog(
                        "Drive Import",
                        "共通ファイルに別の JSON 鍵がすでにあるため、移しませんでした。どちらを使うか確認してください。",
                        "OK");
                }
            }

            // JSON 鍵の入力欄を描画し、変更があれば共通ファイルへ保存する。
            EditorGUI.BeginChangeCheck();
            float width = EditorGUIUtility.currentViewWidth * 0.75f;
            var newJsonKey = EditorGUILayout.TextArea(
                SharedSecrets.GetValue(SharedSecrets.GOOGLE_SERVICE_ACCOUNT_JSON_KEY),
                GUILayout.Width(width));
            if (EditorGUI.EndChangeCheck())
            {
                SharedSecrets.SetValue(SharedSecrets.GOOGLE_SERVICE_ACCOUNT_JSON_KEY, newJsonKey);
            }
        }

        /// <summary>
        ///     共有設定セクション (除外パターン) を描画する。
        /// </summary>
        private static void DrawSharedSettingsSection()
        {
            var settings = DriveImportSettings.instance;
            var serialized = new SerializedObject(settings);
            serialized.Update();

            EditorGUILayout.LabelField("除外設定 (共有情報 / ProjectSettings管理・Git対象)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                serialized.FindProperty("excludeFolderNames"),
                new GUIContent("Exclude Folder Names"), true);
            EditorGUILayout.PropertyField(
                serialized.FindProperty("excludeExtensions"),
                new GUIContent("Exclude Extensions"), true);
            EditorGUILayout.PropertyField(
                serialized.FindProperty("excludeFilePatterns"),
                new GUIContent("Exclude File Patterns"), true);
            var changed = EditorGUI.EndChangeCheck();


            if (changed)
            {
                serialized.ApplyModifiedProperties();
                settings.Persist();
            }
        }
    }
}