namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     画面のインターフェース。
    /// </summary>
    public interface IScreenView
    {
        /// <summary>
        ///    画面を表示状態にします。実際の見た目の変化は USS のトランジションに従います。
        /// </summary>
        void Show();

        /// <summary>
        ///     画面を非表示状態にします。実際の見た目の変化は USS のトランジションに従います。
        /// </summary>
        void Hide();
    }
}
