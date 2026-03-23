namespace Research.SaveSystem
{
    /// <summary>
    ///     システム情報DTO
    /// </summary>
    public class SystemDataDto
    {
        public SystemDataDto()
        {
        }
        /// <summary>全体音量</summary>
        public float MasterVolume { get; set; }
        /// <summary>BGM音量</summary>
        public float BgmVolume { get; set; }
        /// <summary>SE音量</summary>
        public float SeVolume { get; set; }
    }
}