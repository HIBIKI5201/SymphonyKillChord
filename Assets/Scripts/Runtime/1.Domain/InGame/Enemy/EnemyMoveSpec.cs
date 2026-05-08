using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵移動の能力値。
    /// </summary>
    public readonly struct EnemyMoveSpec
    {
        public EnemyMoveSpec(MoveSpeed moveSpeed, AttackRangeMin attackRangeMin, AttackRangeMax attackRangeMax)
        {
            MoveSpeed = moveSpeed;
            AttackRangeMin = attackRangeMin;
            AttackRangeMax = attackRangeMax;
        }

        /// <summary> 移動速度 </summary>
        public MoveSpeed MoveSpeed { get; }
        /// <summary> 最小攻撃距離 </summary>
        public AttackRangeMin AttackRangeMin { get; }
        /// <summary> 最大攻撃距離 </summary>
        public AttackRangeMax AttackRangeMax { get; }
    }
}
