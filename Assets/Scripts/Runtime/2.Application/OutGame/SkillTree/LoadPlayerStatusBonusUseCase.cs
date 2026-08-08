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
        /// <param name="skillUnlockRepository"> スキルノード解放状態Repositoryです。 </param>
        /// <param name="calculator"> プレイヤーステータスボーナス計算クラスです。 </param>
        public LoadPlayerStatusBonusUseCase(
            ISkillUnlockRepository skillUnlockRepository,
            PlayerStatusBonusCalculator calculator)
        {
            _skillUnlockRepository = skillUnlockRepository
                ?? throw new ArgumentNullException(nameof(skillUnlockRepository));
            _calculator = calculator
                ?? throw new ArgumentNullException(nameof(calculator));
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

            return _calculator.Calculate(unlockedNodeIds);
        }

        private readonly ISkillUnlockRepository _skillUnlockRepository;
        private readonly PlayerStatusBonusCalculator _calculator;
    }
}
