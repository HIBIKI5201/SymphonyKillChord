using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     解像度、画面モード、画質プリセット、明るさの環境設定を保持するセーブデータ。
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
        public EnvironmentSettingsData(
            int resolutionWidth = DEFAULT_RESOLUTION_WIDTH,
            int resolutionHeight = DEFAULT_RESOLUTION_HEIGHT,
            bool isFullScreen = DEFAULT_IS_FULL_SCREEN,
            int qualityLevel = DEFAULT_QUALITY_LEVEL,
            int brightness = DEFAULT_BRIGHTNESS)
        {
            SetResolution(resolutionWidth, resolutionHeight, isFullScreen);
            SetQualityLevel(qualityLevel);
            SetBrightness(brightness);
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

        public const int MIN_BRIGHTNESS = 0;
        public const int MAX_BRIGHTNESS = 10;

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
        ///     現在値の複製を作成する。
        /// </summary>
        public EnvironmentSettingsData Copy()
        {
            return new EnvironmentSettingsData(
                ResolutionWidth,
                ResolutionHeight,
                IsFullScreen,
                QualityLevel,
                Brightness);
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

        /// <summary>
        ///     明るさを有効範囲へ制限する。
        /// </summary>
        private static int Clamp(int brightness)
        {
            return Mathf.Clamp(brightness, MIN_BRIGHTNESS, MAX_BRIGHTNESS);
        }
    }
}
