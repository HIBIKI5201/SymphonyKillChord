using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     ParticleSystemのワンショット再生をRingBufferで管理するViewです。
    /// </summary>
    public sealed class ParticleSystemRingBufferView : ReusableParticleSystemView
    {
        /// <summary>
        ///     RingBufferからParticleSystemを取り出して再生します。
        /// </summary>
        public override void Play()
        {
            if (!HasTemplate)
            {
                return;
            }

            EnsureInitialized();

            if (_particleSystems == null || _particleSystems.Length == 0)
            {
                return;
            }

            ParticleSystem particleSystem = _particleSystems[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _particleSystems.Length;

            PrepareInstanceForPlayback(particleSystem);
            particleSystem.transform.position = transform.position;
            particleSystem.Play();
        }

        /// <summary>
        ///     RingBuffer内のParticleSystemをすべて停止します。
        /// </summary>
        public override void StopAll()
        {
            if (_particleSystems == null)
            {
                return;
            }

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ResetInstanceToStorage(_particleSystems[i]);
            }
        }

        [SerializeField, Min(1), Tooltip("先行生成するParticleSystem数です。")]
        private int _bufferSize = 8;

        private ParticleSystem[] _particleSystems;
        private int _nextIndex;

        /// <summary>
        ///     初期化時にRingBufferを構築します。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            EnsureInitialized();
        }

        /// <summary>
        ///     破棄時に生成済みParticleSystemを破棄します。
        /// </summary>
        private void OnDestroy()
        {
            if (_particleSystems == null)
            {
                return;
            }

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] == null)
                {
                    continue;
                }

                Destroy(_particleSystems[i].gameObject);
            }
        }

        /// <summary>
        ///     RingBufferを必要時に構築します。
        /// </summary>
        private void EnsureInitialized()
        {
            if (_particleSystems != null || !HasTemplate)
            {
                return;
            }

            int bufferSize = Mathf.Max(1, _bufferSize);
            _particleSystems = new ParticleSystem[bufferSize];

            for (int i = 0; i < bufferSize; i++)
            {
                _particleSystems[i] = InstantiateTemplateInstance();
            }

            _nextIndex = 0;
        }
    }
}
