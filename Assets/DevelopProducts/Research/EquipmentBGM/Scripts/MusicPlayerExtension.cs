
using KillChord.Runtime.View.Persistent.Music;
namespace DevelopProducts.EquipmentBGM
{
    /// <summary>
    ///     MusicPlayerの拡張クラス。
    /// </summary>
    public static class MusicPlayerExtension
    {
        /// <summary>
        ///     対応するBGMを流す。
        ///     スキル発動時にBGMを切り替える時に使用する想定。
        /// </summary>
        /// <param name="musicPlayer"> 対象となる音楽プレイヤー。 </param>
        /// <param name="selectorName"> CRIのセレクター名。 </param>
        /// <param name="labelName"> 設定するセレクターラベル名。 </param>
        public static void SetSelectorLabel(this MusicPlayer musicPlayer,
            string selectorName, string labelName)
        {
            // MusicPlayer.AtomSource / PlayBack（動作テスト用の未コミットプロパティ）に依存しており、
            // 有効なままだと全員のプロジェクトがコンパイルエラーになるため現在は無効化している。
            // 開発ドキュメント参照: https://app.notion.com/p/38a7e7b728bf803fa9f1ea76057c87a4
            //musicPlayer.AtomSource.player.SetSelectorLabel(selectorName, labelName);
            //musicPlayer.AtomSource.player.Update(musicPlayer.PlayBack);
        }
    }
}