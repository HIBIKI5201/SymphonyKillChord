using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     クリティカルヒットを処理する攻撃処理ステップ。
    /// </summary>
    public class CriticalStep : IAttackStep
    {
        public void Execute(ref AttackContext context)
        {
            if (Random.value <= _criticalChance)
            {
                context.IsCritical = true;
                context.CurrentDamage *= _criticalMultiplier;
            }
        }

        [SerializeField, Range(0f, 1f), Tooltip("クリティカル発生率")] private float _criticalChance;
        [SerializeField, Tooltip("クリティカル倍率")] private float _criticalMultiplier;

    }
}
