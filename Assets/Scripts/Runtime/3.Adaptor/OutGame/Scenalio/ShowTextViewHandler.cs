using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class ShowTextViewHandler : ScenalioViewEventHandlerBase<ShowTextEvent>
    {
        protected override ValueTask ExecuteInternalAsync(ShowTextEvent scenalioEvent, IScenalioView view)
        {
            return view.PlayTypewriterAsync(scenalioEvent.Message);
        }
    }
}
