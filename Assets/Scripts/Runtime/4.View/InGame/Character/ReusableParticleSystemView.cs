using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     再利用可能なParticleSystem再生Viewの基底クラスです。
    /// </summary>
    public abstract class ReusableParticleSystemView : MonoBehaviour
    {
        /// <summary>
        ///     ParticleSystemを再生します。
        /// </summary>
        public abstract void Play();

        /// <summary>
        ///     ParticleSystemを指定位置で再生します。
        /// </summary>
        /// <param name="position"> 再生する位置。 </param>
        public void PlayAt(Vector3 position)
        {
            _playbackPos = position;
            _hasPlaybackPos = true;

            try
            {
                Play();
            }
            finally
            {
                _hasPlaybackPos = false;
            }
        }

        /// <summary>
        ///     管理中のParticleSystemをすべて停止します。
        /// </summary>
        public abstract void StopAll();

        [SerializeField, Tooltip("再利用生成元として使うParticleSystemです。")]
        protected ParticleSystem _particleSystemTemplate;

        private Vector3 _playbackPos;
        private bool _hasPlaybackPos;

        /// <summary>
        ///     テンプレートが設定されているかです。
        /// </summary>
        protected bool HasTemplate => _particleSystemTemplate != null;

        /// <summary>
        ///     初期状態を整えます。
        /// </summary>
        protected virtual void Awake()
        {
            InitializeTemplate();
        }

        /// <summary>
        ///     無効化時に管理中ParticleSystemを停止します。
        /// </summary>
        protected virtual void OnDisable()
        {
            StopAll();
        }

        /// <summary>
        ///     テンプレートParticleSystemを初期状態へ戻します。
        /// </summary>
        protected void InitializeTemplate()
        {
            if (!HasTemplate)
            {
                return;
            }

            _particleSystemTemplate.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystemTemplate.gameObject.SetActive(false);
        }

        /// <summary>
        ///     再生前のParticleSystemをワールドへ展開します。
        /// </summary>
        /// <param name="particleSystem"> 対象ParticleSystemです。 </param>
        protected void PrepareInstanceForPlayback(ParticleSystem particleSystem)
        {
            if (particleSystem == null || !HasTemplate)
            {
                return;
            }

            particleSystem.gameObject.SetActive(true);
            Transform cacheTransform = particleSystem.transform;
            cacheTransform.SetParent(null, true);

            Vector3 position = _hasPlaybackPos
                ? _playbackPos
                : _particleSystemTemplate.transform.position;
            cacheTransform.SetPositionAndRotation(
                position,
                _particleSystemTemplate.transform.rotation);
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        ///     ParticleSystemを待機状態へ戻します。
        /// </summary>
        /// <param name="particleSystem"> 対象ParticleSystemです。 </param>
        protected void ResetInstanceToStorage(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
            {
                return;
            }

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // 親階層の無効化中はUnityがSetParentを許可しないため、再親子化を行いません。
            if (gameObject.activeInHierarchy)
            {
                particleSystem.transform.SetParent(transform, false);
            }

            particleSystem.gameObject.SetActive(false);
        }

        /// <summary>
        ///     複製用ParticleSystemを生成します。
        /// </summary>
        /// <returns> 生成したParticleSystemです。 </returns>
        protected ParticleSystem InstantiateTemplateInstance()
        {
            if (!HasTemplate)
            {
                return null;
            }

            ParticleSystem instance = Instantiate(_particleSystemTemplate, transform);
            ResetInstanceToStorage(instance);
            return instance;
        }
    }
}
