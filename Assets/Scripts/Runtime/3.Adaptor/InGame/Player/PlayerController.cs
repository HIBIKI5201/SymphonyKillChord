using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Domain.Persistent.Input;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     アプリケーション層へプレイヤー操作を委譲するコントローラークラス。
    /// </summary>
    public class PlayerController : IPlayerController
    {
        /// <summary> プレイヤーが移動した時に発火します。 </summary>
        public event Action OnMoved;

        /// <summary> 回避に成功した時に発火します（敵の攻撃を避けられたかどうかは問いません）。 </summary>
        public event Action OnDodgeSucceeded;

        private readonly IPlayerApplication _playerApplication;
        private readonly InputBufferingQueue _inputBufferingQueue;
        private const float MOVE_INPUT_THRESHOLD = 0.15f;

        /// <summary> プレイヤーコントローラーを初期化する。 </summary>
        public PlayerController(IPlayerApplication playerApplication, InputBufferingQueue inputBufferingQueue)
        {
            _playerApplication = playerApplication;
            _inputBufferingQueue = inputBufferingQueue;
        }

        /// <summary> 回避開始を試行する。 </summary>
        public bool TryDodge(Vector2 input, float time)
        {
            bool succeeded = _playerApplication.TryDodge(input, time);

            if (succeeded)
            {
                OnDodgeSucceeded?.Invoke();
            }

            return succeeded;
        }

        /// <summary> 向きと速度を更新する。 </summary>
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            _playerApplication.Update(ref rotation, input, time, out velocity);
            if (input.sqrMagnitude >= MOVE_INPUT_THRESHOLD * MOVE_INPUT_THRESHOLD)
            {
                OnMoved?.Invoke();
            }
        }

        /// <summary> 回避中かどうかを取得する。 </summary>
        public bool IsDodging => _playerApplication.IsDodging;
    }
}
