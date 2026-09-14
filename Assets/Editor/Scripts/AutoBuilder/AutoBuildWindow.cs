using KillChord.Editor.Utility;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    public class AutoBuildWindow : EditorWindow
    {
        [MenuItem(ToolConst.TOOLS_PATH + "AutoBuilder")]
        public static void ShowWindow()
        {
            GetWindow<AutoBuildWindow>("AutoBuilder");
        }

        private void OnEnable()
        {
        }

        private void OnGUI()
        {
            AutoBuilderSettings settings = AutoBuilderSettings.instance;
            bool isRunning = AutoBuildExecuter.IsRunning;

            if (isRunning)
            {
                EditorGUILayout.HelpBox("自動ビルドを実行中です。完了までお待ちください。", MessageType.Info);
            }

            EditorGUI.BeginDisabledGroup(isRunning);

            foreach (AutoBuildSlot slot in AutoBuilderSettings.SLOTS)
            {
                DrawSlotButton(settings, slot);
            }

            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        ///     枠の設定を検証し、有効な場合はビルドボタンを描画します。
        /// </summary>
        /// <param name="settings"> 設定インスタンスです。 </param>
        /// <param name="slot"> 描画する枠です。 </param>
        private static void DrawSlotButton(AutoBuilderSettings settings, AutoBuildSlot slot)
        {
            string path = settings.GetPath(slot);
            BuildProfile[] profiles = settings.GetProfiles(slot);

            if (!AutoBuilderSettings.IsPathValid(path))
            {
                EditorGUILayout.HelpBox($"{slot.PathPropertyName}が不正です。", MessageType.Error);
                return;
            }

            if (!AutoBuilderSettings.IsBuildProfilesValid(profiles))
            {
                EditorGUILayout.HelpBox($"{slot.ProfilesPropertyName}が不正です。", MessageType.Error);
                return;
            }

            if (GUILayout.Button($"{slot.Label} Build"))
            {
                AutoBuildExecuter.Run(path, profiles);
            }
        }
    }
}
