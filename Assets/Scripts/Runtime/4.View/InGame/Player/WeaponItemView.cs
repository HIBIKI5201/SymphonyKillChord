using KillChord.Runtime.View.Persistent.Music;
using LitMotion;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     武器一つのSE再生、表示切替、Effect再生などを行うクラス。
    /// </summary>
    public class WeaponItemView : MonoBehaviour
    {
        /// <summary>
        ///     攻撃によるSEやEffect、モデル切り替えを行います。
        /// </summary>
        /// <param name="clipSeconds"> クリップの長さ。 </param>
        /// <param name="ct"> CancellationToken。 </param>
        /// <returns></returns>
        public async Awaitable PlayAsync(float clipSeconds, CancellationToken ct)
        {
            ShowWeapon();
            PlayAttackSound();
            PlayEffectAsync(ct);
            await Awaitable.WaitForSecondsAsync(clipSeconds, ct);

            HideWeapon();
        }

        /// <summary>
        ///     武器を非表示にします。
        /// </summary>
        public void HideWeapon()
        {
            if (_weaponModel == null)
            {
                return;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();

            _handle.TryCancel();
            _handle = LMotion.Create(1f, 0f, 0.5f)
                .WithOnComplete(() => _weaponModel.SetActive(false))
                .Bind(this, (value, state) => state.ApplyDither(value));
        }
        /// <summary>
        ///     武器を即座に非表示にします。
        /// </summary>
        public void HideWeaponImmidiate()
        {
            if (_weaponModel == null)
            {
                return;
            }
            _handle.TryCancel();
            _weaponModel.SetActive(false);
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        [SerializeField, Tooltip("攻撃中だけ表示する武器モデル。")]
        private GameObject _weaponModel;

        [SerializeField, Tooltip("攻撃SE用Source。")]
        private SoundEffectSource _attackSoundSource;

        [SerializeField, Tooltip("攻撃Effect。")]
        private ParticleSystem _attackEffect;

        [SerializeField, Min(0f), Tooltip("攻撃Effectを再生するまでの遅延時間。")]
        private float _effectDelaySeconds;

        [SerializeField, Tooltip("DitherのMaterialエフェクトを適用するRenderer一覧。")]
        private Renderer[] _effectRenderers;

        /// <summary>
        ///     武器を表示します。
        /// </summary>
        private MotionHandle ShowWeapon()
        {
            if (_weaponModel == null)
            {
                Debug.LogError($"WeaponModel が未設定です。{name}", this);
                return default;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();

            _handle.TryCancel();
            _handle = LSequence.Create()
                .Join(LMotion.Create(0f, 1f, 0.2f)
                    .Bind(this, (value, state) => state.ApplyDither(value)))
                .Join(LMotion.Create(1f, 0f, 0.4f)
                    .Bind(this, (value, state) => state.ApplyFlash(value)))
                .Run();
            _weaponModel.SetActive(true);
            return _handle;
        }

        /// <summary>
        ///     攻撃SEを再生します。
        /// </summary>
        private void PlayAttackSound()
        {
            if (_attackSoundSource == null)
            {
                return;
            }
            _attackSoundSource.Play();
        }

        /// <summary>
        ///     遅延後に攻撃Effectを再生します。
        /// </summary>
        private async Awaitable PlayEffectAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_attackEffect == null)
                {
                    return;
                }

                if (_effectDelaySeconds > 0f)
                {
                    await Awaitable.WaitForSecondsAsync(_effectDelaySeconds, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                _attackEffect.Play();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void ApplyDither(float value)
        {
            foreach (Renderer renderer in _effectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat(DitherId, value);
                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }
        private void ApplyFlash(float value)
        {
            foreach (Renderer renderer in _effectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }
                renderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat(FlashId, value);
                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }


        private MaterialPropertyBlock _materialPropertyBlock;
        private MotionHandle _handle;
        private readonly static int DitherId = Shader.PropertyToID("_Ratio");
        private readonly static int FlashId = Shader.PropertyToID("_Flash");
    }
}
