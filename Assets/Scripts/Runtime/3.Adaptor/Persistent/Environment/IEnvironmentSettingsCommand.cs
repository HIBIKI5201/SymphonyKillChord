namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     UIから環境設定の変更を受け取るコマンドインターフェース。
    /// </summary>
    public interface IEnvironmentSettingsCommand
    {
        /// <summary>
        ///     解像度を前後に切り替える。
        /// </summary>
        /// <param name="direction"> 1なら次、-1なら前へ切り替える。 </param>
        void CycleResolution(int direction);

        /// <summary>
        ///     フルスクリーンとウィンドウを切り替える。
        /// </summary>
        void ToggleScreenMode();

        /// <summary>
        ///     画質プリセットを前後に切り替える。
        /// </summary>
        /// <param name="direction"> 1なら次、-1なら前へ切り替える。 </param>
        void CycleQualityLevel(int direction);

        /// <summary>
        ///     画面の明るさを設定する。
        /// </summary>
        /// <param name="brightness"> 設定する明るさ。 </param>
        void SetBrightness(int brightness);

        /// <summary>
        ///     すべての環境設定を既定値へ戻す。
        /// </summary>
        void ResetToDefaults();

        /// <summary>
        ///     プレビュー中の変更を保存として確定する。
        /// </summary>
        void ConfirmChanges();

        /// <summary>
        ///     プレビュー中の変更を破棄し、直前に保存された状態へ戻す。
        /// </summary>
        void CancelChanges();
    }
}
