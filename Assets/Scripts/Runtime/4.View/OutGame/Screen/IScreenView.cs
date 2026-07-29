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
        ///    画面を表示状態にします。opacity のフェードは LitMotion で再生します。
        ///    フェード完了(または cancellationToken のキャンセル)まで待機できます。
        /// </summary>
        ValueTask Show(CancellationToken cancellationToken = default);

        /// <summary>
        ///     画面を非表示状態にします。opacity のフェードは LitMotion で再生します。
        ///     フェード完了(または cancellationToken のキャンセル)まで待機できます。
        /// </summary>
        ValueTask Hide(CancellationToken cancellationToken = default);
    }
}
