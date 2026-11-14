using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Player
{
    /// <summary>
    ///     プレイヤー移動と回避の設定値を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerMoveSpecAsset), menuName = "KillChord/InGame/PlayerMoveSpecAsset")]
    public sealed class PlayerMoveSpecAsset : ScriptableObject
    {
        /// <summary> 試遊の案で、通常攻撃とスキルの攻撃硬直に掛ける倍率。 </summary>
        public float TunedRecoveryBeatsRate => _tunedRecoveryBeatsRate;

        /// <summary> 通常攻撃の攻撃硬直の既定値（拍数）。 </summary>
        private const float DEFAULT_ATTACK_RECOVERY_BEATS = 0.25f;

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

        [Header("試遊の案：移動・回避・硬直の調整（ゲージ注視の改善案）")]
        [SerializeField, Range(0f, 1f), Tooltip("通常移動速度に掛ける倍率。下げて、回避を立ち回りの中心にする。")]
        private float _tunedMoveSpeedRate = 0.7f;

        [SerializeField, Min(0f), Tooltip("回避クールダウン（拍数）。議事録の案は4拍子の1拍。")]
        private float _tunedDodgeCooldownBeats = 1f;

        [SerializeField, Min(1f), Tooltip("通常攻撃とスキルの攻撃硬直に掛ける倍率。")]
        private float _tunedRecoveryBeatsRate = 2f;

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

        /// <summary>
        ///     試遊の案（移動・回避・硬直の調整）の値で、ドメインパラメータへ変換する。
        ///     回避クールダウンは拍数で持つため、BPMから秒へ直す。
        /// </summary>
        /// <param name="bpm"> 戦闘のBPM。 </param>
        /// <returns> 調整した値のドメインパラメータ。 </returns>
        public PlayerMoveSpec ToTunedDomain(double bpm)
        {
            float secondsPerBeat = bpm > 0d ? (float)(MusicConstants.SECONDS_PER_MINUTE / bpm) : 0f;
            return new PlayerMoveSpec(
                new MoveSpeed(_moveSpeed * _tunedMoveSpeedRate),
                new AttackRotationSpeed(_attackRotationSpeed),
                new DodgeSpeed(_dodgeSpeed),
                new DodgeDuration(_dodgeDuration),
                new DodgeCooldown(bpm > 0d ? _tunedDodgeCooldownBeats * secondsPerBeat : _dodgeCooldown),
                new AttackCooldown(_attackCooldown),
                new AttackRecoveryBeats(_attackRecoveryBeats * _tunedRecoveryBeatsRate));
        }
    }
}

