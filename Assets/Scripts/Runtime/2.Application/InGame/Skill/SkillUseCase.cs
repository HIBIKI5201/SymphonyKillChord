using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキル発動の判定と実行を扱うユースケースクラス。
    /// </summary>
    public class SkillUsecase
    {
        /// <summary>
        ///     コンストラクタ。必要なサービスを注入する。
        /// </summary>
        public SkillUsecase(
            ISkillTargetResolver targetResolver,
            ISkillEffectExecutorResolver effectExecutorResolver,
            CharacterEntity playerEntity)
        {
            _targetResolver = targetResolver;
            _effectExecutorResolver = effectExecutorResolver;
            _playerEntity = playerEntity;
        }

        /// <summary>
        ///     スキルが発動可能な場合、発動する。
        /// </summary>
        /// <param name="skillDefinition"> 対象スキルです。 </param>
        /// <param name="beatType"> 入力の拍子種類です。 </param>
        /// <returns> 発動できた場合はtrue。 </returns>
        public bool TryExecuteSkill(SkillDefinition skillDefinition, BeatType beatType)
        {
            if (!_targetResolver.TryResolveTargets(skillDefinition.EffectSpec.TargetingType, out SkillTargetResolveResult targetResult))
            {
                return false;
            }

            if (!_effectExecutorResolver.TryResolve(skillDefinition.EffectSpec.EffectType, out ISkillEffectExecutor executor))
            {
                Debug.LogError($"[{nameof(SkillUsecase)}] 対応するスキル実行器が見つかりません。 EffectType: {skillDefinition.EffectSpec.EffectType}");
                return false;
            }

            SkillEffectContext context = new SkillEffectContext(
                targetResult.PrimaryTargetEntity,
                _playerEntity,
                beatType,
                targetResult.TargetEntities);
            executor.Execute(context);
            _playerEntity.BuffSystem.Execute(new BuffContext(_playerEntity, targetResult.PrimaryTargetEntity), BuffExecuteTiming.Skill);
            return true;
        }

        private readonly ISkillTargetResolver _targetResolver;
        private readonly ISkillEffectExecutorResolver _effectExecutorResolver;
        private readonly CharacterEntity _playerEntity;
    }
}
