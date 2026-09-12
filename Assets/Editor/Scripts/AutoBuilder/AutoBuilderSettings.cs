using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEditor.Build.Profile;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     オートビルダーの設定を保持するクラス。
    /// </summary>
    [FilePath(
        ProviderConst.PROJECT_SETTINGS_PATH + nameof(AutoBuilderSettings) + ProviderConst.ASSET_EXT,
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

        public static bool IsBuildProfilesValid(BuildProfile[] profiles)
        {
            return !IsBuildProfilesNullOrEmpty(profiles) && !HasEmptyBuildProfile(profiles) && !HasDuplicateBuildProfiles(profiles);
        }
        
        public static bool IsBuildProfilesNullOrEmpty(BuildProfile[] profiles)
        {
            return profiles == null || profiles.Length == 0;
        }

        public static bool HasEmptyBuildProfile(BuildProfile[] profiles)
        {
            foreach (BuildProfile profile in profiles)
            {
                if (profile == null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasDuplicateBuildProfiles(BuildProfile[] profiles)
        {
            HashSet<BuildProfile> uniqueProfiles = new();

            foreach (BuildProfile profile in profiles)
            {
                if (profile == null) { continue; }
                if (!uniqueProfiles.Add(profile))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Save() => instance.Save(true);
        }
    }