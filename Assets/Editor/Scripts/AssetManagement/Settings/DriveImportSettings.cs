using System.Collections.Generic;
using KillChord.Editor.Utility;
using UnityEditor;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    ///     ファイル同期時のフィルタリング設定。
    ///     ProjectSettings に保存され Git 管理対象となる (共有情報)。
    ///     ScriptableSingleton のため単一インスタンスが保証される。
    /// </summary>
    [FilePath(ProviderConst.PROJECT_SETTINGS_PATH + nameof(DriveImportSettings), FilePathAttribute.Location.ProjectFolder)]
    internal class DriveImportSettings : ScriptableSingleton<DriveImportSettings>
    {
        /// <summary> 除外対象フォルダ名のリスト。大文字小文字を区別しない。 </summary>
        public List<string> excludeFolderNames = new();
        /// <summary> 除外対象ファイル拡張子のリスト。 </summary>
        public List<string> excludeExtensions = new();
        /// <summary> 除外対象ファイル名パターンのリスト (ワイルドカード/正規表現)。 </summary>
        public List<FilePattern> excludeFilePatterns = new();

        /// <summary>
        ///     設定をディスクに永続化する。
        /// </summary>
        public void Persist()
        {
            Save(true);
        }
    }
}
