using KillChord.Runtime.View.SoundEffect;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class SoundEffectManager : MonoBehaviour
    {
        [SerializeField] private int _bufferCount;
        [SerializeField] private SoundEffectPlayer _sePrefab;
        private RecycleBuffer<SoundEffectPlayer> _recycleBuffer;

        public void Initialize()
        {
            SoundEffectPlayer[] players = new SoundEffectPlayer[_bufferCount];
            for (int i = 0; i < _bufferCount; i++)
            {
                players[i] = Instantiate(_sePrefab);
            }

            _recycleBuffer = new(players);
        }

        public void PlaySe(string cueName)
        {
            _recycleBuffer.Get().Play(cueName);
        }

        public void StopAllSe()
        {
            _recycleBuffer.RecycleAll();
        }
    }
}