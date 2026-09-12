using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;

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
        ///     プレイヤーのステータスを反映する。
        /// </summary>
        public void Push()
        {
            PlayerStatusBonus bonus = _bonusCalculator.Calculate(_skillTreeStatus.UnlockedNodes);
            float areaAttackRangeMultiplier = _baseAreaAttackRange <= 0f
                ? 1f
                : (_baseAreaAttackRange + bonus.AreaAttackRangeAddition) / _baseAreaAttackRange;
            PlayerStatusDTO dto = new PlayerStatusDTO(
                _baseHealth * bonus.MaxHealthMultiplier,
                _baseAttack * bonus.AttackPowerMultiplier,
                Math.Min(1f, _baseCriticalChance + bonus.CriticalChanceAddition),
                _baseCriticalDamageMultiplier - 1f + bonus.CriticalMultiplierAddition,
                areaAttackRangeMultiplier);
            _viewModel.Apply(dto);
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
