using KillChord.Runtime.Adaptor.InGame.Haptics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.InGame.Haptics
{
    /// <summary>
    ///     ジャスト成立時にゲームパッドを短く振動させるView。
    ///     ゲームパッド未接続時（マウス・キーボード・タッチ操作時）は何も行わない。
    /// </summary>
    public sealed class GamepadHapticsView : MonoBehaviour, IGamepadHapticsViewModel
    {
        /// <summary>
        ///     ジャスト成立時の振動を一度だけ再生する。
        /// </summary>
        public void PlayJustHitPulse()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return;
            }

            gamepad.SetMotorSpeeds(LOW_FREQUENCY_MOTOR_SPEED, HIGH_FREQUENCY_MOTOR_SPEED);
            _remainingPulseTime = PULSE_DURATION;
        }

        private const float LOW_FREQUENCY_MOTOR_SPEED = 0.35f;
        private const float HIGH_FREQUENCY_MOTOR_SPEED = 0.6f;
        private const float PULSE_DURATION = 0.12f;

        private float _remainingPulseTime;

        /// <summary>
        ///     振動の残り時間を減算し、経過後にモーターを停止する。
        /// </summary>
        private void Update()
        {
            if (_remainingPulseTime <= 0f)
            {
                return;
            }

            _remainingPulseTime -= Time.unscaledDeltaTime;
            if (_remainingPulseTime <= 0f)
            {
                StopHaptics();
            }
        }

        /// <summary>
        ///     破棄時に振動が鳴り続けないよう停止する。
        /// </summary>
        private void OnDestroy()
        {
            StopHaptics();
        }

        /// <summary>
        ///     接続中のゲームパッドの振動を停止する。
        /// </summary>
        private void StopHaptics()
        {
            _remainingPulseTime = 0f;
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }
    }
}
