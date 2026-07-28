using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using System;

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
        ///     スキルを発動する。対象を解決できない場合は演出のみの空撃ちとして扱う。
        /// </summary>
        /// <param name="skillDefinition"> 対象スキルです。 </param>
        /// <param name="beatType"> 入力の拍子種類です。 </param>
        /// <returns> 発動できた場合はtrue。 </returns>
        public bool TryExecuteSkill(SkillDefinition skillDefinition, BeatType beatType)
        {
            if (!_targetResolver.TryResolveTargets(skillDefinition.EffectSpec.TargetingType, out SkillTargetResolveResult targetResult))
            {
                // 対象が居なくても発動自体は成立させ、効果適用は行わない空撃ちにする。
                return true;
            }

            if (!_effectExecutorResolver.TryResolve(skillDefinition.EffectSpec.EffectType, out ISkillEffectExecutor executor))
            {
                throw new InvalidOperationException(
                    $"対応するスキル実行器が見つかりません。EffectType: {skillDefinition.EffectSpec.EffectType}");
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
