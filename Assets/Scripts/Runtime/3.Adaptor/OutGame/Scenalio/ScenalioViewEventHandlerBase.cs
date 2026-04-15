using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public abstract class ScenalioViewEventHandlerBase<T> : IScenalioViewEventHandler where T : IScenalioEvent
    {
        public ValueTask ExecuteAsync(IScenalioEvent scenalioEvent, IScenalioView view)
        {
            return ExecuteInternalAsync((T)scenalioEvent, view);
        }

        protected abstract ValueTask ExecuteInternalAsync(T scenalioEvent, IScenalioView view);
    }
}
