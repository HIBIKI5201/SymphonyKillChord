using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

#if UNITY_EDITOR
namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     プレイヤー移動パラメータをデバッグ表示するクラス。
    /// </summary>
    public sealed class PlayerMoveSpecDebug : MonoBehaviour
    {
        [SerializeField, Tooltip("デバッグ表示対象のプレイヤー移動パラメータ。")]
        private PlayerMoveSpec _playerMoveParameter;

        /// <summary> プレイヤー移動パラメータを設定する。 </summary>
        public void SetPlayerMoveSpec(PlayerMoveSpec parameter)
        {
            _playerMoveParameter = parameter;
        }
    }
}
#endif

