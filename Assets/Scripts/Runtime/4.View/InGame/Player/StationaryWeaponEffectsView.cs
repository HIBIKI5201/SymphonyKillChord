using CriWare;
using KillChord.Runtime.View.Persistent.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     事前生成した発射演出をワールド位置で再生し、終了した実体を再利用します。
    /// </summary>
    public sealed class StationaryWeaponEffectsView : MonoBehaviour
    {
        /// <summary>
        ///     武器の演出を複製元とし、発射前に全スロットの粒子・音源・ライトを初期化します。
        /// </summary>
        public void Initialize(SoundEffectSource sound, ParticleSystem particle, MuzzleFlashLight flash)
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _soundTemplate = sound;
            _particleTemplate = particle;
            _flashTemplate = flash;
            if (_soundTemplate == null && _particleTemplate == null && _flashTemplate == null)
            {
                return;
            }

            if (_particleTemplate != null)
            {
                _particleTemplate.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                foreach (ParticleSystem particleSystem in _particleTemplate.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ParticleSystem.MainModule main = particleSystem.main;
                    main.playOnAwake = false;
                }
            }

            for (int i = 0; i < _shots.Length; i++)
            {
                _shots[i] = CreateShot();
            }
        }

        /// <summary>
        ///     SEを拍に合わせて再生し、粒子とライトは武器ごとの遅延後に現在の銃口位置で再生します。
        /// </summary>
        /// <param name="effectDelaySeconds"> 粒子とライトの再生までの遅延秒数です。 </param>
        public void Play(float effectDelaySeconds)
        {
            if (!isActiveAndEnabled || !_isInitialized
                || (_soundTemplate == null && _particleTemplate == null && _flashTemplate == null))
            {
                return;
            }

            Shot shot = FindAvailableShot();
            if (shot == null)
            {
                if (!_hasReportedPoolExhaustion)
                {
                    _hasReportedPoolExhaustion = true;
                    Debug.LogWarning(
                        $"[{nameof(StationaryWeaponEffectsView)}] 発射演出の全スロットを使用中のため、今回の演出を省略します。", this);
                }
                return;
            }

            CancelPendingEffect();
            if (shot.Root.transform.parent != null)
            {
                // 初回発射まではPlayerのシーン移動に同伴し、以後はワールドに固定する。
                // local identityを維持して切り離し、CopyPoseで拡縮を二重適用しない。
                shot.Root.transform.SetParent(null, false);
            }
            shot.IsActive = true;
            shot.StartTime = Time.time;
            shot.StartFrame = Time.frameCount;
            shot.EffectTime = shot.StartTime + Mathf.Max(0f, effectDelaySeconds);
            shot.IsEffectPending = shot.Particle != null || shot.Flash != null;
            CopyPose(_soundTemplate, shot.Sound);
            _lastShot = shot;

            if (shot.Sound != null)
            {
                ResetAudioPosition(shot.Audio);
                shot.Sound.Play();
            }
        }

        /// <summary>
        ///     最新の発射でまだ開始していない粒子とライトの再生を取り消します。
        /// </summary>
        public void CancelPendingEffect()
        {
            if (_lastShot != null)
            {
                _lastShot.IsEffectPending = false;
            }
        }

        /// <summary>
        ///     全発射を停止し、実体は破棄せず音源の停止完了後に再利用します。
        /// </summary>
        public void StopAll()
        {
            for (int i = 0; i < _shots.Length; i++)
            {
                Shot shot = _shots[i];
                if (shot != null && shot.IsActive)
                {
                    ReleaseShot(shot);
                }
            }
        }

        private const int POOL_SIZE = 16;
        private const float MAX_PLAYBACK_SECONDS = 30f;
        private SoundEffectSource _soundTemplate;
        private ParticleSystem _particleTemplate;
        private MuzzleFlashLight _flashTemplate;
        private readonly Shot[] _shots = new Shot[POOL_SIZE];
        private Shot _lastShot;
        private bool _isInitialized;
        private bool _hasReportedPoolExhaustion;
        private bool _hasReportedPlaybackTimeout;

        /// <summary>
        ///     アニメーション評価後の銃口で遅延演出を開始し、終了した実体を回収します。
        /// </summary>
        private void LateUpdate()
        {
            for (int i = 0; i < _shots.Length; i++)
            {
                Shot shot = _shots[i];
                if (shot == null || !shot.IsActive)
                {
                    continue;
                }

                if (shot.IsEffectPending && Time.time >= shot.EffectTime)
                {
                    shot.IsEffectPending = false;
                    // 攻撃要求時の姿勢ではなく、遅延とアニメーション評価を終えた姿勢を一度だけ写す。
                    // 再生開始後は銃に追従させず、発射地点に残す。
                    CopyPose(_particleTemplate, shot.Particle);
                    CopyPose(_flashTemplate, shot.Flash);
                    shot.Particle?.Play(true);
                    shot.Flash?.Play();
                }

                // CRIの再生開始要求が処理される前に、停止中と判断して返却しない。
                if (shot.StartFrame == Time.frameCount)
                {
                    continue;
                }

                bool isParticlePlaying = shot.Particle != null && shot.Particle.IsAlive(true);
                bool isFlashing = shot.Flash != null && shot.Flash.IsFlashing;
                if (!shot.IsEffectPending && !isFlashing && !IsSoundPlaying(shot.Audio) && !isParticlePlaying)
                {
                    ReleaseShot(shot);
                }
                else if (Time.time - shot.StartTime >= MAX_PLAYBACK_SECONDS)
                {
                    if (!_hasReportedPlaybackTimeout)
                    {
                        _hasReportedPlaybackTimeout = true;
                        Debug.LogWarning(
                            $"[{nameof(StationaryWeaponEffectsView)}] 発射演出が制限時間内に終了しないため停止します。", this);
                    }
                    ReleaseShot(shot);
                }
            }
        }

        /// <summary>
        ///     所有する武器の無効化時に遅延再生と全演出を停止します。
        /// </summary>
        private void OnDisable()
        {
            StopAll();
        }

        /// <summary>
        ///     ワールドに分離した全スロットを所有者と一緒に破棄します。
        /// </summary>
        private void OnDestroy()
        {
            StopAll();
            for (int i = 0; i < _shots.Length; i++)
            {
                if (_shots[i]?.Root != null)
                {
                    Destroy(_shots[i].Root);
                }
            }
        }

        /// <summary>
        ///     発射用の実体を初期化時だけ生成し、音量登録とCRIの実体を待機中も維持します。
        /// </summary>
        private Shot CreateShot()
        {
            Shot shot = new() { Root = new GameObject("StationaryWeaponEffects") };
            shot.Root.SetActive(false);
            shot.Root.transform.SetParent(transform, false);
            if (_soundTemplate != null)
            {
                shot.Sound = Instantiate(_soundTemplate, shot.Root.transform);
                // Muzzle配下の粒子やライトは個別に再生するため、音源複製の子を動かさない。
                foreach (Transform child in shot.Sound.transform)
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
                shot.Sound.gameObject.SetActive(true);
                shot.Sound.CopyBaseVolumeFrom(_soundTemplate);
                shot.Audio = shot.Sound.GetComponent<CriAtomSource>();
                shot.Audio.playOnStart = false;
            }
            if (_particleTemplate != null)
            {
                shot.Particle = Instantiate(_particleTemplate, shot.Root.transform);
                foreach (ParticleSystem particle in shot.Particle.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ParticleSystem.MainModule main = particle.main;
                    main.playOnAwake = false;
                    main.stopAction = ParticleSystemStopAction.None;
                }
                shot.Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                shot.Particle.gameObject.SetActive(true);
            }
            if (_flashTemplate != null)
            {
                shot.Flash = Instantiate(_flashTemplate, shot.Root.transform);
                shot.Flash.gameObject.SetActive(true);
            }

            // 全実体のAwake/音量登録/CRI登録を済ませ、初回発射までは所有者と同じシーンに保つ。
            shot.Root.SetActive(true);
            return shot;
        }

        /// <summary>
        ///     再生中と停止要求の処理待ちを除外し、事前生成した空きスロットを返します。
        /// </summary>
        private Shot FindAvailableShot()
        {
            for (int i = 0; i < _shots.Length; i++)
            {
                Shot shot = _shots[i];
                if (shot != null && shot.Root != null && !shot.IsActive && !IsSoundPlaying(shot.Audio))
                {
                    return shot;
                }
            }
            return null;
        }

        /// <summary>
        ///     CRIの準備中と再生中は、停止要求後もスロットを再利用しません。
        /// </summary>
        private static bool IsSoundPlaying(CriAtomSource source)
        {
            if (source == null)
            {
                return false;
            }

            CriAtomSourceBase.Status status = source.status;
            return status == CriAtomSourceBase.Status.Prep || status == CriAtomSourceBase.Status.Playing;
        }

        /// <summary>
        ///     CRIの前回座標を再初期化し、発射地点への瞬間移動を速度として扱わせません。
        /// </summary>
        private static void ResetAudioPosition(CriAtomSource source)
        {
            if (source == null)
            {
                return;
            }

            // 音量登録を持つSoundEffectSourceとrootは有効なまま、CRIの座標履歴だけ初期化する。
            source.enabled = false;
            source.enabled = true;
            if (source.source != null)
            {
                source.source.SetVelocity(0f, 0f, 0f);
                if (!source.freezeOrientation)
                {
                    source.source.SetOrientation(source.transform.forward, source.transform.up);
                }
                source.source.Update();
            }
        }

        /// <summary>
        ///     複製元のワールド位置、姿勢、拡縮を固定された実体へ写します。
        /// </summary>
        private static void CopyPose(Component template, Component instance)
        {
            if (template == null || instance == null)
            {
                return;
            }
            instance.transform.SetPositionAndRotation(template.transform.position, template.transform.rotation);
            instance.transform.localScale = template.transform.lossyScale;
        }

        /// <summary>
        ///     再生と遅延処理だけを止め、音量登録と生成済み実体を保持したまま返却します。
        /// </summary>
        private void ReleaseShot(Shot shot)
        {
            if (_lastShot == shot)
            {
                _lastShot = null;
            }
            shot.IsActive = false;
            shot.IsEffectPending = false;
            if (shot.Sound != null)
            {
                shot.Sound.Stop();
            }
            if (shot.Particle != null)
            {
                shot.Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (shot.Flash != null)
            {
                shot.Flash.Stop();
            }
        }

        /// <summary>
        ///     一回の発射の演出と回収状態を保持します。
        /// </summary>
        private sealed class Shot
        {
            public GameObject Root;
            public SoundEffectSource Sound;
            public CriAtomSource Audio;
            public ParticleSystem Particle;
            public MuzzleFlashLight Flash;
            public float StartTime;
            public float EffectTime;
            public int StartFrame;
            public bool IsEffectPending;
            public bool IsActive;
        }
    }
}
