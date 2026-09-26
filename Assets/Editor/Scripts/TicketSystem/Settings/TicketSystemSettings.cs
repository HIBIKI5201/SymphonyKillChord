using KillChord.Editor.Utility;
using UnityEditor;

namespace KillChord.Editor.TicketSystem
{
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(TicketSystemSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    /// <summary>
    ///     チケットシステムの接続先と利用者名を保持するプロジェクト設定。
    /// </summary>
    public class TicketSystemSettings : ScriptableSingleton<TicketSystemSettings>
    {
        public string GasUrl;
        public string ApiKey;
        public string UserName;

        /// <summary>
        ///     設定をファイルへ保存する。
        /// </summary>
        public void Save()
        {
            Save(false);
        }
    }
}