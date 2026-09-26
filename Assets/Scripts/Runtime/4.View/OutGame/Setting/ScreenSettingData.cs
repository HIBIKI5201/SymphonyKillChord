namespace KillChord.Runtime.View.OutGame.Setting
{
    /// <summary>
    ///     画面設定の値。
    /// </summary>
    public struct ScreenSettingData
    {
        /// <summary>
        ///     解像度・画面モード・垂直同期を指定して生成する。
        /// </summary>
        public ScreenSettingData(int resolutionIndex, int screenModeIndex, bool isVSync)
        {
            ResolutionIndex = resolutionIndex;
            ScreenModeIndex = screenModeIndex;
            IsVSync = isVSync;
        }

        /// <summary> 解像度の選択肢のインデックス。 </summary>
        public int ResolutionIndex { get; set; }
        /// <summary> 画面モードの選択肢のインデックス。 </summary>
        public int ScreenModeIndex { get; set; }
        /// <summary> 垂直同期を有効にするか。 </summary>
        public bool IsVSync { get; set; }
    }
}
