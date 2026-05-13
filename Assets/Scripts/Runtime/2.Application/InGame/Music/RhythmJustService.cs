using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Music
{
    public class RhythmJustService
    {
        public RhythmJustService()
        {
            Instance = this;
        }

        public static RhythmJustService Instance { get; private set; }
        public Action OnJustHit;

        public void Register(Action onJustHit)
        {
            OnJustHit += onJustHit;
        }

        public void Unregister(Action onJustHit)
        {
            OnJustHit -= onJustHit;
        }

        public void TriggerJustHit()
        {
            OnJustHit?.Invoke();
        }


    }
}
