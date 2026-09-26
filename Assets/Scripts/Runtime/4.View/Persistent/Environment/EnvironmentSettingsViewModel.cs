using KillChord.Runtime.Adaptor.Persistent.Environment;
using R3;
using System;

namespace KillChord.Runtime.View.Persistent.Environment
{
    /// <summary>
    ///     UIへ公開する環境設定の表示状態を保持するViewModel。
    /// </summary>
    public sealed class EnvironmentSettingsViewModel : IEnvironmentSettingsViewModel, IDisposable
    {
        /// <summary>
        ///     環境設定ViewModelを初期化する。
        /// </summary>
        public EnvironmentSettingsViewModel()
        {
            _resolutionLabel = new ReactiveProperty<string>(string.Empty);
            _screenModeLabel = new ReactiveProperty<string>(string.Empty);
            _qualityLevelLabel = new ReactiveProperty<string>(string.Empty);
            _brightness = new ReactiveProperty<int>();
            _languageLabel = new ReactiveProperty<string>(string.Empty);
            _vibrationStrengthLabel = new ReactiveProperty<string>(string.Empty);
            _vibrationScale = new ReactiveProperty<float>();
            _rhythmOffsetStep = new ReactiveProperty<int>();
            _rhythmOffsetLabel = new ReactiveProperty<string>(string.Empty);
            _rhythmOffsetSeconds = new ReactiveProperty<float>();
            _cameraSensitivity = new ReactiveProperty<int>();
            _cameraSensitivityScale = new ReactiveProperty<float>(1f);
            _cameraInvertModeLabel = new ReactiveProperty<string>(string.Empty);
            _isCameraInvertVertical = new ReactiveProperty<bool>();
            _isCameraInvertHorizontal = new ReactiveProperty<bool>();
            _autoLockOnLabel = new ReactiveProperty<string>(string.Empty);
            _isAutoLockOnEnabled = new ReactiveProperty<bool>(true);
            _buttonLayoutLabel = new ReactiveProperty<string>(string.Empty);
            _isJapaneseButtonLayout = new ReactiveProperty<bool>();
        }

        /// <summary> 解像度の表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> ResolutionLabel => _resolutionLabel;

        /// <summary> 画面モードの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> ScreenModeLabel => _screenModeLabel;

        /// <summary> 画質プリセットの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> QualityLevelLabel => _qualityLevelLabel;

        /// <summary> 画面の明るさ。 </summary>
        public ReadOnlyReactiveProperty<int> Brightness => _brightness;

        /// <summary> 表示言語の表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> LanguageLabel => _languageLabel;

        /// <summary> ゲームパッド振動の強さの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> VibrationStrengthLabel => _vibrationStrengthLabel;

        /// <summary> ゲームパッド振動へ適用する強さの倍率。 </summary>
        public ReadOnlyReactiveProperty<float> VibrationScale => _vibrationScale;

        /// <summary> リズム判定オフセットの段階（0.05秒刻み、-6～6）。 </summary>
        public ReadOnlyReactiveProperty<int> RhythmOffsetStep => _rhythmOffsetStep;

        /// <summary> リズム判定オフセットの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> RhythmOffsetLabel => _rhythmOffsetLabel;

        /// <summary> リズム判定タイミングへ加算するオフセット秒数。 </summary>
        public ReadOnlyReactiveProperty<float> RhythmOffsetSeconds => _rhythmOffsetSeconds;

        /// <summary> カメラ感度（1～10）。 </summary>
        public ReadOnlyReactiveProperty<int> CameraSensitivity => _cameraSensitivity;

        /// <summary> カメラ入力へ掛ける感度の倍率。 </summary>
        public ReadOnlyReactiveProperty<float> CameraSensitivityScale => _cameraSensitivityScale;

        /// <summary> カメラ操作の反転方向の表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> CameraInvertModeLabel => _cameraInvertModeLabel;

        /// <summary> カメラの上下操作を反転するかどうか。 </summary>
        public ReadOnlyReactiveProperty<bool> IsCameraInvertVertical => _isCameraInvertVertical;

        /// <summary> カメラの左右操作を反転するかどうか。 </summary>
        public ReadOnlyReactiveProperty<bool> IsCameraInvertHorizontal => _isCameraInvertHorizontal;

        /// <summary> オートロックオンの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> AutoLockOnLabel => _autoLockOnLabel;

        /// <summary> 攻撃時のオートロックオンを使うかどうか。 </summary>
        public ReadOnlyReactiveProperty<bool> IsAutoLockOnEnabled => _isAutoLockOnEnabled;

