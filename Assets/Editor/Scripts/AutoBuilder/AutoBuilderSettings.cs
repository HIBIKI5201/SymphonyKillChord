using KillChord.Editor.Utility;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     オートビルダーの設定を保持するクラス。
    /// </summary>
    [FilePath(
        ProviderConst.PROJECT_PATH + nameof(AutoBuilderSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    public class AutoBuilderSettings : ScriptableSingleton<AutoBuilderSettings>
    {
        public string MasterPath;
        public BuildProfile[] MasterBuildProfiles;

        public string DevelopPath;
        public BuildProfile[] DevelopBuildProfiles;

        public static void Save() => instance.Save(true);
    }
}