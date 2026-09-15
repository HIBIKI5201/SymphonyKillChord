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
        }

        /// <summary> 解像度の表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> ResolutionLabel => _resolutionLabel;

        /// <summary> 画面モードの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> ScreenModeLabel => _screenModeLabel;

        /// <summary> 画質プリセットの表示ラベル。 </summary>
        public ReadOnlyReactiveProperty<string> QualityLevelLabel => _qualityLevelLabel;

        /// <summary> 画面の明るさ。 </summary>
        public ReadOnlyReactiveProperty<int> Brightness => _brightness;

        /// <summary>
        ///     表示用DTOを環境設定へ反映する。
        /// </summary>
        public void Apply(in EnvironmentSettingsViewDTO dto)
        {
            _resolutionLabel.Value = dto.ResolutionLabel;
            _screenModeLabel.Value = dto.ScreenModeLabel;
            _qualityLevelLabel.Value = dto.QualityLevelLabel;
            _brightness.Value = dto.Brightness;
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
        }

        private readonly ReactiveProperty<string> _resolutionLabel;
        private readonly ReactiveProperty<string> _screenModeLabel;
        private readonly ReactiveProperty<string> _qualityLevelLabel;
        private readonly ReactiveProperty<int> _brightness;
    }
}
