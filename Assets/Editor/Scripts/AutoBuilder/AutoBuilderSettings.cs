using KillChord.Editor.SourceDataProvider;
using KillChord.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine.Serialization;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     オートビルダーの設定を保持するクラス。
    ///     ゲームデータ種別（Release / Demo）とビルドモード（Master / Develop）の組み合わせごとに枠を持ちます。
    /// </summary>
    [FilePath(
        ProviderConst.PROJECT_SETTINGS_PATH + nameof(AutoBuilderSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    public class AutoBuilderSettings : ScriptableSingleton<AutoBuilderSettings>
    {
        [FormerlySerializedAs("MasterPath")]
        public string ReleaseMasterPath;

        [FormerlySerializedAs("MasterBuildProfiles")]
        public BuildProfile[] ReleaseMasterBuildProfiles;

        [FormerlySerializedAs("DevelopPath")]
        public string ReleaseDevelopPath;

        [FormerlySerializedAs("DevelopBuildProfiles")]
        public BuildProfile[] ReleaseDevelopBuildProfiles;

        public string DemoMasterPath;
        public BuildProfile[] DemoMasterBuildProfiles;

        public string DemoDevelopPath;
        public BuildProfile[] DemoDevelopBuildProfiles;

        /// <summary> 画面表示とCLIで扱う自動ビルドの枠一覧です。 </summary>
        internal static readonly AutoBuildSlot[] SLOTS =
        {
            new(GameDataVariant.Release, AutoBuildMode.Master, nameof(ReleaseMasterPath), nameof(ReleaseMasterBuildProfiles)),
            new(GameDataVariant.Release, AutoBuildMode.Development, nameof(ReleaseDevelopPath), nameof(ReleaseDevelopBuildProfiles)),
            new(GameDataVariant.Demo, AutoBuildMode.Master, nameof(DemoMasterPath), nameof(DemoMasterBuildProfiles)),
            new(GameDataVariant.Demo, AutoBuildMode.Development, nameof(DemoDevelopPath), nameof(DemoDevelopBuildProfiles)),
        };

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

        /// <summary>
        ///     種別とモードに対応する枠を取得します。
        /// </summary>
        /// <param name="variant"> ゲームデータ種別です。 </param>
        /// <param name="mode"> ビルドモードです。 </param>
        /// <param name="slot"> 見つかった枠です。 </param>
        /// <returns> 枠が見つかった場合はtrueです。 </returns>
        internal static bool TryGetSlot(GameDataVariant variant, AutoBuildMode mode, out AutoBuildSlot slot)
        {
            foreach (AutoBuildSlot candidate in SLOTS)
            {
                if (candidate.Variant == variant && candidate.Mode == mode)
                {
                    slot = candidate;
                    return true;
                }
            }

            slot = default;
            return false;
        }

        /// <summary>
        ///     枠の出力先パスを取得します。
        /// </summary>
        /// <param name="slot"> 対象の枠です。 </param>
        /// <returns> 出力先パスです。 </returns>
        internal string GetPath(AutoBuildSlot slot)
        {
            return (slot.Variant, slot.Mode) switch
            {
                (GameDataVariant.Release, AutoBuildMode.Master) => ReleaseMasterPath,
                (GameDataVariant.Release, AutoBuildMode.Development) => ReleaseDevelopPath,
                (GameDataVariant.Demo, AutoBuildMode.Master) => DemoMasterPath,
                (GameDataVariant.Demo, AutoBuildMode.Development) => DemoDevelopPath,
                _ => null,
            };
        }

        /// <summary>
        ///     枠に登録されたBuild Profile一覧を取得します。
        /// </summary>
        /// <param name="slot"> 対象の枠です。 </param>
        /// <returns> Build Profile一覧です。 </returns>
        internal BuildProfile[] GetProfiles(AutoBuildSlot slot)
        {
            return (slot.Variant, slot.Mode) switch
            {
                (GameDataVariant.Release, AutoBuildMode.Master) => ReleaseMasterBuildProfiles,
                (GameDataVariant.Release, AutoBuildMode.Development) => ReleaseDevelopBuildProfiles,
                (GameDataVariant.Demo, AutoBuildMode.Master) => DemoMasterBuildProfiles,
                (GameDataVariant.Demo, AutoBuildMode.Development) => DemoDevelopBuildProfiles,
                _ => null,
            };
        }
    }
}
