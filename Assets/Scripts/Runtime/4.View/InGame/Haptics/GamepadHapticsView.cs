using KillChord.Runtime.Adaptor.InGame.Haptics;
using KillChord.Runtime.Adaptor.Persistent.Environment;
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
        ///     振動の強さと長さの設定を受け取る。
        /// </summary>
        /// <param name="config"> ゲームパッド振動の設定。 </param>
        /// <param name="environmentSettingsViewModel"> 保存済みの振動強度を公開するViewModel。 </param>
        public void Initialize(
            GamepadHapticsConfig config,
            IEnvironmentSettingsViewModel environmentSettingsViewModel)
        {
            _config = config;
            _environmentSettingsViewModel = environmentSettingsViewModel;
        }

        /// <summary>
        ///     ジャスト成立時の振動を一度だけ再生する。
        /// </summary>
        public void PlayJustHitPulse()
        {
            if (_config == null || _environmentSettingsViewModel == null)
            {
                Debug.LogError($"[{nameof(GamepadHapticsView)}] 振動再生に必要な設定が未設定です。", this);
                return;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad == null)
            {
                return;
            }

            float vibrationScale = Mathf.Clamp01(_environmentSettingsViewModel.VibrationScale.CurrentValue);
            if (_config.PulseDuration <= 0f || vibrationScale <= 0f)
            {
                return;
            }

            gamepad.SetMotorSpeeds(
                _config.LowFrequencyMotorSpeed * vibrationScale,
                _config.HighFrequencyMotorSpeed * vibrationScale);
            _pulsingGamepad = gamepad;
            _remainingPulseTime = _config.PulseDuration;
        }

        private GamepadHapticsConfig _config;
        private IEnvironmentSettingsViewModel _environmentSettingsViewModel;
        private Gamepad _pulsingGamepad;
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
        ///     振動を開始したゲームパッドの振動を停止する。
        /// </summary>
        private void StopHaptics()
        {
            _remainingPulseTime = 0f;

            if (_pulsingGamepad != null && _pulsingGamepad.added)
            {
                _pulsingGamepad.SetMotorSpeeds(0f, 0f);
            }

            _pulsingGamepad = null;
        }
    }
}
