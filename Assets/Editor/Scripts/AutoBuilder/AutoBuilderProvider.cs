using KillChord.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Profile;

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
            AutoBuilderSettings settings = AutoBuilderSettings.instance;
            SerializedObject so = new SerializedObject(settings);

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < AutoBuilderSettings.SLOTS.Length; i++)
            {
                if (i > 0)
                {
                    EditorGUILayout.Space(SLOT_SPACING);
                }

                DrawSlot(so, settings, AutoBuilderSettings.SLOTS[i]);
            }

            so.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck()) { AutoBuilderSettings.Save(); }
        }

        private const string SETTINGS_PATH = ProviderConst.PROJECT_PATH + "AutoBuilder";
        private const float SLOT_SPACING = 10.0f;

        /// <summary>
        ///     一つの枠の出力先パスとBuild Profile一覧を描画します。
        /// </summary>
        /// <param name="so"> 設定のSerializedObjectです。 </param>
        /// <param name="settings"> 設定インスタンスです。 </param>
        /// <param name="slot"> 描画する枠です。 </param>
        private static void DrawSlot(SerializedObject so, AutoBuilderSettings settings, AutoBuildSlot slot)
        {
            SerializedProperty pathProperty = so.FindProperty(slot.PathPropertyName);
            SerializedProperty profilesProperty = so.FindProperty(slot.ProfilesPropertyName);
            BuildProfile[] profiles = settings.GetProfiles(slot);

            EditorGUILayout.LabelField(slot.Label, EditorStyles.boldLabel);

            pathProperty.stringValue = EditorGUILayout.TextField(pathProperty.stringValue);
            if (AutoBuilderSettings.IsPathNullOrEmpty(pathProperty.stringValue)) { EditorGUILayout.HelpBox($"{slot.PathPropertyName}が空です。", MessageType.Warning); }
            if (!AutoBuilderSettings.IsPathEndsWithSlash(pathProperty.stringValue)) { EditorGUILayout.HelpBox($"{slot.PathPropertyName}の末尾にスラッシュがありません。", MessageType.Warning); }
            if (AutoBuilderSettings.IsBuildProfilesNullOrEmpty(profiles)) { EditorGUILayout.HelpBox($"{slot.ProfilesPropertyName}が空です。", MessageType.Warning); }
            if (!AutoBuilderSettings.IsBuildProfilesNullOrEmpty(profiles) &&
                AutoBuilderSettings.HasEmptyBuildProfile(profiles)) { EditorGUILayout.HelpBox($"{slot.ProfilesPropertyName}に空のビルドプロファイルがあります。", MessageType.Warning); }
            if (!AutoBuilderSettings.IsBuildProfilesNullOrEmpty(profiles) &&
                AutoBuilderSettings.HasDuplicateBuildProfiles(profiles)) { EditorGUILayout.HelpBox($"{slot.ProfilesPropertyName}に重複するビルドプロファイルがあります。", MessageType.Warning); }

            EditorGUILayout.PropertyField(profilesProperty, true);
        }
    }
}
