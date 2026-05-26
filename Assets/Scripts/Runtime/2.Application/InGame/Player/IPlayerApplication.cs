using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーアプリケーションの操作を定義するインターフェース。
    /// </summary>
    public interface IPlayerApplication
    {
        /// <summary> 回避入力を処理して回避開始を試行する。 </summary>
        public bool TryDodge(Vector2 input, float time);

        /// <summary> 入力と時刻に応じて回転と速度を更新する。 </summary>
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity);
    }
}
