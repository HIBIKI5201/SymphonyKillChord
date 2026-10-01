using System.Collections.Generic;
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
        }

        private const string SETTINGS_PATH = TicketSystemConst.TICKET_SYSTEM_PROJECT_PATH + "Editor";

        private UnityEditor.Editor _editor;
    }
}