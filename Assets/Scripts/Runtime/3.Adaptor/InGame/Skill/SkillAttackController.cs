using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Utility.Persistent;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル専用の攻撃実行アダプターです。
    /// </summary>
    public sealed class SkillAttackController : IAttackController
    {
        /// <summary>
        ///     スキル専用攻撃コントローラーを初期化します。
        /// </summary>
        /// <param name="playerEntity"> プレイヤーEntityです。 </param>
        /// <param name="targetResolver"> 対象解決サービスです。 </param>
        public SkillAttackController(CharacterEntity playerEntity, ISkillTargetResolver targetResolver)
        {
            _playerEntity = playerEntity;
            _targetResolver = targetResolver;
        }

        /// <summary>
        ///     現在ターゲットに対して攻撃を実行します。
        /// </summary>
        /// <param name="beatType"> 攻撃ビートです。 </param>
        public void Execute(int beatType)
        {
            if (!_targetResolver.TryResolveTargets(SkillTargetingType.CurrentTarget, out SkillTargetResolveResult result))
            {
                return;
            }

            ExecuteInternal((BeatType)beatType, result.PrimaryTargetEntity);
        }

        /// <summary>
        ///     指定対象に対して攻撃を実行します。
        /// </summary>
        /// <param name="beatType"> 攻撃ビートです。 </param>
        /// <param name="target"> 攻撃対象です。 </param>
        public void Execute(int beatType, CharacterEntity target)
        {
            ExecuteInternal((BeatType)beatType, target);
        }

        /// <summary>
        ///     実際の攻撃処理を行います。
        /// </summary>
        /// <param name="beatType"> 攻撃ビートです。 </param>
        /// <param name="target"> 攻撃対象です。 </param>
        private void ExecuteInternal(BeatType beatType, CharacterEntity target)
        {
            if (_playerEntity == null || target == null)
            {
                return;
            }

            try
            {
                AttackDefinition attackDefinition = _playerEntity.CombatSpec.GetAttackDefinitionByBeatType(beatType);
                AttackResult result = AttackExecutor.Execute(attackDefinition, _playerEntity, target, false, _playerEntity.BaseDamage,damageAttackType: DamageAttackType.Skill);
                EventBus<EOnTakeDamage>.Raise(new EOnTakeDamage(result.FinalDamage.Value, result.IsCritical, target.Id, DamageAttackType.Skill));
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }

        private readonly CharacterEntity _playerEntity;
        private readonly ISkillTargetResolver _targetResolver;
    }
}
