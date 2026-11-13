using System;

namespace KillChord.Runtime.Domain.Persistent.Experiment
{
    /// <summary>
    ///     「戦闘中にゲージばかり見る問題」の改善案のうち、どれを有効にするかを表す値オブジェクトです。
    ///     試遊でプランナーが案ごとに前後を比べるために使います。
    /// </summary>
    public readonly struct GaugeGazeExperimentSettings : IEquatable<GaugeGazeExperimentSettings>
    {
        /// <summary>
        ///     有効にする案を指定して初期化します。
        /// </summary>
        /// <param name="enabledFeatures"> 有効にする案です。一覧にないビットは無視します。 </param>
        public GaugeGazeExperimentSettings(GaugeGazeExperimentFeature enabledFeatures)
        {
            EnabledFeatures = enabledFeatures & ALL_FEATURES;
        }

        /// <summary> 有効にしている案です。 </summary>
        public GaugeGazeExperimentFeature EnabledFeatures { get; }

        /// <summary> 切り替えられる案の全てです。 </summary>
        public const GaugeGazeExperimentFeature ALL_FEATURES =
            GaugeGazeExperimentFeature.SkillJustBonus
            | GaugeGazeExperimentFeature.JustOnlyCombo
            | GaugeGazeExperimentFeature.SkillGaugeEmphasis
            | GaugeGazeExperimentFeature.GaugeWaveReduction
            | GaugeGazeExperimentFeature.MovementTuning
            | GaugeGazeExperimentFeature.OneBarGauge;

        /// <summary>
        ///     全ての案を有効にした設定を作ります。実験ブランチの既定値です。
        /// </summary>
        /// <returns> 作った設定です。 </returns>
        public static GaugeGazeExperimentSettings CreateAllEnabled()
        {
            return new GaugeGazeExperimentSettings(ALL_FEATURES);
        }

        /// <summary>
        ///     指定した案が有効か判定します。
        /// </summary>
        /// <param name="feature"> 判定する案です。 </param>
        /// <returns> 有効な場合はtrue。 </returns>
        public bool IsEnabled(GaugeGazeExperimentFeature feature)
        {
            return feature != GaugeGazeExperimentFeature.None && (EnabledFeatures & feature) == feature;
        }

        /// <summary>
        ///     指定した案の有効・無効を変えた設定を作ります。
        /// </summary>
        /// <param name="feature"> 変える案です。 </param>
        /// <param name="isEnabled"> 有効にする場合はtrue。 </param>
        /// <returns> 変えた後の設定です。 </returns>
        public GaugeGazeExperimentSettings WithFeature(GaugeGazeExperimentFeature feature, bool isEnabled)
        {
            GaugeGazeExperimentFeature features = isEnabled
                ? EnabledFeatures | feature
                : EnabledFeatures & ~feature;
            return new GaugeGazeExperimentSettings(features);
        }

        /// <summary>
        ///     設定が等しいか判定します。
        /// </summary>
        /// <param name="other"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public bool Equals(GaugeGazeExperimentSettings other)
        {
            return EnabledFeatures == other.EnabledFeatures;
        }

        /// <summary>
        ///     オブジェクトが等しいか判定します。
        /// </summary>
        /// <param name="obj"> 比較対象です。 </param>
        /// <returns> 等しい場合はtrue。 </returns>
        public override bool Equals(object obj)
        {
            return obj is GaugeGazeExperimentSettings other && Equals(other);
        }

        /// <summary>
        ///     ハッシュコードを取得します。
        /// </summary>
        /// <returns> ハッシュコードです。 </returns>
        public override int GetHashCode()
        {
            return (int)EnabledFeatures;
        }
    }
}
