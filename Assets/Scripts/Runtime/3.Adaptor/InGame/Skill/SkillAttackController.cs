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
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        public void Execute(int beatType, bool isJustHit)
        {
            if (!_targetResolver.TryResolveTargets(SkillTargetingType.CurrentTarget, out SkillTargetResolveResult result))
            {
                return;
            }

            ExecuteInternal((BeatType)beatType, result.PrimaryTargetEntity, isJustHit);
        }

        /// <summary>
        ///     指定対象に対して攻撃を実行します。
        /// </summary>
        /// <param name="beatType"> 攻撃ビートです。 </param>
        /// <param name="target"> 攻撃対象です。 </param>
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        public void Execute(int beatType, CharacterEntity target, bool isJustHit)
        {
            ExecuteInternal((BeatType)beatType, target, isJustHit);
        }

        /// <summary>
        ///     実際の攻撃処理を行います。
        /// </summary>
        /// <param name="beatType"> 攻撃ビートです。 </param>
        /// <param name="target"> 攻撃対象です。 </param>
        /// <param name="isJustHit"> ジャストヒットかどうか </param>
        private void ExecuteInternal(BeatType beatType, CharacterEntity target, bool isJustHit)
        {
            if (_playerEntity == null || target == null)
            {
                return;
            }

            try
            {
                AttackDefinition attackDefinition =
                    _playerEntity.CombatSpec.GetAttackDefinitionByBeatType(beatType);

                AttackExecutor.Execute(
                    attackDefinition,
                    _playerEntity,
                    target,
                    isJustHit,
                    _playerEntity.
                    BaseDamage,
                    damageAttackType: DamageAttackType.Skill);
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
