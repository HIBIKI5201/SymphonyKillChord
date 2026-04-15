using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public interface IScenalioViewEventHandler
    {
        public ValueTask ExecuteAsync(IScenalioEvent scenalioEvent, IScenalioView view);
    }
}
