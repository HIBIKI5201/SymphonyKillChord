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

        public MoveSpeed MoveSpeed { get; }
        public AttackRangeMin AttackRangeMin { get; }
        public AttackRangeMax AttackRangeMax { get; }
    }
}
