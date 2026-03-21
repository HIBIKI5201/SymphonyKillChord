using System;
namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータクラス
    /// </summary>
    [Serializable]
    public class KillChordGameData
    {
        public KillChordGameData()
        {
            VersionNo = Constants.CURRENT_VERSION;
            SystemData = new();
            PlayerData = new();
            OutGameData = new();
        }
        /// <summary>システム情報</summary>
        public string VersionNo { get; set; }
        /// <summary>システム情報</summary>
        public SystemData SystemData { get; set; }
        /// <summary>プレイヤー情報</summary>
        public PlayerData PlayerData { get; set; }
        /// <summary>アウトゲーム情報</summary>
        public OutGameData OutGameData { get; set; }
    }
}