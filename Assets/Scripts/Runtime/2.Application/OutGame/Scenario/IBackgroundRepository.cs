using UnityEngine;

namespace KillChord.Runtime.Application
{
    public interface IBackgroundRepository
    {
        bool TryFindById(string id, out Sprite background);
    }
}
