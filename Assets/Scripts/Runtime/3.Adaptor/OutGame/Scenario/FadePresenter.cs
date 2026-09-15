using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class FadePresenter : IFadeOutputPort
    {
        /// <summary>
        /// フェード表示の出力先を受け取る。
        /// </summary>
        public FadePresenter(IFadeViewSink viewSink)
        {
            _viewSink = viewSink ?? throw new System.ArgumentNullException(nameof(viewSink));
        }

        /// <summary>
        /// フェード演出要求をビューへ通知する。
        /// </summary>
        public ValueTask FadeAsync(
            FadeTarget target,
            FadeMode mode,
            float start,
            float end,
            float duration,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var dto = new ScenarioFadeViewDTO(
                ConvertTarget(target),
                ConvertMode(mode),
                start,
                end,
                duration);
            return _viewSink.SetFadeAsync(in dto, ct);
        }

        /// <summary>
        /// Domain の対象を View 向け対象へ変換する。
        /// </summary>
        private static ScenarioFadeTarget ConvertTarget(FadeTarget target)
        {
            return target switch
            {
                FadeTarget.Screen => ScenarioFadeTarget.Screen,
                FadeTarget.Background => ScenarioFadeTarget.Background,
                FadeTarget.PortraitLeft => ScenarioFadeTarget.PortraitLeft,
                FadeTarget.PortraitCenter => ScenarioFadeTarget.PortraitCenter,
                FadeTarget.PortraitRight => ScenarioFadeTarget.PortraitRight,
                FadeTarget.Text => ScenarioFadeTarget.Text,
                FadeTarget.Black => ScenarioFadeTarget.Black,
                _ => throw new System.ArgumentOutOfRangeException(nameof(target), target, null),
            };
        }

        /// <summary>
        /// Domain の表示チャネルを View 向け表示チャネルへ変換する。
        /// </summary>
        private static ScenarioFadeMode ConvertMode(FadeMode mode)
        {
            return mode switch
            {
                FadeMode.Alpha => ScenarioFadeMode.Alpha,
                FadeMode.Black => ScenarioFadeMode.Black,
                _ => throw new System.ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
        }

        private readonly IFadeViewSink _viewSink;
    }
}
