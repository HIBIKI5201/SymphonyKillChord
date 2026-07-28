using KillChord.Runtime.Domain.OutGame.Screen;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面表示切り替えのインターフェース。
    /// </summary>
    public interface IScreenViewRegistry
    {
        /// <summary>
        ///    指定された画面を即座に表示します。
        /// </summary>
        void ShowImmediately(ScreenId screenId, string targetSceneName = null);

        /// <summary>
        ///    指定された画面を即座に非表示にします。
        /// </summary>
        void HideImmediately(ScreenId screenId);

        /// <summary>
        ///     すべての画面を即座に非表示にします。
        /// </summary>
        void HideAllImmediately();

        /// <summary>
        ///     指定された画面を表示します。
        /// </summary>
        Task Show(ScreenId screenId, CancellationToken token, string targetSceneName = null);

        /// <summary>
        ///     指定された画面を非表示にします。
        /// </summary>
        Task Hide(ScreenId screenId, CancellationToken token);
    }
}
