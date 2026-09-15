using KillChord.Runtime.Domain.Persistent.Savedata;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータが存在しない初回起動時に適用する環境設定の既定値を保持するアセット。
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnvironmentSettingsDefaultAsset",
        menuName = "KillChord/InfraStructure/Persistent/Environment Settings Default")]
    public sealed class EnvironmentSettingsDefaultAsset : ScriptableObject
    {
        /// <summary> 初回起動時に適用する解像度の幅。 </summary>
        public int ResolutionWidth => _resolutionWidth;

        /// <summary> 初回起動時に適用する解像度の高さ。 </summary>
        public int ResolutionHeight => _resolutionHeight;

        /// <summary> 初回起動時にフルスクリーンにするかどうか。 </summary>
        public bool IsFullScreen => _isFullScreen;

        /// <summary> 初回起動時に適用する画質プリセットのレベル。 </summary>
        public int QualityLevel => _qualityLevel;

        /// <summary> 初回起動時に適用する画面の明るさ。 </summary>
        public int Brightness => _brightness;

        /// <summary>
        ///     初回起動時に適用する環境設定のDomainデータへ変換する。
        /// </summary>
        public EnvironmentSettingsData ToEnvironmentSettingsData()
        {
            return new EnvironmentSettingsData(
                _resolutionWidth,
                _resolutionHeight,
                _isFullScreen,
                _qualityLevel,
                _brightness);
        }

        [SerializeField, Tooltip("初回起動時に適用する解像度の幅")]
        private int _resolutionWidth = EnvironmentSettingsData.DEFAULT_RESOLUTION_WIDTH;

        [SerializeField, Tooltip("初回起動時に適用する解像度の高さ")]
        private int _resolutionHeight = EnvironmentSettingsData.DEFAULT_RESOLUTION_HEIGHT;

        [SerializeField, Tooltip("初回起動時にフルスクリーンにするかどうか")]
        private bool _isFullScreen = EnvironmentSettingsData.DEFAULT_IS_FULL_SCREEN;

        [SerializeField, Tooltip("初回起動時に適用する画質プリセットのレベル")]
        private int _qualityLevel = EnvironmentSettingsData.DEFAULT_QUALITY_LEVEL;

        [SerializeField, Tooltip("初回起動時に適用する画面の明るさ（0～10）")]
        private int _brightness = EnvironmentSettingsData.DEFAULT_BRIGHTNESS;
    }
}
