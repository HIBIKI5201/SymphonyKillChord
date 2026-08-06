using KillChord.Runtime.Domain.OutGame.SkillTree;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     解放済みスキルノードを取得するRepositoryです。
    /// </summary>
    public interface ISkillUnlockRepository
    {
        /// <summary>
        ///     解放済みスキルノードIDを読み込みます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 解放済みスキルノードID一覧です。 </returns>
        public ValueTask<IReadOnlyCollection<SkillNodeId>> LoadUnlockedNodeIdsAsync(
            CancellationToken cancellationToken);
    }
}
