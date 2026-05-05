using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Domain.Persistent.Input;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     プレイヤーの操作要求をアプリケーション層へ仲介するコントローラークラス。
    /// </summary>
    public class PlayerController : IPlayerController
    {
        public PlayerController(IPlayerApplication playerApplication, InputBufferingQueue inputBufferingQueue)
        {
            _playerApplication = playerApplication;
            _inputBufferingQueue = inputBufferingQueue;
        }

        public bool TryDodge(Vector2 input, float time)
            => _playerApplication.TryDodge(input, time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            _playerApplication.Update(ref rotation, input, time, out velocity);
        }

        private readonly IPlayerApplication _playerApplication;
        private readonly InputBufferingQueue _inputBufferingQueue;
    }
}
