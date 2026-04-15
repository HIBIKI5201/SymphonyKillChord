using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
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
        public CharacterCombatSpec(IReadOnlyList<AttackDefinition> attackDifinitions)
        {
            if (attackDifinitions == null)
            {
                throw new ArgumentNullException(nameof(attackDifinitions));
            }
            _attackDifinitions = attackDifinitions;
        }

        public bool TryGetAttackDefinitionByBeatType(BeatType beatType, out AttackDefinition attackDefinition)
        {
            foreach (var attack in _attackDifinitions)
            {
                if (attack.BeatType.HasValue && attack.BeatType.Value == beatType)
                {
                    attackDefinition = attack;
                    return true;
                }
            }
            attackDefinition = null;
            return false;
        }

        public AttackDefinition GetAttackDefinitionByBeatType(BeatType beatType)
        {
            if (TryGetAttackDefinitionByBeatType(beatType, out var attackDefinition))
            {
                return attackDefinition;
            }
            throw new InvalidOperationException($"ビートタイプ{beatType}に対応する攻撃定義が見つかりませんでした。");
        }

        /// <summary>
        ///     指定した攻撃IDに対応する攻撃定義を取得する。
        /// </summary>
        /// <param name="id"> 取得したい攻撃ID。 </param>
        /// <returns>　対応する攻撃定義。　</returns>
        public AttackDefinition GetAttackDifinition(int index)
        {
            if (index < 0 || index >= _attackDifinitions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"攻撃IDは0以上{_attackDifinitions.Count - 1}以下でなければなりません。");
            }

            return _attackDifinitions[index];
        }

        private readonly IReadOnlyList<AttackDefinition> _attackDifinitions;
    }
}
