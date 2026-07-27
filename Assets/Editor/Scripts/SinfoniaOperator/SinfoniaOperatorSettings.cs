using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     SinfoniaOperator（Notionタスク表・Discord通知）のUnity側設定。
    ///     Notion/Discordの設定はBot本体(Exe)と共有するJSON設定ファイルから読み込むため、
    ///     ここでは公開設定・秘密設定へのパスと、Unity固有の項目のみを保持する。
    /// </summary>
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(SinfoniaOperatorSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    public class SinfoniaOperatorSettings : ScriptableSingleton<SinfoniaOperatorSettings>
    {
        [FormerlySerializedAs("ConfigJsonPath")]
        [Tooltip("Bot本体(Exe)と共有する公開JSON設定ファイルへのパスです。" +
            "プロジェクトルートからの相対パス、または絶対パスで指定します。")]
        public string EnvironmentConfigJsonPath = DEFAULT_ENVIRONMENT_CONFIG_JSON_PATH;

        [Tooltip("NotionとDiscordのトークンを保持する秘密JSON設定ファイルへのパスです。" +
            "プロジェクトルートからの相対パス、または絶対パスで指定します。")]
        public string SecretsConfigJsonPath = DEFAULT_SECRETS_CONFIG_JSON_PATH;

        [Tooltip("作業ログに表示する名前です。空の場合はOSのユーザー名を使用します。")]
        public string WorkLogUserName;

        /// <summary> 公開設定ファイルの既定パス（プロジェクトルートからの相対パス）。 </summary>
        public const string DEFAULT_ENVIRONMENT_CONFIG_JSON_PATH = "SinfoniaOperator/sinfonia-operator.env.json";

        /// <summary> 秘密設定ファイルの既定パス（プロジェクトルートからの相対パス）。 </summary>
        public const string DEFAULT_SECRETS_CONFIG_JSON_PATH = "SinfoniaOperator/sinfonia-operator.secrets.json";

        /// <summary> 分割前の設定ファイルのパス。 </summary>
        public const string LEGACY_CONFIG_JSON_PATH = "SinfoniaOperator/sinfonia-operator.settings.json";

        /// <summary>
        ///     設定をファイルへ保存する。
        /// </summary>
        public void Save()
        {
            Save(false);
        }
    }
}
