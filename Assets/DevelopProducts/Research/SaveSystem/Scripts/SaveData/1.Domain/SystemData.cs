using System;

namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータ：システム情報
    /// </summary>
    [Serializable]
    public class SystemData : SaveDataBase
    {
        public SystemData()
        {
            MasterVolume = Constants.CONFIG_MASTER_VOLUME_DEFAULT;
            BgmVolume = Constants.CONFIG_BGM_VOLUME_DEFAULT;
            SeVolume = Constants.CONFIG_SE_VOLUME_DEFAULT;
            Version = Constants.CURRENT_VERSION;
        }

        /// <summary>全体音量</summary>
        public float MasterVolume { get; set; }
        /// <summary>BGM音量</summary>
        public float BgmVolume { get; set; }
        /// <summary>SE音量</summary>
        public float SeVolume { get; set; }
    }
}