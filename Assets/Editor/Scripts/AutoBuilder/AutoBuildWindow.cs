using KillChord.Editor.Utility;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     自動ビルドの実行と状態確認を行うエディタウィンドウ。
    /// </summary>
    public class AutoBuildWindow : EditorWindow
    {
        /// <summary>
        ///     AutoBuilder ウィンドウを開く。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "AutoBuilder")]
        public static void ShowWindow()
        {
            GetWindow<AutoBuildWindow>("AutoBuilder");
        }

        /// <summary>
        ///     ウィンドウ有効化時の処理。現在は何もしない。
        /// </summary>
        private void OnEnable()
        {
        }

        /// <summary>
        ///     ビルド設定の確認と実行ボタンを描画する。
        /// </summary>
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
