using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Player;
using System;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///   スキルID 02 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_02 : SkillBase
    {
        /// <summary>
        ///     スキル効果を初期化します。
        /// </summary>
        /// <param name="buff"> 付与バフです。 </param>
        /// <param name="attackController"> 攻撃実行器です。 </param>
        public Skill_02(IBuff buff, IAttackController attackController) : base(buff)
        {
            _attackController = attackController;
        }

        /// <summary>
        ///     スキル効果を実行します。
        /// </summary>
        /// <param name="context"> 実行コンテキストです。 </param>
        public override void Execute(in SkillEffectContext context)
        {
            _attackController?.Execute((int)_shotGunBeat);

            ReadOnlySpan<CharacterEntity> targets = context.TargetEntities.Span;
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i]?.BuffSystem.Add(_buff);
            }
        }

        private readonly BeatType _shotGunBeat = BeatType.Two;
        private readonly IAttackController _attackController;
    }
}
