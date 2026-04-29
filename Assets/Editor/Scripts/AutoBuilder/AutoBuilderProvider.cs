using KillChord.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     オートビルダーの設定画面。
    /// </summary>
    public class AutoBuilderProvider : SettingsProvider
    {
        public AutoBuilderProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new AutoBuilderProvider(SETTINGS_PATH, SettingsScope.Project);
        }

        public override void OnGUI(string searchContext)
        {
            SerializedObject so = new SerializedObject(AutoBuilderSettings.instance);
            SerializedProperty prop = so.FindProperty("BuildProfiles");

            EditorGUILayout.PropertyField(prop, true);

            so.ApplyModifiedProperties();
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "AutoBuilder";
    }
}