using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Portrait イベントを出力処理へ橋渡しする。
    /// </summary>
    public class PortraitEventHandler : IScenarioEventHandler<PortraitEvent>
    {
        /// <summary>
        /// 立ち絵イベント用の依存関係を受け取る。
        /// </summary>
        public PortraitEventHandler(IPortraitOutputPort portraitOutputPort, IPortraitRepository portraitRepository)
        {
            _portraitOutputPort = portraitOutputPort;
            _portraitRepository = portraitRepository;
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
        public ValueTask HandleAsync(PortraitEvent e, CancellationToken ct)
        {
            if (!_portraitRepository.TryFindById(e.PortraitId, out PortraitDefinition portrait))
            {
                return default;
            }

            return _portraitOutputPort.ShowPortraitAsync(
                e.Slot.ToString(),
                portrait.AssetKey,
                e.PositionX,
                e.PositionY,
                e.Scale,
                e.Visible,
                ct);
        }

        private readonly IPortraitOutputPort _portraitOutputPort;
        private readonly IPortraitRepository _portraitRepository;
    }
}