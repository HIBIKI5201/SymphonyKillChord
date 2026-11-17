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

        /// <summary> 議事録の値で試す、戦闘とゲージの案です。実験ブランチでは最初から有効にします。 </summary>
        public const GaugeGazeExperimentFeature DEFAULT_FEATURES =
            GaugeGazeExperimentFeature.SkillJustBonus
            | GaugeGazeExperimentFeature.JustOnlyCombo
            | GaugeGazeExperimentFeature.SkillGaugeEmphasis
            | GaugeGazeExperimentFeature.GaugeWaveReduction
            | GaugeGazeExperimentFeature.MovementTuning
            | GaugeGazeExperimentFeature.OneBarGauge;

        /// <summary> リングの形の案です。どちらか1つだけを有効にできます。 </summary>
        public const GaugeGazeExperimentFeature RING_FEATURES =
            GaugeGazeExperimentFeature.LargeRing
            | GaugeGazeExperimentFeature.ScreenEdgeRing;

        /// <summary> リズムUIの見た目の案です。見比べるときだけ有効にするため、最初は無効にします。 </summary>
        public const GaugeGazeExperimentFeature LOOK_FEATURES =
            GaugeGazeExperimentFeature.CenteredGauge
            | GaugeGazeExperimentFeature.InwardGauge
            | GaugeGazeExperimentFeature.TwentyFourStepGauge
            | GaugeGazeExperimentFeature.JustBarDesign
            | RING_FEATURES;

        /// <summary> 切り替えられる案の全てです。 </summary>
        public const GaugeGazeExperimentFeature ALL_FEATURES = DEFAULT_FEATURES | LOOK_FEATURES;

        /// <summary>
        ///     戦闘とゲージの案を全て有効にし、見た目の案を無効にした設定を作ります。実験ブランチの既定値です。
        /// </summary>
        /// <returns> 作った設定です。 </returns>
        public static GaugeGazeExperimentSettings CreateDefault()
        {
            return new GaugeGazeExperimentSettings(DEFAULT_FEATURES);
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
        ///     リングの形の案を有効にしたときは、もう一方のリングを無効にします。
        /// </summary>
        /// <param name="feature"> 変える案です。 </param>
        /// <param name="isEnabled"> 有効にする場合はtrue。 </param>
        /// <returns> 変えた後の設定です。 </returns>
        public GaugeGazeExperimentSettings WithFeature(GaugeGazeExperimentFeature feature, bool isEnabled)
        {
            GaugeGazeExperimentFeature features = EnabledFeatures;
            if (isEnabled && (feature & RING_FEATURES) != GaugeGazeExperimentFeature.None)
            {
                // リングの形は同時に1つしか出せないため、もう一方を無効にする。
                features &= ~RING_FEATURES;
            }

            features = isEnabled ? features | feature : features & ~feature;
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
