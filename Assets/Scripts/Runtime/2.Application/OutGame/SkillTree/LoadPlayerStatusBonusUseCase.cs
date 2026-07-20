using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     解放済みスキルノードからプレイヤーステータスボーナスを読み込むユースケースです。
    /// </summary>
    public sealed class LoadPlayerStatusBonusUseCase
    {
        /// <summary>
        ///     ユースケースを生成します。
        /// </summary>
        /// <param name="skillNodeRepository"> スキルノード定義Repositoryです。 </param>
        /// <param name="skillUnlockRepository"> スキルノード解放状態Repositoryです。 </param>
        public LoadPlayerStatusBonusUseCase(
            ISkillNodeRepository skillNodeRepository,
            ISkillUnlockRepository skillUnlockRepository)
        {
            _skillNodeRepository = skillNodeRepository
                ?? throw new ArgumentNullException(nameof(skillNodeRepository));
            _skillUnlockRepository = skillUnlockRepository
                ?? throw new ArgumentNullException(nameof(skillUnlockRepository));
        }

        /// <summary>
        ///     プレイヤーステータスボーナスを読み込みます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 集計済みのプレイヤーステータスボーナスです。 </returns>
        public async ValueTask<PlayerStatusBonus> ExecuteAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<SkillNodeId> unlockedNodeIds =
                await _skillUnlockRepository.LoadUnlockedNodeIdsAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            PlayerStatusBonusCalculator calculator =
                new PlayerStatusBonusCalculator(_skillNodeRepository.GetAll());
            return calculator.Calculate(unlockedNodeIds);
        }

        private readonly ISkillNodeRepository _skillNodeRepository;
        private readonly ISkillUnlockRepository _skillUnlockRepository;
    }
}
