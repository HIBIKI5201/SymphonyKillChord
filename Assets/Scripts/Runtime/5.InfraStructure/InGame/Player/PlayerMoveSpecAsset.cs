using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Player
{
    /// <summary>
    ///     プレイヤー移動と回避の設定値を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerMoveSpecAsset), menuName = "KillChord/InGame/PlayerMoveSpecAsset")]
    public sealed class PlayerMoveSpecAsset : ScriptableObject
    {
        /// <summary> 通常攻撃の攻撃硬直の既定値（拍数）。 </summary>
        private const float DEFAULT_ATTACK_RECOVERY_BEATS = 0.5f;

        [SerializeField, Tooltip("通常移動速度。")]
        private float _moveSpeed;
        [SerializeField,Tooltip("攻撃した際の回転速度")] 
        private float _attackRotationSpeed;

        [Space]
        [SerializeField, Tooltip("回避移動速度。")]
        private float _dodgeSpeed;

        [SerializeField, Tooltip("回避継続時間。")]
        private float _dodgeDuration;

        [SerializeField, Tooltip("回避クールダウン時間。")]
        private float _dodgeCooldown;

        [SerializeField, Tooltip("回避の後に攻撃できない時間（拍数）。")]
        private float _attackCooldown;

        [SerializeField, Min(0f), Tooltip("通常攻撃の攻撃硬直（拍数）。この間は移動・攻撃できない。BPMが変わっても拍に合わせる。")]
        private float _attackRecoveryBeats = DEFAULT_ATTACK_RECOVERY_BEATS;

        /// <summary> ScriptableObjectからドメインパラメータへ変換する。 </summary>
        public PlayerMoveSpec ToDomain()
            => new PlayerMoveSpec(
                new MoveSpeed(_moveSpeed),
                new AttackRotationSpeed(_attackRotationSpeed),
                new DodgeSpeed(_dodgeSpeed),
                new DodgeDuration(_dodgeDuration),
                new DodgeCooldown(_dodgeCooldown),
                new AttackCooldown(_attackCooldown),
                new AttackRecoveryBeats(_attackRecoveryBeats)
            );
    }
}

