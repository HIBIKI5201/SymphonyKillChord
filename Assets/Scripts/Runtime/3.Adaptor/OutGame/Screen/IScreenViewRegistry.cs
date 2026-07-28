using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面表示切り替えのインターフェース。
    /// </summary>
    public interface IScreenViewRegistry
    {
        /// <summary>
        ///    指定された画面を表示状態にします。
        /// </summary>
        void Show(ScreenId screenId, string targetSceneName = null);

        /// <summary>
        ///    指定された画面を非表示状態にします。
        /// </summary>
        void Hide(ScreenId screenId);

        /// <summary>
        ///     すべての画面を非表示状態にします。
        /// </summary>
        void HideAll();
    }
}
