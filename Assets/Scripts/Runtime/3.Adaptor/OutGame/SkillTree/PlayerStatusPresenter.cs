using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス画面のPresenter。
    /// </summary>
    public class PlayerStatusPresenter
    {
        /// <summary>
        ///     表示先、ボーナス計算器、基礎ステータスを設定する。
        /// </summary>
        public PlayerStatusPresenter(
            IPlayerStatusViewModel viewModel,
            PlayerStatusBonusCalculator bonusCalculator,
            SkillTreeStatusEntity skillTreeStatus,
            float baseHealth,
            float baseAttack,
            float baseCriticalChance,
            float baseCriticalDamageMultiplier,
            float baseAreaAttackRange)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _bonusCalculator = bonusCalculator ?? throw new ArgumentNullException(nameof(bonusCalculator));
            _skillTreeStatus = skillTreeStatus ?? throw new ArgumentNullException(nameof(skillTreeStatus));
            _baseHealth = baseHealth;
            _baseAttack = baseAttack;
            _baseCriticalChance = baseCriticalChance;
            _baseCriticalDamageMultiplier = baseCriticalDamageMultiplier;
            _baseAreaAttackRange = baseAreaAttackRange;
        }

        /// <summary>
        ///     プレイヤーのステータスを反映する(プレビューなし、解放済み分のみ)。
        /// </summary>
        public void Push()
        {
            (float health, float attack, float criticalChance, float criticalDamage, float areaAttackRangeMultiplier) current =
                ComputeStats(_skillTreeStatus.UnlockedNodes);
            PlayerStatusDTO dto = new PlayerStatusDTO(
                current.health,
                current.attack,
                current.criticalChance,
                current.criticalDamage,
                current.areaAttackRangeMultiplier,
                current.health,
                current.attack,
                current.criticalChance,
                current.criticalDamage,
                current.areaAttackRangeMultiplier);
            _viewModel.Apply(dto);
        }

        /// <summary>
        ///     選択中ノードを解放した場合の変化をプレビューとして反映する。
        /// </summary>
        /// <param name="previewNodes"> 選択中ノードを解放するために追加で解放されるノード一覧(既に解放済みの場合は空)。 </param>
        public void PushPreview(IReadOnlyCollection<SkillNodeEntity> previewNodes)
        {
            (float health, float attack, float criticalChance, float criticalDamage, float areaAttackRangeMultiplier) current =
                ComputeStats(_skillTreeStatus.UnlockedNodes);

            (float health, float attack, float criticalChance, float criticalDamage, float areaAttackRangeMultiplier) preview = current;
            if (previewNodes != null && previewNodes.Count > 0)
            {
                IEnumerable<SkillNodeId> previewNodeIds = _skillTreeStatus.UnlockedNodes
                    .Concat(previewNodes.Select(node => node.SkillNodeIdVO));
                preview = ComputeStats(previewNodeIds);
            }

            PlayerStatusDTO dto = new PlayerStatusDTO(
                current.health,
                current.attack,
                current.criticalChance,
                current.criticalDamage,
                current.areaAttackRangeMultiplier,
                preview.health,
                preview.attack,
                preview.criticalChance,
                preview.criticalDamage,
                preview.areaAttackRangeMultiplier);
            _viewModel.Apply(dto);
        }

        /// <summary>
        ///     指定したノードID群からプレイヤーステータスを計算する。
        /// </summary>
        /// <param name="nodeIds"> 集計対象のノードID群。 </param>
        /// <returns> 計算済みの各ステータス値。 </returns>
        private (float health, float attack, float criticalChance, float criticalDamage, float areaAttackRangeMultiplier) ComputeStats(
            IEnumerable<SkillNodeId> nodeIds)
        {
            PlayerStatusBonus bonus = _bonusCalculator.Calculate(nodeIds);
            float areaAttackRangeMultiplier = _baseAreaAttackRange <= 0f
                ? 1f
                : (_baseAreaAttackRange + bonus.AreaAttackRangeAddition) / _baseAreaAttackRange;
            return (
                _baseHealth * bonus.MaxHealthMultiplier,
                _baseAttack * bonus.AttackPowerMultiplier,
                Math.Min(1f, _baseCriticalChance + bonus.CriticalChanceAddition),
                _baseCriticalDamageMultiplier - 1f + bonus.CriticalMultiplierAddition,
                areaAttackRangeMultiplier);
        }

        private readonly IPlayerStatusViewModel _viewModel;
        private readonly PlayerStatusBonusCalculator _bonusCalculator;
        private readonly SkillTreeStatusEntity _skillTreeStatus;
        private readonly float _baseHealth;
        private readonly float _baseAttack;
        private readonly float _baseCriticalChance;
        private readonly float _baseCriticalDamageMultiplier;
        private readonly float _baseAreaAttackRange;
    }
}
