namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     戦闘ポーズを行うインターフェース。
    /// </summary>
    public interface IBattlePausable
    {
        /// <summary>
        ///     戦闘ポーズを行う。
        /// </summary>
        void PauseBattle();

        /// <summary>
        ///     戦闘ポーズを解除する。
        /// </summary>
        void ResumeBattle();
    }
}