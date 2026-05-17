using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果の適用先のインターフェース。
    /// </summary>
    public interface IScreenTransitionApplicable
    {
        /// <summary>
        ///     画面遷移結果を適用します。
        /// </summary>
        Task Apply(in ScreenViewDTO screenViewDTO, CancellationToken token);
    }
}
