using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     現在の戦闘状態として、攻撃者と攻撃対象を保持するクラス。
    /// </summary>
    public class PlayerBattleState
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attacker"></param>
        public PlayerBattleState(CharacterEntity attacker)
        {
            Attacker = attacker;
        }
        /// <summary> 攻撃者を取得する。 </summary>
        public CharacterEntity Attacker { get; }
        /// <summary> 攻撃対象を取得する。 </summary>
        public IDefender Target { get; private set; }

        /// <summary>
        ///     攻撃者と攻撃対象を受け取り、状態を更新するメソッド。
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        public void ChangeTarget(IDefender target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
