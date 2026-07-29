using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルプレイビュー画面の表示用インタフェース。
    /// </summary>
    public interface IPreviewVideoScreenViewShowable
    {
        /// <summary>
        ///     画面を表示します。
        /// </summary>
        ValueTask Show(CancellationToken cancellationToken = default);
    }
}
