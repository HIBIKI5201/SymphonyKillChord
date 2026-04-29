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
            SerializedProperty masterProp = so.FindProperty(nameof(AutoBuilderSettings.instance.MasterBuildProfiles));
            SerializedProperty devProp = so.FindProperty(nameof(AutoBuilderSettings.instance.DevelopBuildProfiles));

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(masterProp, true);
            EditorGUILayout.PropertyField(devProp, true);

            if (EditorGUI.EndChangeCheck()) { AutoBuilderSettings.Save(); }

            so.ApplyModifiedProperties();
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "AutoBuilder";
    }
}