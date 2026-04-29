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

            SerializedProperty masterPath = so.FindProperty(nameof(AutoBuilderSettings.instance.MasterPath));
            SerializedProperty masterProp = so.FindProperty(nameof(AutoBuilderSettings.instance.MasterBuildProfiles));
            
            SerializedProperty devPath = so.FindProperty(nameof(AutoBuilderSettings.instance.DevelopPath));
            SerializedProperty devProp = so.FindProperty(nameof(AutoBuilderSettings.instance.DevelopBuildProfiles));

            EditorGUI.BeginChangeCheck();

            masterPath.stringValue = EditorGUILayout.TextField(masterPath.stringValue);
            EditorGUILayout.PropertyField(masterProp, true);

            EditorGUILayout.Space(10);
            devPath.stringValue = EditorGUILayout.TextField(devPath.stringValue);
            EditorGUILayout.PropertyField(devProp, true);

            if (EditorGUI.EndChangeCheck()) { AutoBuilderSettings.Save(); }

            so.ApplyModifiedProperties();
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "AutoBuilder";
    }
}