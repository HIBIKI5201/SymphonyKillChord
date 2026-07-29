using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細面面の表示用インタフェース。
    /// </summary>
    public interface ISkillDetailShowable
    {
        /// <summary>
        ///     面面を表示します。
        /// </summary>
        ValueTask Show(CancellationToken cancellationToken = default);
    }
}
