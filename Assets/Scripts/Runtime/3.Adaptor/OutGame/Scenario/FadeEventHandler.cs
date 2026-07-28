using KillChord.Runtime.Application.OutGame.Scenario;
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
        public ValueTask HandleAsync(FadeEvent e, CancellationToken ct)
        {
            return _fadeOutputPort.FadeAsync(e.Start, e.End, e.DurationSec, ct);
        }

        private readonly IFadeOutputPort _fadeOutputPort;
    }
}