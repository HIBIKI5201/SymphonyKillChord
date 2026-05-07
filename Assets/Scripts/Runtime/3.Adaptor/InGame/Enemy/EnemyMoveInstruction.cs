using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵の移動指示に関するデータを保持する構造体。
    /// </summary>
    public readonly struct EnemyMoveInstruction
    {
        public EnemyMoveInstruction(bool shouldMove, Vector3 destination, float moveSpeed)
        {
            ShouldMove = shouldMove;
            Destination = destination;
            MoveSpeed = moveSpeed;
        }

        /// <summary> 移動すべきか </summary>
        public bool ShouldMove { get; }
        /// <summary> 目的位置 </summary>
        public Vector3 Destination { get; }
        /// <summary> 移動速度 </summary>
        public float MoveSpeed { get; }
    }
}
