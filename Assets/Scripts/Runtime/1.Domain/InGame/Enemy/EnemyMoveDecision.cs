using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵移動の結果を表す構造体。
    ///     敵が移動すべきか、移動先の座標、移動速度を含む。
    /// </summary>
    public readonly struct EnemyMoveDecision
    {
        public EnemyMoveDecision(bool shouldMove, Vector3 destination, float speed)
        {
            ShouldMove = shouldMove;
            Destination = destination;
            Speed = speed;
        }

        public bool ShouldMove { get; }
        public Vector3 Destination { get; }
        public float Speed { get; }
    }
}
