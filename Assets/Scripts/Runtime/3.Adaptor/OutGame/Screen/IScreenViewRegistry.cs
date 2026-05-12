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
        ///     指定された画面を表示します。
        /// </summary>
        Task Show(ScreenId screenId, CancellationToken token);

        /// <summary>
        ///     指定された画面を非表示にします。
        /// </summary>
        Task Hide(ScreenId screenId, CancellationToken token);

        /// <summary>
        ///     すべての画面を即座に非表示にします。
        /// </summary>
        void HideAllImmediately();
    }
}
