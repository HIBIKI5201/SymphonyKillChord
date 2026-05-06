using CriWare;
using KillChord.Runtime.View.SoundEffect;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class SoundEffectManager : MonoBehaviour
    {
        private RecycleBuffer<SoundEffectPlayer> _recycleBuffer;
    }
}
