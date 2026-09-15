using R3;

namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     UIへ環境設定を公開するViewModelインターフェース。
    /// </summary>
    public interface IEnvironmentSettingsViewModel
    {
        /// <summary> 解像度の表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> ResolutionLabel { get; }

        /// <summary> 画面モードの表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> ScreenModeLabel { get; }

        /// <summary> 画質プリセットの表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> QualityLevelLabel { get; }

        /// <summary> 画面の明るさ。 </summary>
        ReadOnlyReactiveProperty<int> Brightness { get; }

        /// <summary>
        ///     表示用DTOを環境設定へ反映する。
        /// </summary>
        /// <param name="dto"> 反映する環境設定の表示用DTO。 </param>
        void Apply(in EnvironmentSettingsViewDTO dto);
    }
}
