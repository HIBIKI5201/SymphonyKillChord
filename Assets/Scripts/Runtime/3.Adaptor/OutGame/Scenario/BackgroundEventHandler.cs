using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Background イベントを出力処理へ橋渡しする。
    /// </summary>
    public class BackgroundEventHandler : IScenarioEventHandler<BackgroundEvent>
    {
        /// <summary>
        /// 背景イベント用の依存関係を受け取る。
        /// </summary>
        public BackgroundEventHandler(IBackgroundOutputPort backgroundOutputPort, IBackgroundRepository backgroundRepository)
        {
            _backgroundOutputPort = backgroundOutputPort;
            _backgroundRepository = backgroundRepository;
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
        public async ValueTask HandleAsync(BackgroundEvent e, CancellationToken ct)
        {
            if (!_backgroundRepository.TryFindById(e.BackgroundId, out BackgroundDefinition background))
            {
                return;
            }

            await _backgroundOutputPort.ShowBackgroundAsync(background.AssetKey, ct);
        }

        private readonly IBackgroundOutputPort _backgroundOutputPort;
        private readonly IBackgroundRepository _backgroundRepository;
    }
}