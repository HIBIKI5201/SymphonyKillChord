using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Player;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     プレイヤーの操作要求をアプリケーション層へ仲介するコントローラークラス。
    /// </summary>
    public class PlayerController
    {
        public PlayerController(PlayerApplication playerApplication)
        {
            _playerApplication = playerApplication;
        }

        public bool TryDodge(Vector2 input, float time)
            => _playerApplication.TryDodge(input, time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            _playerApplication.Update(ref rotation, input, time, out velocity);
        }

        private readonly PlayerApplication _playerApplication;

    }
}
