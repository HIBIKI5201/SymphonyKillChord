using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

#if UNITY_EDITOR
namespace KillChord.Runtime.Composition.InGame.Debugger
{
    /// <summary>
    ///     プレイヤー移動パラメータをデバッグ表示するクラス。
    /// </summary>
    public sealed class PlayerMoveParameterDebug : MonoBehaviour
    {
        [SerializeField, Tooltip("デバッグ表示対象のプレイヤー移動パラメータ。")]
        private PlayerMoveParameter _playerMoveParameter;

        /// <summary> プレイヤー移動パラメータを設定する。 </summary>
        public void SetPlayerMoveParameter(PlayerMoveParameter parameter)
        {
            _playerMoveParameter = parameter;
        }
    }
}
#endif
