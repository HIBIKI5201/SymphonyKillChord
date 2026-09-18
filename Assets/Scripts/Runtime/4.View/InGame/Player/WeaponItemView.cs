using KillChord.Runtime.View.Persistent.Music;
using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;

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
            PlayAttackEffects();
        }

        /// <summary>
        ///     武器表示を変更せず、攻撃時のSEやEffectだけを再生します。
        /// </summary>
        public void PlayAttackEffects()
        {
            PlayWeaponFlash();
            EnsureAttackEffects();
            _attackEffects.Play(_effectDelaySeconds);
            EjectCasing();
        }

        /// <summary>
        ///     インゲームと同じシェーダー演出で武器を表示します。
        /// </summary>
        public void ShowWeapon()
        {
            if (_weaponModel == null)
            {
                Debug.LogError($"{nameof(WeaponItemView)}が未設定です。", this);
                return;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();

            _weaponHandle.TryCancel();
            _dissolveMaterials?.Begin();
            _weaponHandle = LSequence.Create()
                .Join(LMotion.Create(0f, 1f, 0.2f)
                    .Bind(this, (value, state) => state.ApplyShowDither(value)))
                .AppendInterval(2f)
                .Run(x => x.WithOnComplete(HideWeapon));
            ApplyDither(0.0f);
            _weaponModel.SetActive(true);
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
            _attackEffects?.CancelPendingEffect();
            _flashHandle.TryCancel();
            ApplyFlash(0.0f);
            _weaponHandle.TryCancel();
            _dissolveMaterials?.Begin();
            _weaponHandle = LMotion.Create(1f, 0f, 0.5f)
                .WithOnComplete(CompleteHide)
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

            _materialPropertyBlock ??= new MaterialPropertyBlock();

            // 遅延待ちのEffectが非表示後に発火しないよう、モデルを消す前に打ち消す。
            _attackEffects?.CancelPendingEffect();
            _flashHandle.TryCancel();
            ApplyFlash(0.0f);
            _weaponHandle.TryCancel();
            _weaponModel.SetActive(false);
            RestoreOriginalMaterials();
        }

        /// <summary>
        ///     破棄時に再生中のMotionを全て打ち消します。
        /// </summary>
        private void OnDestroy()
        {
            _weaponHandle.TryCancel();
            if (_attackEffects != null)
            {
                Destroy(_attackEffects);
            }
            _flashHandle.TryCancel();
            _dissolveMaterials?.Dispose();
        }

        /// <summary>
        ///     元の粒子の自動再生を止め、発射演出の所有者を初期化します。
        /// </summary>
        private void Awake()
        {
            if (_weaponModel != null && _dissolveShader != null)
            {
                _dissolveMaterials = new WeaponDissolveMaterials(_weaponModel, _dissolveShader);
            }
            EnsureAttackEffects();
        }

        /// <summary>
        ///     武器Viewの無効化時にワールドへ分離した演出を停止します。
        /// </summary>
        private void OnDisable()
        {
            if (_dissolveMaterials != null)
            {
                HideWeaponImmediate();
            }
            _attackEffects?.StopAll();
        }

        /// <summary>
        ///     発射演出の所有者を必要時に生成します。
        /// </summary>
        private void EnsureAttackEffects()
        {
            if (_attackEffects != null)
            {
                return;
            }
            _attackEffects = gameObject.AddComponent<StationaryWeaponEffectsView>();
            _attackEffects.Initialize(_attackSoundSource, _attackEffect, _muzzleFlashLight);
        }

        [SerializeField, Tooltip("攻撃中だけ表示する武器モデル。")]
        private GameObject _weaponModel;

        [SerializeField, Tooltip("攻撃SE用Source。")]
        private SoundEffectSource _attackSoundSource;

        [SerializeField, Tooltip("攻撃Effect。")]
        private ParticleSystem _attackEffect;

        [SerializeField, Tooltip("攻撃時に点滅させるライト。")]
        private MuzzleFlashLight _muzzleFlashLight;

        [SerializeField, Min(0f), Tooltip("攻撃Effectを再生するまでの遅延時間。")]
        private float _effectDelaySeconds;

        [SerializeField, Tooltip("攻撃時に薬莢を排出するEjector。未設定の場合は排出しません。")]
        private CasingEjectorView _casingEjector;

        [SerializeField, Tooltip("DitherのMaterialエフェクトを適用するRenderer一覧。")]
        private Renderer[] _effectRenderers;

        [SerializeField, Tooltip("出現・収納時だけ使うシェーダー。通常表示は元材質を保持し、未指定なら従来のRenderer設定を使います。")]
        private Shader _dissolveShader;

        /// <summary>
        ///     出現完了または演出中断後に、通常時の材質へ復元します。
        /// </summary>
        private void RestoreOriginalMaterials()
        {
            _dissolveMaterials?.Restore();
        }

        /// <summary>
        ///     収納演出が完了したモデルを非表示にし、次の表示へ向けて材質を復元します。
        /// </summary>
        private void CompleteHide()
        {
            if (_weaponModel != null)
            {
                _weaponModel.SetActive(false);
            }
            RestoreOriginalMaterials();
        }

        /// <summary>
        ///     発砲時だけ武器マテリアルを発光させます。
        /// </summary>
        private void PlayWeaponFlash()
        {
            if (_weaponModel == null)
            {
                return;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();
            _flashHandle.TryCancel();
            _flashHandle = LMotion.Create(1f, 0f, 0.4f)
                .Bind(this, (value, state) => state.ApplyFlash(value));
        }

        /// <summary>
        ///     薬莢を排出します。
        /// </summary>
        private void EjectCasing()
        {
            if (_casingEjector == null)
            {
                return;
            }
            _casingEjector.Eject();
        }

        /// <summary>
        ///     出現の表示率を適用し、全表示へ到達した時点で通常材質へ戻します。
        /// </summary>
        /// <param name="value"> 出現中の表示率です。 </param>
        private void ApplyShowDither(float value)
        {
            ApplyDither(value);
            if (value >= 1f)
            {
                RestoreOriginalMaterials();
            }
        }

        /// <summary>
        ///     全Rendererに現在のDither値を適用します。
        /// </summary>
        /// <param name="value"> 適用するDither値。 </param>
        private void ApplyDither(float value)
        {
            if (_dissolveMaterials != null)
            {
                _dissolveMaterials.SetRatio(value);
                return;
            }
            if (_effectRenderers == null)
            {
                return;
            }
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
            if (_effectRenderers == null)
            {
                return;
            }
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
        private WeaponDissolveMaterials _dissolveMaterials;
        private MotionHandle _weaponHandle;
        private StationaryWeaponEffectsView _attackEffects;
        private MotionHandle _flashHandle;
        private readonly static int DITHER_ID = Shader.PropertyToID("_Ratio");
        private readonly static int FLASH_ID = Shader.PropertyToID("_Flash");
    }
}
