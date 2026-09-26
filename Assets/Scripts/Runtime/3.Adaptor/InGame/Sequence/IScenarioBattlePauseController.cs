namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     シナリオ再生中の戦闘ポーズを制御する契約です。
    ///     通常ポーズ中に開始されたシナリオでは、終了時に通常ポーズを解除しない実装にします。
    /// </summary>
    public interface IScenarioBattlePauseController
    {
        /// <summary>
        ///     シナリオ再生用に戦闘をポーズします。
        /// </summary>
        /// <returns>シナリオ用ポーズを開始できた場合はtrue</returns>
        bool BeginScenarioPause();

        /// <summary>
        ///     シナリオ再生用の戦闘ポーズを終了します。
        /// </summary>
        /// <returns>シナリオ用ポーズを終了できた場合はtrue</returns>
        bool EndScenarioPause();
    }
}
