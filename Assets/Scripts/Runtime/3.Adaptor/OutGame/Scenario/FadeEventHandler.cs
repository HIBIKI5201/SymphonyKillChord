using KillChord.Runtime.Application.OutGame.Scenario;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Fade イベントを出力処理へ橋渡しする。
    /// </summary>
    public class FadeEventHandler : IScenarioEventHandler<FadeEvent>
    {
        /// <summary>
        /// フェードイベント用の出力先を受け取る。
        /// </summary>
        public FadeEventHandler(IFadeOutputPort fadeOutputPort)
        {
            _fadeOutputPort = fadeOutputPort ?? throw new System.ArgumentNullException(nameof(fadeOutputPort));
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
        public async ValueTask HandleAsync(FadeEvent e, CancellationToken ct)
        {
            long startedAt = Stopwatch.GetTimestamp();
            await _fadeOutputPort.FadeAsync(
                e.Target,
                e.Mode,
                e.Start,
                e.End,
                e.DurationSec,
                ct);

            // View が演出を行えず即時完了した場合も、イベントの指定時間は維持する。
            double elapsedSec = (Stopwatch.GetTimestamp() - startedAt) / (double)Stopwatch.Frequency;
            double remainingSec = e.DurationSec - elapsedSec;
            if (remainingSec > 0d)
            {
                await Task.Delay(TimeSpan.FromSeconds(remainingSec), ct);
            }
        }

        private readonly IFadeOutputPort _fadeOutputPort;
    }
}
