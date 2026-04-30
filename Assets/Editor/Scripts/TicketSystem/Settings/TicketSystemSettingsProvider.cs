using KillChord.Editor.Utility;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.TicketSystem
{
    public class TicketSystemSettingsProvider : SettingsProvider
    {
        private TicketSystemSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new TicketSystemSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = TicketSystemSettings.instance;
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            UnityEditor.Editor.CreateCachedEditor(settings, null, ref _editor);
        }

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