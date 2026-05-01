using KillChord.Editor.Utility;
using UnityEditor;
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

            if (!AutoBuilderSettings.IsPathValid(settings.MasterPath))
            {
                EditorGUILayout.HelpBox("MasterPathが不正です。", MessageType.Error);
            }
            else
            {
                if (GUILayout.Button("Master Build"))
                {
                    AutoBuildExecuter.Run(
                        settings.MasterPath,
                        settings.MasterBuildProfiles);
                }
            }

            if (!AutoBuilderSettings.IsPathValid(settings.DevelopPath))
            {
                EditorGUILayout.HelpBox("DevelopPathが不正です。", MessageType.Error);
            }
            else
            {
                if (GUILayout.Button("Develop Build"))
                {
                    AutoBuildExecuter.Run(
                        settings.DevelopPath,
                        settings.DevelopBuildProfiles);
                }
            }
        }
    }
}