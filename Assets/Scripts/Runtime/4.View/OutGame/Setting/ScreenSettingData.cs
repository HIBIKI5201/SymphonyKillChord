namespace KillChord.Runtime.View.OutGame.Setting
{
    public struct ScreenSettingData
    {
        public ScreenSettingData(int resolutionIndex, int screenModeIndex, bool isVSync)
        {
            ResolutionIndex = resolutionIndex;
            ScreenModeIndex = screenModeIndex;
            IsVSync = isVSync;
        }

        public int ResolutionIndex { get; set; }
        public int ScreenModeIndex { get; set; }
        public bool IsVSync { get; set; }
    }
}
