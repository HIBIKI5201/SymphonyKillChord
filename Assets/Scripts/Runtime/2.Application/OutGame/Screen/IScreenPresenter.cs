using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果の出力先インターフェース。
    /// </summary>
    public interface IScreenPresenter
    {
        /// <summary>
        ///     画面遷移結果を出力します。
        /// </summary>
        Task Present(ScreenTransitionResult result, CancellationToken token);
    }
}
