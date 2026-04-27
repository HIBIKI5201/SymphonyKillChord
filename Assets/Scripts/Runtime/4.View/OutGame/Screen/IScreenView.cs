namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     画面 のインターフェース。
    /// </summary>
    public interface IScreenView
    {
        /// <summary>
        ///     画面を表示します。
        /// </summary>
        void Show();

        /// <summary>
        ///     画面を非表示にします。
        /// </summary>
        void Hide();

        /// <summary>
        ///     画面を即座に非表示にします。
        /// </summary>
        void HideImmediately();
    }
}
