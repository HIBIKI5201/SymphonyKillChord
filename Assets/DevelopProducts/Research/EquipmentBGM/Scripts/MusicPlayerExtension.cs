using KillChord.Runtime.View.Persistent.Music;
using static CriWare.CriProfiler;
namespace DevelopProducts.EquipmentBGM
{
    /// <summary>
    ///    　MusicPlayerの拡張クラス
    /// </summary>
    public static class MusicPlayerExtension
    {
        /// <summary>
        ///     対応するBGMを流す
        ///     スキル発動時にのBGMを切り替える時に使用する想定
        /// </summary>
        /// <param name="musicPlayer"></param>
        /// <param name="selectorName"></param>
        /// <param name="labelName"></param>
        public static void SetSelectorLabel(this MusicPlayer musicPlayer,
            string selectorName, string labelName)
        {
            musicPlayer.AtomSource.player.SetSelectorLabel(selectorName, labelName);
            musicPlayer.AtomSource.player.Update(musicPlayer.PlayBack);
        }
    }
}