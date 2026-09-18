using CriWare;
using Cysharp.Threading.Tasks;
using KillChord.Runtime.View.Persistent.Music;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     発射ごとの演出実体をワールド位置に保持し、再生終了後に回収します。
    /// </summary>
    public sealed class StationaryWeaponEffectsView : MonoBehaviour
    {
        /// <summary>
        ///     武器が保持する演出の複製元を設定します。
        /// </summary>
        public void Initialize(SoundEffectSource sound, ParticleSystem particle, MuzzleFlashLight flash)
        {
            _soundTemplate = sound;
            _particleTemplate = particle;
            _flashTemplate = flash;
            if (_particleTemplate != null)
            {
                _particleTemplate.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                foreach (ParticleSystem particleSystem in _particleTemplate.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ParticleSystem.MainModule main = particleSystem.main;
                    main.playOnAwake = false;
                }
            }
        }

        /// <summary>
        ///     発射時点の位置と姿勢でSE、粒子、ライトを再生します。
        /// </summary>
        /// <param name="effectDelaySeconds"> 粒子再生までの遅延秒数です。 </param>
        public void Play(float effectDelaySeconds)
        {
            if (!isActiveAndEnabled || (_soundTemplate == null && _particleTemplate == null && _flashTemplate == null))
            {
                return;
            }
            CancelPendingEffect();
            Shot shot = _pool.Count > 0 ? _pool.Pop() : CreateShot();
            shot.Generation++;
            shot.Cancellation = new CancellationTokenSource();
            shot.StartTime = Time.time;
            shot.EffectTime = shot.StartTime + Mathf.Max(0f, effectDelaySeconds);
            shot.IsEffectPending = shot.Particle != null;
            CopyPose(_soundTemplate, shot.Sound);
            CopyPose(_particleTemplate, shot.Particle);
            CopyPose(_flashTemplate, shot.Flash);
            shot.Root.SetActive(true);
            _active.Add(shot);
            _lastShot = shot;

            if (shot.Sound != null)
            {
                shot.Sound.Play();
            }
            if (shot.Flash != null)
            {
                shot.IsFlashing = true;
                FlashAsync(shot, shot.Generation, shot.Cancellation.Token).Forget();
            }
        }

        /// <summary>
        ///     最新の発射でまだ開始していない粒子再生を取り消します。
        /// </summary>
        public void CancelPendingEffect()
        {
            if (_lastShot == null)
            {
                return;
            }

            _lastShot.IsEffectPending = false;
        }

        /// <summary>
        ///     全発射を取り消し、停止処理中の実体を再利用せず破棄します。
        /// </summary>
        public void StopAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ReleaseShot(i, false);
            }
        }

        private const int MAX_POOL_SIZE = 16;
        private const float MAX_PLAYBACK_SECONDS = 30f;
        private SoundEffectSource _soundTemplate;
        private ParticleSystem _particleTemplate;
        private MuzzleFlashLight _flashTemplate;
        private readonly Stack<Shot> _pool = new();
        private readonly List<Shot> _active = new();
        private Shot _lastShot;

        /// <summary>
        ///     遅延再生と演出終了を監視し、終了した実体を回収します。
        /// </summary>
        private void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Shot shot = _active[i];
                if (shot.IsEffectPending && Time.time >= shot.EffectTime)
                {
                    shot.IsEffectPending = false;
                    shot.Particle.Play(true);
                }

                bool isSoundPlaying = shot.Audio != null &&
                    (shot.Audio.status == CriAtomSourceBase.Status.Prep || shot.Audio.status == CriAtomSourceBase.Status.Playing);
                bool isParticlePlaying = shot.Particle != null && shot.Particle.IsAlive(true);
                if (!shot.IsEffectPending && !shot.IsFlashing && !isSoundPlaying && !isParticlePlaying)
                {
                    ReleaseShot(i);
                }
                else if (Time.time - shot.StartTime >= MAX_PLAYBACK_SECONDS)
                {
                    // ループCueや無限粒子を設定しても発射実体を蓄積させない。
                    Debug.LogWarning($"[{nameof(StationaryWeaponEffectsView)}] 発射演出が制限時間内に終了しないため停止します。", this);
                    ReleaseShot(i, false);
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
        ///     ワールドに分離した待機実体も所有者と一緒に破棄します。
        /// </summary>
        private void OnDestroy()
        {
            StopAll();
            while (_pool.Count > 0)
            {
                Destroy(_pool.Pop().Root);
            }
        }

        /// <summary>
        ///     発射用の非アクティブな演出実体を生成します。
        /// </summary>
        private Shot CreateShot()
        {
            Shot shot = new() { Root = new GameObject("StationaryWeaponEffects") };
            shot.Root.SetActive(false);
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
            return shot;
        }

        /// <summary>
        ///     ライト演出の終了を待ち、同じ発射世代だけへ結果を反映します。
        /// </summary>
        private async UniTaskVoid FlashAsync(Shot shot, int generation, CancellationToken token)
        {
            try
            {
                await shot.Flash.Flash(token);
            }
            catch (OperationCanceledException)
            {
                // 武器の無効化・破棄による取消は正常な終了。
            }
            finally
            {
                if (shot.Generation == generation)
                {
                    shot.IsFlashing = false;
                }
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
        ///     再生、遅延処理、音量登録を止めて実体を待機プールへ戻します。
        /// </summary>
        private void ReleaseShot(int index, bool returnToPool = true)
        {
            Shot shot = _active[index];
            _active.RemoveAt(index);
            if (_lastShot == shot)
            {
                _lastShot = null;
            }
            shot.Generation++;
            shot.Cancellation.Cancel();
            shot.Cancellation.Dispose();
            shot.Cancellation = null;
            shot.IsFlashing = false;
            shot.IsEffectPending = false;
            if (shot.Sound != null)
            {
                shot.Sound.Stop();
            }
            if (shot.Particle != null)
            {
                shot.Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (shot.Root == null)
            {
                return;
            }
            shot.Root.SetActive(false);
            if (returnToPool && _pool.Count < MAX_POOL_SIZE)
            {
                _pool.Push(shot);
            }
            else
            {
                Destroy(shot.Root);
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
            public CancellationTokenSource Cancellation;
            public int Generation;
            public float StartTime;
            public float EffectTime;
            public bool IsEffectPending;
            public bool IsFlashing;
        }
    }
}
