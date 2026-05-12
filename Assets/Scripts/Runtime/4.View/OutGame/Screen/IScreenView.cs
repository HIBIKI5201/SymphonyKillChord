using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     画面のインターフェース。
    /// </summary>
    public interface IScreenView
    {
        /// <summary>
        ///     画面を表示します。
        /// </summary>
        Task Show(CancellationToken token);

        /// <summary>
        ///     画面を非表示にします。
        /// </summary>
        Task Hide(CancellationToken token);

        /// <summary>
        ///     画面を即座に非表示にします。
        /// </summary>
        void HideImmediately();
    }
}
