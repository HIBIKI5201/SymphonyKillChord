namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     会話表示用の情報を保持するViewModel。
    /// </summary>
    public interface IMissionDialogueViewModel
    {
        /// <summary>
        ///     会話情報を反映する。
        /// </summary>
        void Apply(in MissionDialogueDTO dto);
    }
}
