using KillChord.Runtime.Domain.Persistent.Experiment;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.Experiment
{
    /// <summary>
    ///     ゲージ注視の改善案の切り替え設定を検証します。
    /// </summary>
    public sealed class GaugeGazeExperimentSettingsTests
    {
        /// <summary>
        ///     既定値では戦闘とゲージの案が有効で、見た目の案が無効であることを確認します。
        /// </summary>
        [Test]
        public void CreateDefault_EnablesGameplayFeaturesOnly()
        {
            GaugeGazeExperimentSettings settings = GaugeGazeExperimentSettings.CreateDefault();

            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.SkillJustBonus), Is.True);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.OneBarGauge), Is.True);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.CenteredGauge), Is.False);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.LargeRing), Is.False);
        }

        /// <summary>
        ///     リングの形を有効にすると、もう一方のリングが無効になることを確認します。
        /// </summary>
        [Test]
        public void WithFeature_RingShapesAreExclusive()
        {
            GaugeGazeExperimentSettings settings = GaugeGazeExperimentSettings.CreateDefault()
                .WithFeature(GaugeGazeExperimentFeature.LargeRing, true)
                .WithFeature(GaugeGazeExperimentFeature.ScreenEdgeRing, true);

            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.ScreenEdgeRing), Is.True);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.LargeRing), Is.False);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.SkillJustBonus), Is.True);
        }

        /// <summary>
        ///     バーの見た目の案は、ほかの案と一緒に有効にできることを確認します。
        /// </summary>
        [Test]
        public void WithFeature_BarLooksCanBeCombined()
        {
            GaugeGazeExperimentSettings settings = GaugeGazeExperimentSettings.CreateDefault()
                .WithFeature(GaugeGazeExperimentFeature.CenteredGauge, true)
                .WithFeature(GaugeGazeExperimentFeature.InwardGauge, true)
                .WithFeature(GaugeGazeExperimentFeature.TwentyFourStepGauge, true);

            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.CenteredGauge), Is.True);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.InwardGauge), Is.True);
            Assert.That(settings.IsEnabled(GaugeGazeExperimentFeature.TwentyFourStepGauge), Is.True);
        }
    }
}
