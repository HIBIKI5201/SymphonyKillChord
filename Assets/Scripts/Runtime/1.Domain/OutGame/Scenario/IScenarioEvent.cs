using System.Threading;
using System.Threading.Tasks;
using Codice.Client.BaseCommands.Differences;

namespace KillChord.Runtime.Domain
{
    public interface IScenarioEvent
    {
        public bool RequirePlayerAdvance { get; }
    }
}
