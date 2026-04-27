using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面表示切り替えのインターフェース。
    /// </summary>
    public interface IScreenViewRegistry
    {
        /// <summary>
        ///     指定された画面を表示します。
        /// </summary>
        void Show(ScreenId screenId);

        /// <summary>
        ///     指定された画面を非表示にします。
        /// </summary>
        void Hide(ScreenId screenId);

        /// <summary>
        ///     すべての画面を即座に非表示にします。
        /// </summary>
        void HideAllImmediately();
    }
}
