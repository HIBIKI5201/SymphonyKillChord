using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
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
            Attacker = attacker;
            Target = target;
        }
    }
}
