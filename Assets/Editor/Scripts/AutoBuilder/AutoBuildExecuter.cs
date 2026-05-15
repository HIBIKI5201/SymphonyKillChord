using SymphonyFrameWork.Utility;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    public static class AutoBuildExecuter
    {
        public static void Run(string path, params BuildProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError("Build Profiles are not set.");
                return;
            }

            foreach (BuildProfile profile in profiles)
            {
                if (profile == null)
                {
                    Debug.LogWarning("BuildProfile is null. Skipping.");
                    continue;
                }

                Debug.Log($"Start Build: {profile.name}");

                BuildProfile.SetActiveBuildProfile(profile);

                string[] scenes = profile.GetScenesForBuild()
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                // プロファイルにシーンが設定されていない場合、ビルド設定の有効なシーンを使用する。
                if (scenes.Length == 0)
                {
                    scenes = EditorBuildSettings.scenes
                        .Where(s => s.enabled)
                        .Select(s => s.path)
                        .ToArray();
                }

                if (scenes.Length == 0)
                {
                    Debug.LogWarning($"No enabled scenes found in profile: {profile.name}. Skipping build.");
                    continue;
                }

                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

                string buildDir = Path.Combine(path, profile.name);
                string fileName = Application.productName + GetExtension(target);

                string locationPath = Path.Combine(buildDir, fileName);
                BuildPlayerOptions options = new()
                {
                    scenes = scenes,
                    target = target,
                    locationPathName = locationPath
                };


                if (!Directory.Exists(buildDir)) { Directory.CreateDirectory(buildDir); }

                BuildReport report = BuildPipeline.BuildPlayer(options);

                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"Build Failed: {profile.name}");
                }
                else
                {
                    Debug.Log($"Build Succeeded: {profile.name}");
                }
            }
        }

        private static string GetExtension(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.Android => ".apk",
                BuildTarget.StandaloneOSX => ".app",
                _ => ""
            };
        }
    }
}