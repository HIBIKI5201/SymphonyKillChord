using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     プレイヤー操作の入口を定義するインターフェース。
    /// </summary>
    public interface IPlayerController
    {
        /// <summary> 回避開始を試行する。 </summary>
        public bool TryDodge(Vector2 input, float time);

        /// <summary> 向きと速度を更新する。 </summary>
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity);
    }
}
