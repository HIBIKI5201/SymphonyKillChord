using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.TicketSystem
{
    [FilePath(ProviderConst.USER_SETTINGS_PATH + nameof(TicketSystemSettings) + ProviderConst.ASSET_EXT,
        FilePathAttribute.Location.ProjectFolder)]
    /// <summary>
    ///     チケットシステムの接続先と利用者名を保持するプロジェクト設定。
    ///     API キーは共通秘密設定ファイル（<see cref="SharedSecrets"/>）から読む。
    /// </summary>
    public class TicketSystemSettings : ScriptableSingleton<TicketSystemSettings>
    {
        public string GasUrl;

        /// <summary>
        ///     移行前の API キー。現在は共通秘密設定ファイルに置く。
        ///     移行期間中だけ読み、移行したら空にする。
        /// </summary>
        [HideInInspector]
        public string ApiKey;

        public string UserName;

        /// <summary>
        ///     API キーを取得する。共通秘密設定ファイルを優先し、無い場合は移行前の値を使う。
        /// </summary>
        /// <returns> API キー。未設定の場合は空文字。 </returns>
        public string GetApiKey()
        {
            return SharedSecrets.ResolveValue(
                SharedSecrets.TICKET_SYSTEM_API_KEY,
                ApiKey,
                "Project Settings > TicketSystem");
        }

        /// <summary>
        ///     移行前の API キーを共通秘密設定ファイルへ移し、移せたらこの設定から消す。
        /// </summary>
        /// <returns> 移行前の値が残っていない場合は true。 </returns>
        public bool MigrateToSharedSecrets()
        {
            if (!SharedSecrets.TryMigrate(SharedSecrets.TICKET_SYSTEM_API_KEY, ApiKey)) { return false; }

            if (!string.IsNullOrEmpty(ApiKey))
            {
                ApiKey = string.Empty;
                Save();
            }

            return true;
        }

        /// <summary>
        ///     設定をファイルへ保存する。
        /// </summary>
        public void Save()
        {
            Save(false);
        }
    }
}
