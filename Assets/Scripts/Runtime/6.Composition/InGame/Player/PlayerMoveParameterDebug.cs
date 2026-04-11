using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

#if UNITY_EDITOR

namespace KillChord.Runtime.Composition.InGame.Debugger
{
    /// <summary>
    ///     プレイヤーの移動パラメータをデバッグ表示するためのクラス。
    /// </summary>
    public sealed class PlayerMoveParameterDebug : MonoBehaviour
    {
        [SerializeField] private PlayerMoveParameter _playerMoveParameter;

        public void SetPlayerMoveParameter(PlayerMoveParameter parameter)
        {
            _playerMoveParameter = parameter;
        }
    }
}

#endif
