using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace KillChord.Editor.ProjectWindow
{
    /// <summary>
    ///     Addressables登録アセットやビルド対象シーンから参照経由でビルドに含まれる、
    ///     Addressables未登録のゲームデータ(ScriptableObject)をGUID単位で保持します。
    ///     依存関係の走査は重いため、結果をLibraryフォルダへキャッシュし、変更後にまとめて再計算します。
    /// </summary>
    [InitializeOnLoad]
    internal static class BuildDependencyAssetIndex
    {
        /// <summary>
        ///     ドメインロード時にLibraryのキャッシュを読み込み、必要なら再計算を予約します。
        /// </summary>
        static BuildDependencyAssetIndex()
        {
            if (!TryLoadCache())
            {
                ScheduleRebuild();
            }

            EditorApplication.update += UpdateHandler;
        }

        /// <summary> 参照経由でビルドに含まれるAddressables未登録アセットのGUID一覧です。 </summary>
        internal static IReadOnlyCollection<string> Guids => _guids;

        /// <summary>
        ///     変更されたアセットパスに依存関係へ影響し得るものがあれば再計算を予約します。
        /// </summary>
        /// <param name="assetPaths"> 変更されたアセットパスです。 </param>
        internal static void ScheduleRebuildIfRelevant(string[] assetPaths)
        {
            for (int i = 0; i < assetPaths.Length; i++)
            {
                if (IsDependencySourcePath(assetPaths[i]))
                {
                    ScheduleRebuild();
                    return;
                }
            }
        }

        /// <summary>
        ///     再計算を予約します。連続した変更は最後の変更から一定時間後に1回だけ処理します。
        /// </summary>
        internal static void ScheduleRebuild()
        {
            _isRebuildScheduled = true;
            _rebuildRequestedTime = EditorApplication.timeSinceStartup;
        }

        /// <summary>
        ///     依存関係を即座に再計算し、キャッシュを更新します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Project Window/Rebuild Build Dependency Index")]
        internal static void RebuildNow()
        {
            _isRebuildScheduled = false;
            _guids = CollectBuildDependencyGuids();
            SaveCache();
            SourceDataAssetIndex.Invalidate();
        }

        private const string CACHE_DIRECTORY = "Library/KillChord";
        private const string CACHE_FILE_PATH = CACHE_DIRECTORY + "/BuildDependencyAssetIndex.json";
        private const int CACHE_VERSION = 1;
        private const double REBUILD_DEBOUNCE_SECONDS = 1.5d;
        private const string GAME_DATA_ASSEMBLY_PREFIX = "KillChord";

        private static readonly string[] _dependencySourceExtensions = { ".asset", ".prefab", ".unity" };

        private static HashSet<string> _guids = new();
        private static bool _isRebuildScheduled;
        private static double _rebuildRequestedTime;

        /// <summary>
        ///     予約済みの再計算を、変更が落ち着きエディタが待機状態のときに実行します。
        /// </summary>
        private static void UpdateHandler()
        {
            if (!_isRebuildScheduled
                || EditorApplication.timeSinceStartup - _rebuildRequestedTime < REBUILD_DEBOUNCE_SECONDS
                || EditorApplication.isCompiling
                || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            RebuildNow();
        }

        /// <summary>
        ///     依存関係の起点となり得るアセットパスかを返します。
        /// </summary>
        /// <param name="assetPath"> 確認対象のアセットパスです。 </param>
        /// <returns> 起点となり得る場合はtrueです。 </returns>
        private static bool IsDependencySourcePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            for (int i = 0; i < _dependencySourceExtensions.Length; i++)
            {
                if (assetPath.EndsWith(_dependencySourceExtensions[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Addressables登録アセットと有効なビルド対象シーンを起点に、
        ///     参照経由でビルドに含まれるAddressables未登録のゲームデータGUIDを収集します。
        /// </summary>
        /// <returns> 収集したGUIDの集合です。 </returns>
        private static HashSet<string> CollectBuildDependencyGuids()
        {
            HashSet<string> addressableGuids = new();
            List<string> rootPaths = new();
            CollectAddressableRoots(addressableGuids, rootPaths);

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && !string.IsNullOrEmpty(scenes[i].path))
                {
                    rootPaths.Add(scenes[i].path);
                }
            }

            HashSet<string> result = new();
            string[] dependencies = AssetDatabase.GetDependencies(rootPaths.ToArray(), true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string path = dependencies[i];
                if (!IsGameDataAsset(path))
                {
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid) && !addressableGuids.Contains(guid))
                {
                    result.Add(guid);
                }
            }

            return result;
        }

        /// <summary>
        ///     全AddressablesGroupの登録アセット(フォルダ登録の中身を含む)を起点として収集します。
        /// </summary>
        /// <param name="addressableGuids"> Addressables登録済みGUIDの格納先です。 </param>
        /// <param name="rootPaths"> 依存関係走査の起点パスの格納先です。 </param>
        private static void CollectAddressableRoots(HashSet<string> addressableGuids, List<string> rootPaths)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings?.groups == null)
            {
                return;
            }

            List<AddressableAssetEntry> entries = new();
            for (int i = 0; i < settings.groups.Count; i++)
            {
                AddressableAssetGroup group = settings.groups[i];
                if (group == null)
                {
                    continue;
                }

                entries.Clear();
                group.GatherAllAssets(entries, true, true, true);
                for (int j = 0; j < entries.Count; j++)
                {
                    AddressableAssetEntry entry = entries[j];
                    if (entry == null || string.IsNullOrEmpty(entry.guid))
                    {
                        continue;
                    }

                    addressableGuids.Add(entry.guid);
                    if (!string.IsNullOrEmpty(entry.AssetPath))
                    {
                        rootPaths.Add(entry.AssetPath);
                    }
                }
            }
        }

        /// <summary>
        ///     ゲーム側アセンブリで定義されたScriptableObjectのアセットかを返します。
        /// </summary>
        /// <param name="assetPath"> 確認対象のアセットパスです。 </param>
        /// <returns> ゲームデータの場合はtrueです。 </returns>
        private static bool IsGameDataAsset(string assetPath)
        {
            if (!assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            return mainAssetType != null
                && typeof(ScriptableObject).IsAssignableFrom(mainAssetType)
                && mainAssetType.Assembly.GetName().Name.StartsWith(GAME_DATA_ASSEMBLY_PREFIX, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Libraryフォルダのキャッシュを読み込みます。
        /// </summary>
        /// <returns> 有効なキャッシュを読み込めた場合はtrueです。 </returns>
        private static bool TryLoadCache()
        {
            try
            {
                if (!File.Exists(CACHE_FILE_PATH))
                {
                    return false;
                }

                CacheData data = JsonUtility.FromJson<CacheData>(File.ReadAllText(CACHE_FILE_PATH));
                if (data == null || data.Version != CACHE_VERSION || data.Guids == null)
                {
                    return false;
                }

                _guids = new HashSet<string>(data.Guids);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(BuildDependencyAssetIndex)}] キャッシュを読み込めませんでした。再計算します。{exception.Message}");
                return false;
            }
        }

        /// <summary>
        ///     現在のインデックスをLibraryフォルダへ保存します。
        /// </summary>
        private static void SaveCache()
        {
            try
            {
                Directory.CreateDirectory(CACHE_DIRECTORY);
                CacheData data = new() { Version = CACHE_VERSION, Guids = new List<string>(_guids) };
                data.Guids.Sort(StringComparer.Ordinal);
                File.WriteAllText(CACHE_FILE_PATH, JsonUtility.ToJson(data));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(BuildDependencyAssetIndex)}] キャッシュを保存できませんでした。{exception.Message}");
            }
        }

        /// <summary>
        ///     Libraryフォルダへ保存するキャッシュの形式です。
        /// </summary>
        [Serializable]
        private sealed class CacheData
        {
            /// <summary> キャッシュ形式のバージョンです。 </summary>
            public int Version;

            /// <summary> 参照経由でビルドに含まれるアセットのGUID一覧です。 </summary>
            public List<string> Guids;
        }
    }
}
