using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象として必要な位置情報と生存状態を提供するインターフェース。
    /// </summary>
    public interface ILockOnTarget
    {
        /// <summary> ロックオン対象のワールド座標。 </summary>
        public Vector3 Position { get; }

        /// <summary> ロックオン対象が有効な状態であるかを示す。 </summary>
        public bool IsAlive { get; }
    }
}
