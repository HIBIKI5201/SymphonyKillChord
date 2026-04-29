using UnityEditor;

namespace KillChord.Editor.AutoBuilder
{
    public static class AutoBuildExecuter
    {
        public static void BuildMaster()
        {
            var settings = AutoBuilderSettings.instance;
            if (settings.MasterBuildProfiles == null || settings.MasterBuildProfiles.Length == 0)
            {
                UnityEngine.Debug.LogError("Master Build Profiles are not set.");
                return;
            }

            foreach (var profile in settings.MasterBuildProfiles)
            {
                BuildPipeline.BuildPlayer(profile);
            }
        }
    }
}