using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     SinfoniaOperator（Notionタスク表・Discord通知）のUnity側設定。
    ///     Notion/DiscordのトークンはBot本体(Exe)と共有するJSON設定ファイルから読み込むため、
    ///     ここではそのファイルへのパスと、Unity固有の項目のみを保持する。
    /// </summary>
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(SinfoniaOperatorSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    public class SinfoniaOperatorSettings : ScriptableSingleton<SinfoniaOperatorSettings>
    {
        [Tooltip("Bot本体(Exe)と共有するJSON設定ファイルへのパスです。" +
            "プロジェクトルートからの相対パス、または絶対パスで指定します。")]
        public string ConfigJsonPath = DEFAULT_CONFIG_JSON_PATH;

        [Tooltip("作業ログに表示する名前です。空の場合はOSのユーザー名を使用します。")]
        public string WorkLogUserName;

        /// <summary> JSON設定ファイルの既定パス（プロジェクトルートからの相対パス）。 </summary>
        public const string DEFAULT_CONFIG_JSON_PATH = "SinfoniaOperator/sinfonia-operator.settings.json";

        /// <summary>
        ///     設定をファイルへ保存する。
        /// </summary>
        public void Save()
        {
            Save(false);
        }
    }
}
