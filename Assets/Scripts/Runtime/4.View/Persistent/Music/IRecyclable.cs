using UnityEngine;

namespace KillChord.Runtime.View.SoundEffect
{
    public interface IRecyclable
    { 
        int RecycleId { get; set; }
        void OnRecycle();
    }
}
