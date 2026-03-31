using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     敵の移動操作を表すインターフェース。
    /// </summary>
    public interface IEnemyNavigationAgent
    {
        bool IsReady { get; }
        void SetMoveSpeed(float moveSpeed);
        void MoveTo(Vector3 destination);
        void Stop();
    }
}
