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

        /// <summary> 表示言語の表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> LanguageLabel { get; }

        /// <summary> ゲームパッド振動の強さの表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> VibrationStrengthLabel { get; }

        /// <summary> ゲームパッド振動へ適用する強さの倍率。 </summary>
        ReadOnlyReactiveProperty<float> VibrationScale { get; }

        /// <summary> リズム判定オフセットの段階（0.05秒刻み、-6～6）。 </summary>
        ReadOnlyReactiveProperty<int> RhythmOffsetStep { get; }

        /// <summary> リズム判定オフセットの表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> RhythmOffsetLabel { get; }

        /// <summary> リズム判定タイミングへ加算するオフセット秒数。 </summary>
        ReadOnlyReactiveProperty<float> RhythmOffsetSeconds { get; }

        /// <summary>
        ///     表示用DTOを環境設定へ反映する。
        /// </summary>
        /// <param name="dto"> 反映する環境設定の表示用DTO。 </param>
        void Apply(in EnvironmentSettingsViewDTO dto);
    }
}
