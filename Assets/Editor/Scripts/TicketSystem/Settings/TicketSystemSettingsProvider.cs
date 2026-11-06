using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.TicketSystem
{
    /// <summary>
    ///     Project Settings にチケットシステムの設定ページを提供する。
    /// </summary>
    public class TicketSystemSettingsProvider : SettingsProvider
    {
        /// <summary>
        ///     チケットシステムの設定プロバイダーを生成する。
        /// </summary>
        private TicketSystemSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords)
        {
        }

        /// <summary>
        ///     Project Settings にチケットシステムの設定ページを登録する。
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new TicketSystemSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        /// <summary>
        ///     設定ページを開いたときに、設定アセットを編集できる状態にしてインスペクターを生成する。
        /// </summary>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = TicketSystemSettings.instance;
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            UnityEditor.Editor.CreateCachedEditor(settings, null, ref _editor);
        }

        /// <summary>
        ///     設定のインスペクターを描画し、変更があれば保存する。
        /// </summary>
        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();
            _editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                TicketSystemSettings.instance.Save();
            }

            DrawApiKeySection(TicketSystemSettings.instance);
        }

        private const string SETTINGS_PATH = TicketSystemConst.TICKET_SYSTEM_PROJECT_PATH + "Editor";

        private UnityEditor.Editor _editor;

        /// <summary>
        ///     API キーの入力欄を描画する。値は共通秘密設定ファイルへ保存する。
        ///     移行前の値が設定に残っている場合は、移行ボタンを出す。
        /// </summary>
        /// <param name="settings"> チケットシステムの設定。 </param>
        private static void DrawApiKeySection(TicketSystemSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"API キーの保存先: {SharedSecrets.FullPath}", EditorStyles.miniLabel);

            if (!string.IsNullOrEmpty(settings.ApiKey))
            {
                EditorGUILayout.HelpBox(
                    "移行前の設定に API キーが残っています。共通ファイルへ移すと、ほかの秘密設定と同じファイルで管理できます。",
                    MessageType.Warning);
                if (GUILayout.Button("共通ファイルへ移す") && !settings.MigrateToSharedSecrets())
                {
                    EditorUtility.DisplayDialog(
                        "TicketSystem",
                        "共通ファイルに別の API キーがすでにあるため、移しませんでした。どちらを使うか確認してください。",
                        "OK");
                }
            }

            // API キーの入力欄を描画し、変更があれば共通ファイルへ保存する。
            EditorGUI.BeginChangeCheck();
            string apiKey = EditorGUILayout.PasswordField(
                "Api Key",
                SharedSecrets.GetValue(SharedSecrets.TICKET_SYSTEM_API_KEY));
            if (EditorGUI.EndChangeCheck())
            {
                SharedSecrets.SetValue(SharedSecrets.TICKET_SYSTEM_API_KEY, apiKey);
            }
        }
    }
}