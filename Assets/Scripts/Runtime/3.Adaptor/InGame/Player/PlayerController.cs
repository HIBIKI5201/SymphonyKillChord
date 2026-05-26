using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Domain.Persistent.Input;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     アプリケーション層へプレイヤー操作を委譲するコントローラークラス。
    /// </summary>
    public class PlayerController : IPlayerController
    {
        private readonly IPlayerApplication _playerApplication;
        private readonly InputBufferingQueue _inputBufferingQueue;

        /// <summary> プレイヤーコントローラーを初期化する。 </summary>
        public PlayerController(IPlayerApplication playerApplication, InputBufferingQueue inputBufferingQueue)
        {
            _playerApplication = playerApplication;
            _inputBufferingQueue = inputBufferingQueue;
        }

        /// <summary> 回避開始を試行する。 </summary>
        public bool TryDodge(Vector2 input, float time)
            => _playerApplication.TryDodge(input, time);

        /// <summary> 向きと速度を更新する。 </summary>
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            _playerApplication.Update(ref rotation, input, time, out velocity);
        }
    }
}
