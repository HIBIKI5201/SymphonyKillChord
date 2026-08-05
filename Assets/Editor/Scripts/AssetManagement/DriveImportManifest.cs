using System;
using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    ///     差分検出用のローカルキャッシュ。端末ローカルの状態であり、UserSettings配下に保存する(Git管理対象外)。
    /// </summary>
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(DriveImportManifest), FilePathAttribute.Location.ProjectFolder)]
    internal class DriveImportManifest : ScriptableSingleton<DriveImportManifest>
    {
        /// <summary>
        ///     ファイルの同期状態を表現するエントリ。
        /// </summary>
        [Serializable]
        public class Entry
        {
            /// <summary> Google Drive のファイル ID。 </summary>
            public string fileId;
            /// <summary> ファイルの最終更新日時 (RFC 3339 形式)。差分検出に使用。 </summary>
            public string modifiedTime;
        }

        /// <summary> ファイル ID → 最終更新日時 のキャッシュマップ。 </summary>
        public List<Entry> entries = new();

        /// <summary> 在メモリキャッシュ。entries から遅延初期化される。 </summary>
        private Dictionary<string, string> _cache;

        /// <summary> エントリから構築されるメモリ内辞書。遅延初期化対応。 </summary>
        private Dictionary<string, string> Cache
        {
            get
            {
                if (_cache == null)
                {
                    _cache = new Dictionary<string, string>();
                    foreach (var e in entries)
                    {
                        if (!string.IsNullOrEmpty(e.fileId))
                        {
                            _cache[e.fileId] = e.modifiedTime;
                        }
                    }
                }
                return _cache;
            }
        }

        /// <summary>
        ///     指定ファイルID の最終更新日時を取得する。
        /// </summary>
        /// <param name="fileId"> 検索対象のファイル ID。 </param>
        /// <param name="modifiedTime"> 見つかった場合の最終更新日時。 </param>
        /// <returns> ファイルがキャッシュに存在すれば true。 </returns>
        public bool TryGetModifiedTime(string fileId, out string modifiedTime)
        {
            return Cache.TryGetValue(fileId, out modifiedTime);
        }

        /// <summary>
        ///     指定ファイルID の最終更新日時を記録する。既存エントリは更新、なければ新規追加される。
        /// </summary>
        /// <param name="fileId"> ファイル ID。 </param>
        /// <param name="modifiedTime"> ファイルの最終更新日時。 </param>
        public void SetModifiedTime(string fileId, string modifiedTime)
        {
            Cache[fileId] = modifiedTime;
            var index = entries.FindIndex(e => e.fileId == fileId);
            if (index >= 0)
            {
                entries[index].modifiedTime = modifiedTime;
            }
            else
            {
                entries.Add(new Entry { fileId = fileId, modifiedTime = modifiedTime });
            }
        }

        /// <summary>
        ///     メモリ上のキャッシュをディスクに永続化する。
        /// </summary>
        public void Persist()
        {
            entries = new List<Entry>();
            foreach (KeyValuePair<string, string> kv in Cache)
            {
                entries.Add(new Entry { fileId = kv.Key, modifiedTime = kv.Value });
            }
            Save(true);
        }
    }
}
