using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public event Action OnJustHit;

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
            ChangeJustHit(true);
            OnJustHit?.Invoke();
        }
        public bool IsJustHit()
        {
            bool wasJustHit = _isJustHit;
            ChangeJustHit(false); // ジャストヒットの状態は一度確認されたらリセットする
            return wasJustHit;
        }
        public void ChangeJustHit(bool isJustHit)
        {
            _isJustHit = isJustHit;
        }

        public void Dispose()
        {

            if (Instance == this)
            {
                _instance = null;
            }
        }


        private bool _isJustHit;
        private static RhythmJustService _instance;

    }
}
