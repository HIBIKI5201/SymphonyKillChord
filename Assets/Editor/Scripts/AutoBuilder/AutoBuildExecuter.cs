using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    public static class AutoBuildExecuter
    {
        public static void Run(params BuildProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError("Master Build Profiles are not set.");
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

                BuildPlayerWithProfileOptions options = new() { buildProfile = profile };
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
    }
}