        /// <summary> ゲームパッドの決定・キャンセルの配置の表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> ButtonLayoutLabel => _buttonLayoutLabel;

        /// <summary> ゲームパッドの決定・キャンセルが日本式（決定=右ボタン）かどうか。 </summary>
        public ReadOnlyReactiveProperty<bool> IsJapaneseButtonLayout => _isJapaneseButtonLayout;

        /// <summary>
        ///     表示用DTOを環境設定へ反映する。
        /// </summary>
        public void Apply(in EnvironmentSettingsViewDTO dto)
        {
            _resolutionLabel.Value = dto.ResolutionLabel;
            _screenModeLabel.Value = dto.ScreenModeLabel;
            _qualityLevelLabel.Value = dto.QualityLevelLabel;
            _brightness.Value = dto.Brightness;
            _languageLabel.Value = dto.LanguageLabel;
            _vibrationStrengthLabel.Value = dto.VibrationStrengthLabel;
            _vibrationScale.Value = dto.VibrationScale;
            _rhythmOffsetStep.Value = dto.RhythmOffsetStep;
            _rhythmOffsetLabel.Value = dto.RhythmOffsetLabel;
            _rhythmOffsetSeconds.Value = dto.RhythmOffsetSeconds;
            _cameraSensitivity.Value = dto.CameraSensitivity;
            _cameraSensitivityScale.Value = dto.CameraSensitivityScale;
            _cameraInvertModeLabel.Value = dto.CameraInvertModeLabel;
            _isCameraInvertVertical.Value = dto.IsCameraInvertVertical;
            _isCameraInvertHorizontal.Value = dto.IsCameraInvertHorizontal;
            _autoLockOnLabel.Value = dto.AutoLockOnLabel;
            _isAutoLockOnEnabled.Value = dto.IsAutoLockOnEnabled;
            _buttonLayoutLabel.Value = dto.ButtonLayoutLabel;
            _isJapaneseButtonLayout.Value = dto.IsJapaneseButtonLayout;
        }

        /// <summary>
        ///     ReactivePropertyを解放する。
        /// </summary>
        public void Dispose()
        {
            _resolutionLabel.Dispose();
            _screenModeLabel.Dispose();
            _qualityLevelLabel.Dispose();
            _brightness.Dispose();
            _languageLabel.Dispose();
            _vibrationStrengthLabel.Dispose();
            _vibrationScale.Dispose();
            _rhythmOffsetStep.Dispose();
            _rhythmOffsetLabel.Dispose();
            _rhythmOffsetSeconds.Dispose();
            _cameraSensitivity.Dispose();
            _cameraSensitivityScale.Dispose();
            _cameraInvertModeLabel.Dispose();
            _isCameraInvertVertical.Dispose();
            _isCameraInvertHorizontal.Dispose();
            _autoLockOnLabel.Dispose();
            _isAutoLockOnEnabled.Dispose();
            _buttonLayoutLabel.Dispose();
            _isJapaneseButtonLayout.Dispose();
        }

        private readonly ReactiveProperty<string> _resolutionLabel;
        private readonly ReactiveProperty<string> _screenModeLabel;
        private readonly ReactiveProperty<string> _qualityLevelLabel;
        private readonly ReactiveProperty<int> _brightness;
        private readonly ReactiveProperty<string> _languageLabel;
        private readonly ReactiveProperty<string> _vibrationStrengthLabel;
        private readonly ReactiveProperty<float> _vibrationScale;
        private readonly ReactiveProperty<int> _rhythmOffsetStep;
        private readonly ReactiveProperty<string> _rhythmOffsetLabel;
        private readonly ReactiveProperty<float> _rhythmOffsetSeconds;
        private readonly ReactiveProperty<int> _cameraSensitivity;
        private readonly ReactiveProperty<float> _cameraSensitivityScale;
        private readonly ReactiveProperty<string> _cameraInvertModeLabel;
        private readonly ReactiveProperty<bool> _isCameraInvertVertical;
        private readonly ReactiveProperty<bool> _isCameraInvertHorizontal;
        private readonly ReactiveProperty<string> _autoLockOnLabel;
        private readonly ReactiveProperty<bool> _isAutoLockOnEnabled;
        private readonly ReactiveProperty<string> _buttonLayoutLabel;
        private readonly ReactiveProperty<bool> _isJapaneseButtonLayout;
    }
}
