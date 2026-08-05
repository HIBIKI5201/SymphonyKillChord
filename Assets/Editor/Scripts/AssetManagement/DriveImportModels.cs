using System;

namespace KillChord.Editor.AssetManagement
{
    /// <summary> ファイル除外パターンの種類。 </summary>
    public enum FilePatternType
    {
        /// <summary> ワイルドカード方式 (*, ? による指定)。 </summary>
        Wildcard,
        /// <summary> 正規表現方式。 </summary>
        Regex
    }

    /// <summary>
    /// ファイル除外パターンの定義。
    /// </summary>
    [Serializable]
    public class FilePattern
    {
        /// <summary> 除外パターン文字列。 </summary>
        public string pattern = "";
        /// <summary> パターンの種類 (ワイルドカードまたは正規表現)。 </summary>
        public FilePatternType type = FilePatternType.Wildcard;
    }

    /// <summary>
    /// 取得元フォルダIDと配置先パスの組。フォルダIDは外部から参照/推測される危険性があるため機密扱い。
    /// DriveImportSecrets側で管理する。
    /// </summary>
    [Serializable]
    public class DriveSourceFolder
    {
        public string folderId = "";
        public string destinationPath = "Assets/Imported";
    }
}
