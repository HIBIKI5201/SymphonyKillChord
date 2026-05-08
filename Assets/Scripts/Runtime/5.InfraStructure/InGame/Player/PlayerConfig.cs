using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Player
{
    /// <summary>
    ///     プレイヤー移動と回避の設定値を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = "KillChord/InGame/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField, Tooltip("通常移動速度。")]
        private float _moveSpeed;

        [Space]
        [SerializeField, Tooltip("回避移動速度。")]
        private float _dodgeSpeed;

        [SerializeField, Tooltip("回避継続時間。")]
        private float _dodgeDuration;

        [SerializeField, Tooltip("回避クールダウン時間。")]
        private float _dodgeCooldown;

        /// <summary> ScriptableObjectからドメインパラメータへ変換する。 </summary>
        public PlayerMoveParameter ToDomain()
            => new PlayerMoveParameter(
                new MoveSpeed(_moveSpeed),
                new DodgeSpeed(_dodgeSpeed),
                new DodgeDuration(_dodgeDuration),
                new DodgeCooldown(_dodgeCooldown)
            );
    }
}
