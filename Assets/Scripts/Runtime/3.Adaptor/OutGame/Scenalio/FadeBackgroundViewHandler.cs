using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class FadeBackgroundViewHandler : ScenalioViewEventHandlerBase<FadeBackgroundEvent>
    {
        protected override ValueTask ExecuteInternalAsync(FadeBackgroundEvent scenalioEvent, IScenalioView view)
        {
            return view.PLayFadeInAsync(scenalioEvent.Duration);
        }
    }
}
