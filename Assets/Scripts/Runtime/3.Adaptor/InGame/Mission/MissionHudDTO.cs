namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ミッションHUDに表示する情報をまとめたDTOクラス。
    /// </summary>
    public readonly ref struct MissionHudDTO
    {
        public MissionHudDTO(string mainMissionText, string resultText)
        {
            MainMissionText = mainMissionText;
            ResultText = resultText;
        }

        public string MainMissionText { get; }
        public string ResultText { get; }
    }
}
