using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     確定ダメージを適用する攻撃処理ステップ。
    ///     攻撃のダメージが確定ダメージより小さい場合、確定ダメージに置き換える。
    /// </summary>
    public class ConfirmedDamage : IAttackStep
    {
        public void Execute(AttackContext context)
        {
            if (context.CurrentDamage.Value < _confirmedDamage)
            {
                context.CurrentDamage = new Damage(_confirmedDamage);
            }
        }

        [SerializeField, Tooltip("確定ダメージ。")] private float _confirmedDamage;
    }
}
