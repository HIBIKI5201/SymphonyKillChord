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

        /// <summary> カメラ感度（1～10）。 </summary>
        ReadOnlyReactiveProperty<int> CameraSensitivity { get; }

        /// <summary> カメラ入力へ掛ける感度の倍率。 </summary>
        ReadOnlyReactiveProperty<float> CameraSensitivityScale { get; }

        /// <summary> カメラ操作の反転方向の表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> CameraInvertModeLabel { get; }

        /// <summary> カメラの上下操作を反転するかどうか。 </summary>
        ReadOnlyReactiveProperty<bool> IsCameraInvertVertical { get; }

        /// <summary> カメラの左右操作を反転するかどうか。 </summary>
        ReadOnlyReactiveProperty<bool> IsCameraInvertHorizontal { get; }

        /// <summary> オートロックオンの表示ラベル。 </summary>
        ReadOnlyReactiveProperty<string> AutoLockOnLabel { get; }

        /// <summary> 攻撃時のオートロックオンを使うかどうか。 </summary>
        ReadOnlyReactiveProperty<bool> IsAutoLockOnEnabled { get; }

        /// <summary>
        ///     表示用DTOを環境設定へ反映する。
        /// </summary>
        /// <param name="dto"> 反映する環境設定の表示用DTO。 </param>
        void Apply(in EnvironmentSettingsViewDTO dto);
    }
}
