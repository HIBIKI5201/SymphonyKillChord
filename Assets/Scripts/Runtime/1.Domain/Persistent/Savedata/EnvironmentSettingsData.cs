using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     解像度、画面モード、画質プリセット、明るさ、言語、振動、カメラ操作の環境設定を保持するセーブデータ。
    /// </summary>
    [Serializable]
    public sealed class EnvironmentSettingsData
    {
        /// <summary>
        ///     環境設定を初期化する。
        /// </summary>
        /// <param name="resolutionWidth"> 解像度の幅。 </param>
        /// <param name="resolutionHeight"> 解像度の高さ。 </param>
        /// <param name="isFullScreen"> フルスクリーンかどうか。 </param>
        /// <param name="qualityLevel"> 画質プリセットのレベル。 </param>
        /// <param name="brightness"> 画面の明るさ。 </param>
        /// <param name="language"> 表示言語。 </param>
        /// <param name="vibrationStrength"> ゲームパッド振動の強さ。 </param>
        /// <param name="rhythmOffsetSeconds"> リズム判定タイミングへ加算するオフセット秒数。 </param>
        /// <param name="cameraSensitivity"> カメラ感度（1～10）。 </param>
        /// <param name="cameraInvertMode"> カメラ操作の反転方向。 </param>
        /// <param name="isAutoLockOnEnabled"> 攻撃時のオートロックオンを使うかどうか。 </param>
        /// <param name="isJapaneseButtonLayout"> ゲームパッドの決定・キャンセルを日本式（決定=右ボタン）にするかどうか。 </param>
        public EnvironmentSettingsData(
            int resolutionWidth = DEFAULT_RESOLUTION_WIDTH,
            int resolutionHeight = DEFAULT_RESOLUTION_HEIGHT,
            bool isFullScreen = DEFAULT_IS_FULL_SCREEN,
            int qualityLevel = DEFAULT_QUALITY_LEVEL,
            int brightness = DEFAULT_BRIGHTNESS,
            GameLanguage language = DEFAULT_LANGUAGE,
            VibrationStrength vibrationStrength = DEFAULT_VIBRATION_STRENGTH,
            double rhythmOffsetSeconds = DEFAULT_RHYTHM_OFFSET_SECONDS,
            int cameraSensitivity = DEFAULT_CAMERA_SENSITIVITY,
            CameraInvertMode cameraInvertMode = DEFAULT_CAMERA_INVERT_MODE,
            bool isAutoLockOnEnabled = DEFAULT_IS_AUTO_LOCK_ON_ENABLED,
            bool isJapaneseButtonLayout = DEFAULT_IS_JAPANESE_BUTTON_LAYOUT)
        {
            SetResolution(resolutionWidth, resolutionHeight, isFullScreen);
            SetQualityLevel(qualityLevel);
            SetBrightness(brightness);
            SetLanguage(language);
            SetVibrationStrength(vibrationStrength);
            SetRhythmOffsetSeconds(rhythmOffsetSeconds);
            SetCameraSensitivity(cameraSensitivity);
            SetCameraInvertMode(cameraInvertMode);
            SetAutoLockOnEnabled(isAutoLockOnEnabled);
            SetJapaneseButtonLayout(isJapaneseButtonLayout);
        }

        /// <summary> 解像度の幅。 </summary>
        public int ResolutionWidth => _resolutionWidth;

        /// <summary> 解像度の高さ。 </summary>
        public int ResolutionHeight => _resolutionHeight;

        /// <summary> フルスクリーンかどうか。 </summary>
        public bool IsFullScreen => _isFullScreen;

        /// <summary> 画質プリセットのレベル。 </summary>
        public int QualityLevel => _qualityLevel;

        /// <summary> 画面の明るさ。 </summary>
        public int Brightness => Mathf.Clamp(_brightness, MIN_BRIGHTNESS, MAX_BRIGHTNESS);

        /// <summary> 表示言語。 </summary>
        public GameLanguage Language => _language;

        /// <summary> ゲームパッド振動の強さ。 </summary>
        public VibrationStrength VibrationStrength => _vibrationStrength;

        /// <summary> リズム判定タイミングへ加算するオフセット秒数。 </summary>
        public double RhythmOffsetSeconds => _rhythmOffsetSeconds;

        /// <summary>
        ///     カメラ感度（1～10）。
        ///     項目追加前のセーブデータでは0になるため、その場合は既定値として扱う。
        /// </summary>
        public int CameraSensitivity => _cameraSensitivity <= 0
            ? DEFAULT_CAMERA_SENSITIVITY
            : Mathf.Clamp(_cameraSensitivity, MIN_CAMERA_SENSITIVITY, MAX_CAMERA_SENSITIVITY);

        /// <summary> カメラ操作の反転方向。 </summary>
        public CameraInvertMode CameraInvertMode => _cameraInvertMode;

        /// <summary> 攻撃時のオートロックオンを使うかどうか。 </summary>
        public bool IsAutoLockOnEnabled => !_isAutoLockOnDisabled;

        /// <summary>
        ///     ゲームパッドの決定・キャンセルが日本式（決定=右ボタン、キャンセル=下ボタン）かどうか。
        ///     falseの場合は海外式（決定=下ボタン、キャンセル=右ボタン）。
        /// </summary>
        public bool IsJapaneseButtonLayout => _isJapaneseButtonLayout;

        public const int MIN_BRIGHTNESS = 0;
        public const int MAX_BRIGHTNESS = 10;

        /// <summary> リズム判定オフセットの最小値（秒）。 </summary>
        public const double MIN_RHYTHM_OFFSET_SECONDS = -0.30d;

        /// <summary> リズム判定オフセットの最大値（秒）。 </summary>
        public const double MAX_RHYTHM_OFFSET_SECONDS = 0.30d;

        /// <summary> リズム判定オフセットの刻み幅（秒）。 </summary>
        public const double RHYTHM_OFFSET_STEP_SECONDS = 0.05d;

        /// <summary> リズム判定オフセットの既定値（秒）。 </summary>
        public const double DEFAULT_RHYTHM_OFFSET_SECONDS = 0d;

        /// <summary> リズム判定オフセットの最小段階（0.05秒刻み）。 </summary>
        public const int MIN_RHYTHM_OFFSET_STEP = -6;

        /// <summary> リズム判定オフセットの最大段階（0.05秒刻み）。 </summary>
        public const int MAX_RHYTHM_OFFSET_STEP = 6;

        /// <summary> カメラ感度の最小値。 </summary>
        public const int MIN_CAMERA_SENSITIVITY = 1;

        /// <summary> カメラ感度の最大値。 </summary>
        public const int MAX_CAMERA_SENSITIVITY = 10;

        /// <summary> カメラ感度の既定値。この値で倍率1倍になる。 </summary>
        public const int DEFAULT_CAMERA_SENSITIVITY = 5;

        /// <summary> カメラ操作の反転方向の既定値。 </summary>
        public const CameraInvertMode DEFAULT_CAMERA_INVERT_MODE = CameraInvertMode.None;

        /// <summary> オートロックオンの既定値。 </summary>
        public const bool DEFAULT_IS_AUTO_LOCK_ON_ENABLED = true;

        /// <summary> ゲームパッドの決定・キャンセルの既定値。海外式（決定=下ボタン）。 </summary>
        public const bool DEFAULT_IS_JAPANESE_BUTTON_LAYOUT = false;

        /// <summary> 明るさの既定値。 </summary>
        public const int DEFAULT_BRIGHTNESS = 5;

        /// <summary> 解像度の幅の既定値。 </summary>
        public const int DEFAULT_RESOLUTION_WIDTH = 1920;

        /// <summary> 解像度の高さの既定値。 </summary>
        public const int DEFAULT_RESOLUTION_HEIGHT = 1080;

        /// <summary> フルスクリーンの既定値。 </summary>
        public const bool DEFAULT_IS_FULL_SCREEN = true;

        /// <summary> 画質プリセットの既定値。 </summary>
        public const int DEFAULT_QUALITY_LEVEL = 3;

        /// <summary> 表示言語の既定値。 </summary>
        public const GameLanguage DEFAULT_LANGUAGE = GameLanguage.Japanese;

        /// <summary> ゲームパッド振動の既定値。 </summary>
        public const VibrationStrength DEFAULT_VIBRATION_STRENGTH = VibrationStrength.Strong;

        /// <summary>
        ///     解像度と画面モードを設定する。
        /// </summary>
        public void SetResolution(int width, int height, bool isFullScreen)
        {
            _resolutionWidth = Mathf.Max(1, width);
            _resolutionHeight = Mathf.Max(1, height);
            _isFullScreen = isFullScreen;
        }

        /// <summary>
        ///     画質プリセットのレベルを設定する。
        /// </summary>
        public void SetQualityLevel(int qualityLevel)
        {
            _qualityLevel = Mathf.Max(0, qualityLevel);
        }

        /// <summary>
        ///     画面の明るさを設定する。
        /// </summary>
        public void SetBrightness(int brightness)
        {
            _brightness = Clamp(brightness);
        }

        /// <summary>
        ///     表示言語を設定する。
        /// </summary>
        public void SetLanguage(GameLanguage language)
        {
            _language = Enum.IsDefined(typeof(GameLanguage), language)
                ? language
                : DEFAULT_LANGUAGE;
        }

        /// <summary>
        ///     ゲームパッド振動の強さを設定する。
        /// </summary>
        public void SetVibrationStrength(VibrationStrength vibrationStrength)
        {
            _vibrationStrength = Enum.IsDefined(typeof(VibrationStrength), vibrationStrength)
                ? vibrationStrength
                : DEFAULT_VIBRATION_STRENGTH;
        }

        /// <summary>
        ///     リズム判定タイミングへ加算するオフセット秒数を設定する。
        ///     範囲外の値は最小・最大値へ丸め、0.05秒刻みへスナップする。
        /// </summary>
        public void SetRhythmOffsetSeconds(double rhythmOffsetSeconds)
        {
            _rhythmOffsetSeconds = ClampRhythmOffsetSeconds(rhythmOffsetSeconds);
        }

        /// <summary>
        ///     カメラ感度を設定する。範囲外の値は最小・最大値へ丸める。
        /// </summary>
        public void SetCameraSensitivity(int cameraSensitivity)
        {
            _cameraSensitivity = Mathf.Clamp(cameraSensitivity, MIN_CAMERA_SENSITIVITY, MAX_CAMERA_SENSITIVITY);
        }

        /// <summary>
        ///     カメラ操作の反転方向を設定する。
        /// </summary>
        public void SetCameraInvertMode(CameraInvertMode cameraInvertMode)
        {
            _cameraInvertMode = Enum.IsDefined(typeof(CameraInvertMode), cameraInvertMode)
                ? cameraInvertMode
                : DEFAULT_CAMERA_INVERT_MODE;
        }

        /// <summary>
        ///     攻撃時のオートロックオンを使うかどうかを設定する。
        /// </summary>
        public void SetAutoLockOnEnabled(bool isAutoLockOnEnabled)
        {
            _isAutoLockOnDisabled = !isAutoLockOnEnabled;
        }

        /// <summary>
        ///     ゲームパッドの決定・キャンセルを日本式にするかどうかを設定する。
        /// </summary>
        public void SetJapaneseButtonLayout(bool isJapaneseButtonLayout)
        {
            _isJapaneseButtonLayout = isJapaneseButtonLayout;
        }

        /// <summary>
        ///     現在値の複製を作成する。
        /// </summary>
        public EnvironmentSettingsData Copy()
        {
            return new EnvironmentSettingsData(
                ResolutionWidth,
                ResolutionHeight,
                IsFullScreen,
                QualityLevel,
                Brightness,
                Language,
                VibrationStrength,
                RhythmOffsetSeconds,
                CameraSensitivity,
                CameraInvertMode,
                IsAutoLockOnEnabled,
                IsJapaneseButtonLayout);
        }

        [SerializeField, Tooltip("解像度の幅")]
        private int _resolutionWidth = DEFAULT_RESOLUTION_WIDTH;

        [SerializeField, Tooltip("解像度の高さ")]
        private int _resolutionHeight = DEFAULT_RESOLUTION_HEIGHT;

        [SerializeField, Tooltip("フルスクリーンかどうか")]
        private bool _isFullScreen = DEFAULT_IS_FULL_SCREEN;

        [SerializeField, Tooltip("画質プリセットのレベル")]
        private int _qualityLevel = DEFAULT_QUALITY_LEVEL;

        [SerializeField, Tooltip("画面の明るさ（0～10）")]
        private int _brightness = DEFAULT_BRIGHTNESS;

        [SerializeField, Tooltip("表示言語")]
        private GameLanguage _language = DEFAULT_LANGUAGE;

        [SerializeField, Tooltip("ゲームパッド振動の強さ")]
        private VibrationStrength _vibrationStrength = DEFAULT_VIBRATION_STRENGTH;

        [SerializeField, Tooltip("リズム判定タイミングへ加算するオフセット秒数（±0.30秒、0.05秒刻み）")]
        private double _rhythmOffsetSeconds = DEFAULT_RHYTHM_OFFSET_SECONDS;

        [SerializeField, Tooltip("カメラ感度（1～10）。0は項目追加前のセーブデータで、既定値として扱う")]
        private int _cameraSensitivity = DEFAULT_CAMERA_SENSITIVITY;

        [SerializeField, Tooltip("カメラ操作の反転方向")]
        private CameraInvertMode _cameraInvertMode = DEFAULT_CAMERA_INVERT_MODE;

        // 項目追加前のセーブデータでもオンになるよう、無効側をbool値で保持する。
        [SerializeField, Tooltip("攻撃時のオートロックオンを使わないかどうか")]
        private bool _isAutoLockOnDisabled = !DEFAULT_IS_AUTO_LOCK_ON_ENABLED;

        // 項目追加前のセーブデータでは既定値の海外式（false）になる。
        [SerializeField, Tooltip("ゲームパッドの決定・キャンセルを日本式（決定=右ボタン）にするかどうか")]
        private bool _isJapaneseButtonLayout = DEFAULT_IS_JAPANESE_BUTTON_LAYOUT;

        /// <summary>
        ///     明るさを有効範囲へ制限する。
        /// </summary>
        private static int Clamp(int brightness)
        {
            return Mathf.Clamp(brightness, MIN_BRIGHTNESS, MAX_BRIGHTNESS);
        }

        /// <summary>
        ///     リズム判定オフセットを有効範囲へ制限し、0.05秒刻みへスナップする。
        /// </summary>
        private static double ClampRhythmOffsetSeconds(double rhythmOffsetSeconds)
        {
            if (double.IsNaN(rhythmOffsetSeconds) || double.IsInfinity(rhythmOffsetSeconds))
            {
                return DEFAULT_RHYTHM_OFFSET_SECONDS;
            }

            double clamped = System.Math.Clamp(
                rhythmOffsetSeconds,
                MIN_RHYTHM_OFFSET_SECONDS,
                MAX_RHYTHM_OFFSET_SECONDS);
            double snappedSteps = System.Math.Round(clamped / RHYTHM_OFFSET_STEP_SECONDS);
            return System.Math.Clamp(
                snappedSteps * RHYTHM_OFFSET_STEP_SECONDS,
                MIN_RHYTHM_OFFSET_SECONDS,
                MAX_RHYTHM_OFFSET_SECONDS);
        }
    }
}
