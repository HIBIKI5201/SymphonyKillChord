using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス面面の表示用インタフェース。
    /// </summary>
    public interface IPlayerStatusShowable
    {
        /// <summary>
        ///     面面を表示します。
        /// </summary>
        ValueTask Show(CancellationToken cancellationToken = default);
    }
}
