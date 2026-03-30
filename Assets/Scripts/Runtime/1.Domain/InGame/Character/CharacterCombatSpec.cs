using KillChord.Runtime.Domain.InGame.Battle;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクターが使用できる攻撃定義を管理するクラス。
    /// </summary>
    public class CharacterCombatSpec
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attackDifinitions"></param>
        public CharacterCombatSpec(IReadOnlyDictionary<AttackId, AttackDefinition> attackDifinitions)
        {
            if (attackDifinitions == null)
            {
                throw new ArgumentNullException(nameof(attackDifinitions));
            }
            _attackDifinitions = attackDifinitions;
        }

        /// <summary>
        ///     指定した攻撃IDに対応する攻撃定義を取得する。
        /// </summary>
        /// <param name="id"> 取得したい攻撃ID。 </param>
        /// <returns>　対応する攻撃定義。　</returns>
        public AttackDefinition GetAttackDifinition(AttackId id)
        {
            if (_attackDifinitions.TryGetValue(id, out var difinition))
            {
                return difinition;
            }

            throw new InvalidOperationException($"AttackId {id} is not defined in this CharacterCombatSpec.");
        }

        private readonly IReadOnlyDictionary<AttackId, AttackDefinition> _attackDifinitions;
    }
}
