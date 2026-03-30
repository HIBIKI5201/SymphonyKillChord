using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃処理の文脈を保持するクラス。
    /// </summary>
    public struct AttackContext
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="attack"></param>
        public AttackContext(CharacterEntity attacker, IHitTarget target, AttackDefinition attack)
        {
            Attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Attack = attack ?? throw new ArgumentNullException(nameof(attack));
            CurrentDamage = attack.BaseDamage;
            IsCritical = false;
        }

        /// <summary> アタッカー。</summary>
        public CharacterEntity Attacker { get; }

        /// <summary>　ターゲット。 </summary>
        public IHitTarget Target { get; }

        /// <summary> 攻撃の基本情報。 </summary>
        public AttackDefinition Attack { get; }

        /// <summary> 現在のダメージ。 </summary>
        public Damage CurrentDamage { get; set; }

        /// <summary> クリティカルヒットかどうかを示すフラグ。 </summary>
        public bool IsCritical { get; set; }
    }
}
