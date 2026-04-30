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

        public static bool IsPathValid(string path)
        {
            return !IsPathNullOrEmpty(path) && IsPathEndsWithSlash(path);
        }

        public static bool IsPathNullOrEmpty(string path)
        {
            return string.IsNullOrEmpty(path);
        }

        public static bool IsPathEndsWithSlash(string path)
        {
            if (path.Length < 1) { return false; }
            return path[^1] == '/' || path[^1] == '\\';
        }

        public static void Save() => instance.Save(true);
    }
}