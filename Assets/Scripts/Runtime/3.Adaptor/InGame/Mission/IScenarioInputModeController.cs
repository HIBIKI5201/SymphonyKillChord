namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     シナリオ再生中の入力マップを切り替えるインタフェース。
    /// </summary>
    public interface IScenarioInputModeController
    {
        /// <summary>
        ///     シナリオ操作用の入力マップへ切り替えます。
        /// </summary>
        void EnterScenarioInputMode();

        /// <summary>
        ///     InGame操作用の入力マップへ戻します。
        /// </summary>
        void ExitScenarioInputMode();
    }
}
