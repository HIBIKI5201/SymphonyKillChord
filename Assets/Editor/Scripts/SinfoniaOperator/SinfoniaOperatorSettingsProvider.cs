using KillChord.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     SinfoniaOperatorの設定をProjectSettingsに表示するプロバイダー。
    /// </summary>
    public class SinfoniaOperatorSettingsProvider : SettingsProvider
    {
        private SinfoniaOperatorSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SinfoniaOperatorSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = SinfoniaOperatorSettings.instance;
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            UnityEditor.Editor.CreateCachedEditor(settings, null, ref _editor);
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();
            _editor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                SinfoniaOperatorSettings.instance.Save();
            }
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "SinfoniaOperator";

        private UnityEditor.Editor _editor;
    }
}
