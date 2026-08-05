using KillChord.Runtime.View.Persistent.Music;
using LitMotion;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     武器一つのSE再生、表示切替、Effect再生などを行うクラス。
    /// </summary>
    public sealed class WeaponItemView : MonoBehaviour
    {
        /// <summary>
        ///     攻撃によるSEやEffect、モデル切り替えを行います。
        /// </summary>
        public void Play()
        {
            ShowWeapon();
            PlayAttackSound();
            PlayEffect();
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

            // 遅延待ちのEffectが非表示後に発火しないよう、Dither開始前に打ち消す。
            _effectHandle.TryCancel();
            _weaponHandle.TryCancel();
            _weaponHandle = LMotion.Create(1f, 0f, 0.5f)
                .WithOnComplete(() => _weaponModel.SetActive(false))
                .Bind(this, (value, state) => state.ApplyDither(value));
        }
        /// <summary>
        ///     武器を即座に非表示にします。
        /// </summary>
        public void HideWeaponImmediate()
        {
            if (_weaponModel == null)
            {
                return;
            }
            // 遅延待ちのEffectが非表示後に発火しないよう、モデルを消す前に打ち消す。
            _effectHandle.TryCancel();
            _weaponHandle.TryCancel();
            _weaponModel.SetActive(false);
        }

        /// <summary>
        ///     破棄時に再生中のMotionを全て打ち消します。
        /// </summary>
        private void OnDestroy()
        {
            _weaponHandle.TryCancel();
            _effectHandle.TryCancel();
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
        private void ShowWeapon()
        {
            if (_weaponModel == null)
            {
                Debug.LogError($"{nameof(WeaponItemView)}が未設定です。", this);
                return;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();

            _weaponHandle.TryCancel();
            _weaponHandle = LSequence.Create()
                .Join(LMotion.Create(0f, 1f, 0.2f)
                    .Bind(this, (value, state) => state.ApplyDither(value)))
                .Join(LMotion.Create(1f, 0f, 0.4f)
                    .Bind(this, (value, state) => state.ApplyFlash(value)))
                .AppendInterval(2f)
                .Run(x => x.WithOnComplete(HideWeapon));
            _weaponModel.SetActive(true);
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
        private void PlayEffect()
        {
            if (_attackEffect == null)
            {
                return;
            }
            _effectHandle.TryCancel();
            // 値を使わないMotionを遅延タイマーとして扱い、完了コールバックでEffectを再生する。
            _effectHandle = LMotion.Create(0, 0, _effectDelaySeconds)
                .WithOnComplete(() => _attackEffect.Play())
                .WithOnCancel(() => _attackEffect.Stop(true))
                .RunWithoutBinding();
        }

        /// <summary>
        ///     全Rendererに現在のDither値を適用します。
        /// </summary>
        /// <param name="value"> 適用するDither値。 </param>
        private void ApplyDither(float value)
        {
            foreach (Renderer renderer in _effectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat(DITHER_ID, value);
                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }

        /// <summary>
        ///     全Rendererに現在のFlash値を適用します。
        /// </summary>
        /// <param name="value"> 適用するFlash値。 </param>
        private void ApplyFlash(float value)
        {
            foreach (Renderer renderer in _effectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }
                renderer.GetPropertyBlock(_materialPropertyBlock);
                _materialPropertyBlock.SetFloat(FLASH_ID, value);
                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }


        private MaterialPropertyBlock _materialPropertyBlock;
        private MotionHandle _weaponHandle;
        private MotionHandle _effectHandle;
        private readonly static int DITHER_ID = Shader.PropertyToID("_Ratio");
        private readonly static int FLASH_ID = Shader.PropertyToID("_Flash");
    }
}
