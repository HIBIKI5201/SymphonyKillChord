using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面の表示操作を抽象化するインターフェース。
    /// </summary>
    public interface IStageDetailScreenShowable
    {
        /// <summary>
        ///     詳細画面を表示します。
        /// </summary>
        ValueTask Show(CancellationToken cancellationToken = default);
    }
}
