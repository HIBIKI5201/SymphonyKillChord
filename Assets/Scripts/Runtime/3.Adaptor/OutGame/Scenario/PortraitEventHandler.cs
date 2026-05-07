using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class PortraitEventHandler : IScenarioEventHandler<PortraitEvent>
    {
        public PortraitEventHandler(IPortraitOutputPort portraitOutputPort, IPortraitRepository portraitRepository)
        {
            _portraitOutputPort = portraitOutputPort;
            _portraitRepository = portraitRepository;
        }

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
