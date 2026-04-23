using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevelopProducts.TicketSystem
{
    public class TicketSystemSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Project/TicketWindowSettings/Editor";
        private Editor _editor;

        private TicketSystemSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
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
            Editor.CreateCachedEditor(settings, null, ref _editor);
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
    }
}
