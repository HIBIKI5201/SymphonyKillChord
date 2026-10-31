using UnityEngine;

namespace KillChord.Runtime.View.InGame.Haptics
{
    /// <summary>
    ///     ジャスト成立時に再生するゲームパッド振動の強さと長さを保持する。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(GamepadHapticsConfig),
        menuName = "KillChord/InGame/Haptics/Gamepad Haptics Config")]
    /// <summary>
    ///     ゲームパッドの振動の強さと長さを設定するデータ。
    /// </summary>
    public sealed class GamepadHapticsConfig : ScriptableObject
    {
        /// <summary> 低周波モーターの振動の強さ（0〜1）。 </summary>
        public float LowFrequencyMotorSpeed => _lowFrequencyMotorSpeed;

        /// <summary> 高周波モーターの振動の強さ（0〜1）。 </summary>
        public float HighFrequencyMotorSpeed => _highFrequencyMotorSpeed;

        /// <summary> 振動を再生する時間（秒）。 </summary>
        public float PulseDuration => _pulseDuration;

        /// <summary> 被弾時の低周波モーターの振動の強さ（0〜1）。 </summary>
        public float DamageLowFrequencyMotorSpeed => _damageLowFrequencyMotorSpeed;

        /// <summary> 被弾時の高周波モーターの振動の強さ（0〜1）。 </summary>
        public float DamageHighFrequencyMotorSpeed => _damageHighFrequencyMotorSpeed;

        /// <summary> 被弾時に振動を再生する時間（秒）。 </summary>
        public float DamagePulseDuration => _damagePulseDuration;

        [SerializeField, Range(0f, 1f), Tooltip("低周波モーターの振動の強さ。")]
        private float _lowFrequencyMotorSpeed = 0.35f;

        [SerializeField, Range(0f, 1f), Tooltip("高周波モーターの振動の強さ。")]
        private float _highFrequencyMotorSpeed = 0.6f;

        [SerializeField, Min(0f), Tooltip("振動を再生する時間（秒）。")]
        private float _pulseDuration = 0.12f;

        [SerializeField, Range(0f, 1f), Tooltip("被弾時の低周波モーターの振動の強さ。")]
        private float _damageLowFrequencyMotorSpeed = 0.6f;

        [SerializeField, Range(0f, 1f), Tooltip("被弾時の高周波モーターの振動の強さ。")]
        private float _damageHighFrequencyMotorSpeed = 0.2f;

        [SerializeField, Min(0f), Tooltip("被弾時に振動を再生する時間（秒）。短くする。")]
        private float _damagePulseDuration = 0.15f;
    }
}
