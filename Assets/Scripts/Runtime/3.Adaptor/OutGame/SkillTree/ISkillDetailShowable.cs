using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細画面の表示用インタフェース。
    /// </summary>
    public interface ISkillDetailShowable
    {
        Task Show(CancellationToken token);
    }
}
