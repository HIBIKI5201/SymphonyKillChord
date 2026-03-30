using UnityEngine;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     敵移動の能力値。
    /// </summary>
    public readonly struct EnemyMoveSpec
    {
        public EnemyMoveSpec(MoveSpeed moveSpeed, AttackRange attackRange)
        {
            MoveSpeed = moveSpeed;
            AttackRange = attackRange;
        }

        public MoveSpeed MoveSpeed { get; }
        public AttackRange AttackRange { get; }
    }
}
