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

                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

                string buildPath = $"{path}{profile.name}/";
                BuildPlayerOptions options = new()
                {
                    scenes = scenes,
                    target = target,                     
                    locationPathName = buildPath + profile.name + GetExtension(target)
                };

                if (!Directory.Exists(buildPath)) { Directory.CreateDirectory(buildPath); }

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