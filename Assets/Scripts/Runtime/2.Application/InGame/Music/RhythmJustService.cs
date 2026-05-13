using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Music
{
    public class RhythmJustService : IDisposable
    {

        public static RhythmJustService Instance
        {
            get
            {
                _instance ??= new RhythmJustService();
                return _instance;
            }
        }
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

        public void Dispose()
        {

            if (Instance == this)
            {
                _instance = null;
            }
        }

        private static RhythmJustService _instance;

    }
}
