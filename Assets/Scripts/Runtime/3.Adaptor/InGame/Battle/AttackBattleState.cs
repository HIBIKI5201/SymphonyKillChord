using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     現在の戦闘状態として、攻撃者と攻撃対象を保持するクラス。
    /// </summary>
    public class AttackBattleState
    {
        /// <summary> 攻撃者。 </summary>
        public CharacterEntity Attacker { get; private set; }
        /// <summary> 攻撃対象。 </summary>
        public IHitTarget Target { get; private set; }

        /// <summary>
        ///     攻撃者と攻撃対象を受け取り、状態を更新するメソッド。
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        public void Setup(CharacterEntity attacker, IHitTarget target)
        {
            Attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
