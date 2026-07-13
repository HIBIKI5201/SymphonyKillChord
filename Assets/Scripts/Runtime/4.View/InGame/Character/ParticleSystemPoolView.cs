using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
     ///     ParticleSystemのワンショット再生をプールで管理するViewです。
     /// </summary>
    public sealed class ParticleSystemPoolView : ReusableParticleSystemView
    {
        /// <summary>
     ///     現在位置でParticleSystemを再生します。
     /// </summary>
        public override void Play()
        {
            if (!HasTemplate)
            {
                return;
            }

            EnsureInitialized();

            ParticleSystem particleSystem = _particleSystemPool.Get();
            PrepareInstanceForPlayback(particleSystem);
            particleSystem.Play();

            if (!_activeParticleSystems.Contains(particleSystem))
            {
                _activeParticleSystems.Add(particleSystem);
            }
        }

        /// <summary>
        ///     再生中のParticleSystemをすべて停止してプールへ戻します。
        /// </summary>
        public override void StopAll()
        {
            for (int i = _activeParticleSystems.Count - 1; i >= 0; i--)
            {
                ReleaseInstance(_activeParticleSystems[i]);
            }
        }

        [SerializeField, Min(1), Tooltip("プールの初期生成数です。")]
        private int _defaultCapacity = 4;

        [SerializeField, Min(1), Tooltip("プールの最大保持数です。")]
        private int _maxPoolSize = 16;

        private readonly List<ParticleSystem> _activeParticleSystems = new();
        private IObjectPool<ParticleSystem> _particleSystemPool;

        /// <summary>
        ///     停止済みParticleSystemを監視してプールへ戻します。
        /// </summary>
        private void Update()
        {
            ReleaseStoppedParticles();
        }

        /// <summary>
        ///     初期化時にプールを構築します。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            EnsureInitialized();
        }

        /// <summary>
        ///     破棄時にプール内容を解放します。
        /// </summary>
        private void OnDestroy()
        {
            StopAll();
            _particleSystemPool?.Clear();
        }

        /// <summary>
        ///     プールを必要時に初期化します。
        /// </summary>
        private void EnsureInitialized()
        {
            if (_particleSystemPool != null)
            {
                return;
            }

            _particleSystemPool = new ObjectPool<ParticleSystem>(
                CreateParticleSystem,
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPooledParticleSystem,
                true,
                _defaultCapacity,
                _maxPoolSize);
        }

        /// <summary>
        ///     新しいParticleSystemを生成します。
        /// </summary>
        /// <returns> 生成したParticleSystemです。 </returns>
        private ParticleSystem CreateParticleSystem()
        {
            return InstantiateTemplateInstance();
        }

        /// <summary>
        ///     プールから取り出したParticleSystemを有効化します。
        /// </summary>
        /// <param name="particleSystem"> 対象ParticleSystemです。 </param>
        private void OnGetFromPool(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
            {
                return;
            }

            particleSystem.gameObject.SetActive(true);
        }

        /// <summary>
        ///     プールへ戻すParticleSystemを初期状態へ戻します。
        /// </summary>
        /// <param name="particleSystem"> 対象ParticleSystemです。 </param>
        private void OnReleaseToPool(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
            {
                return;
            }

            ResetInstanceToStorage(particleSystem);
        }

        /// <summary>
        ///     不要になったParticleSystemを破棄します。
        /// </summary>
        /// <param name="particleSystem"> 対象ParticleSystemです。 </param>
        private void OnDestroyPooledParticleSystem(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
            {
                return;
            }

            Destroy(particleSystem.gameObject);
        }

        /// <summary>
        ///     停止済みParticleSystemを検出してプールへ戻します。
        /// </summary>
        private void ReleaseStoppedParticles()
        {
            for (int i = _activeParticleSystems.Count - 1; i >= 0; i--)
            {
                ParticleSystem particleSystem = _activeParticleSystems[i];
                if (particleSystem == null)
                {
                    _activeParticleSystems.RemoveAt(i);
                    continue;
                }

                if (particleSystem.IsAlive(true))
                {
                    continue;
                }

                ReleaseInstance(particleSystem);
            }
        }

        /// <summary>
        ///     指定ParticleSystemをプールへ戻します。
        /// </summary>
        /// <param name="particleSystem"> 対象ParticleSystemです。 </param>
        private void ReleaseInstance(ParticleSystem particleSystem)
        {
            if (particleSystem == null || _particleSystemPool == null)
            {
                return;
            }

            if (!_activeParticleSystems.Remove(particleSystem))
            {
                return;
            }

            _particleSystemPool.Release(particleSystem);
        }
    }
}
