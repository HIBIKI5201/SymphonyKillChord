using System;

namespace Research.SaveSystem
{
    /// <summary>
    ///     セーブデータ：システム情報
    /// </summary>
    [Serializable]
    public class SystemData
    {
        public SystemData()
        {
            Volume = 80f;
        }

        /// <summary>音量</summary>
        public float Volume { get; set; }
    }
}