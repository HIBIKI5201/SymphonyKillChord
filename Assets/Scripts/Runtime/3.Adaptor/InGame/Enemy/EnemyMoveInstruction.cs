using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public readonly struct EnemyMoveInstruction
    {
        public EnemyMoveInstruction(bool shouldMove, Vector3 destination, float moveSpeed)
        {
            ShouldMove = shouldMove;
            Destination = destination;
            MoveSpeed = moveSpeed;
        }

        public bool ShouldMove { get; }
        public Vector3 Destination { get; }
        public float MoveSpeed { get; }
    }
}
