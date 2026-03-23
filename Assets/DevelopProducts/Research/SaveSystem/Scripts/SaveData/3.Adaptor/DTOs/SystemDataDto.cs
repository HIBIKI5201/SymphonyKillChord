namespace Research.SaveSystem
{
    /// <summary>
    ///     システム情報のDTO。
    /// </summary>
    public class SystemDataDto
    {
        /// <summary>全体音量</summary>
        public float MasterVolume { get; set; } = 0.7f;
        /// <summary>BGM音量</summary>
        public float BgmVolume { get; set; } = 0.7f;
        /// <summary>SE音量</summary>
        public float SeVolume { get; set; } = 0.7f;
    }
}