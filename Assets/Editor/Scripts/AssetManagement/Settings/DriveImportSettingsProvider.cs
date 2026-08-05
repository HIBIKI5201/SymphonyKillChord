using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    /// Project Settings UI の Drive Import ページを提供する。
    /// </summary>
    internal static class DriveImportSettingsProvider
    {
        /// <summary>
        /// Project Settings に Drive Import ページを登録する。
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
        /// 機密情報セクション (Service Account JSON Key と取得元フォルダ) を描画する。
        /// </summary>
        private static void DrawSecretsSection()
        {
            var secrets = DriveImportSecrets.instance;
            var serialized = new SerializedObject(secrets);
            serialized.Update();

            EditorGUILayout.LabelField("認証・取得元 (機密情報 / UserSettings管理・Git対象外)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Service Account JSON Key");

            EditorGUI.BeginChangeCheck();
            float width = EditorGUIUtility.currentViewWidth * 0.75f;
            var newApiKey = EditorGUILayout.TextArea(secrets.serviceAccountJsonKey,
                GUILayout.Width(width));
            var changedJsonKey = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                serialized.FindProperty("sourceFolders"),
                new GUIContent("Source Folders"), true);
            var changedFolders = EditorGUI.EndChangeCheck();

            if (changedFolders)
            {
                serialized.ApplyModifiedProperties();
            }

            if (changedJsonKey || changedFolders)
            {
                secrets.serviceAccountJsonKey = newApiKey;
                secrets.Persist();
            }
            
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy Secrets"))
                {
                    string json = JsonUtility.ToJson(secrets);
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

                        secrets.Persist();

                        GUI.FocusControl(null);

                        EditorUtility.DisplayDialog(
                            "Drive Import",
                            "DriveImportSecretsを読み込みました。",
                            "OK");
                    }
                    catch
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
        /// 共有設定セクション (除外パターン) を描画する。
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

            serialized.ApplyModifiedProperties();

            if (changed)
            {
                settings.Persist();
            }
        }
    }
}