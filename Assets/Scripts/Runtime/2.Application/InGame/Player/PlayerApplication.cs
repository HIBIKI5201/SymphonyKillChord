using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Player
{
    /// <summary>
    ///     プレイヤーの移動処理と回避処理を統合するアプリケーションクラス。
    /// </summary>
    public sealed class PlayerApplication : IPlayerApplication
    {
        private readonly PlayerMovementApplication _movement;
        private readonly PlayerDodgeMovementApplication _dodge;

        /// <summary> プレイヤーアプリケーションを初期化する。 </summary>
        public PlayerApplication(PlayerMovementApplication movement, PlayerDodgeMovementApplication dodge)
        {
            _movement = movement;
            _dodge = dodge;
        }

        /// <summary> 回避開始を試行する。 </summary>
        public bool TryDodge(Vector2 input, float time)
            => _dodge.TryDodge(input, time);

        /// <summary> 回避中か通常時かに応じて移動更新を実行する。 </summary>
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (_dodge.IsDodging)
            {
                _dodge.Update(ref rotation, time, out velocity);
            }
            else
            {
                _movement.Update(ref rotation, input, out velocity);
            }
        }
    }
}
