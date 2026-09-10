using KillChord.Runtime.Adaptor.InGame.UI;
using LitMotion;
using LitMotion.Extensions;
using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     現在HPをゲージの充填量で表示するViewクラス。
    ///     被弾時は赤、回復時は緑のフラッシュを重ねて増減の向きを伝える。
    /// </summary>
    public sealed class HealthBarView : MonoBehaviour
    {
        /// <summary>
        ///     依存関係構築、及びReactivePropertyの購読。
        /// </summary>
        /// <param name="vm"> HP HUDのViewModel。 </param>
        /// <exception cref="ArgumentNullException"> ViewModelがnullの場合。 </exception>
        public void Bind(IHealthHudViewModel vm)
        {
            if (vm == null)
            {
                throw new ArgumentNullException(nameof(vm), "HPバーのViewModelがNULL。");
            }

            vm.HealthHudDTO
                .Subscribe(UpdateHealthBar)
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary> ゲージの充填量が変化しきるまでの時間（秒）。 </summary>
        private const float FILL_DURATION_SECONDS = 0.2f;

        /// <summary> 被弾フラッシュ1回分の時間（秒）。 </summary>
        private const float DAMAGE_FLASH_DURATION_SECONDS = 0.05f;

        /// <summary> 被弾フラッシュの点滅回数。 </summary>
        private const int DAMAGE_FLASH_LOOP_COUNT = 6;

        /// <summary> 回復フラッシュが元の色へ戻るまでの時間（秒）。 </summary>
        private const float HEAL_FLASH_DURATION_SECONDS = 3f;

        [SerializeField, Tooltip("現在HPを表示するImage")]
        private Image _healthBarImage;

        private MotionHandle _handle;

        /// <summary>
        ///     インスペクター参照の設定漏れを検知する。
        /// </summary>
        private void Awake()
        {
            if (_healthBarImage == null)
            {
                Debug.LogError($"[{nameof(HealthBarView)}] {nameof(_healthBarImage)} が未設定です。", this);
            }
        }

        /// <summary>
        ///     破棄時に再生中のアニメーションを停止する。
        /// </summary>
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        /// <summary>
        ///     HPの変化をゲージの充填量とフラッシュ演出へ反映する。
        /// </summary>
        /// <param name="dto"> HP HUDの表示データ。 </param>
        private void UpdateHealthBar(HealthHudDTO dto)
        {
            // 参照欠落時は更新しない。
            if (_healthBarImage == null)
            {
                return;
            }

            // 最大HPが未設定(0以下)の場合はゼロ除算になるため、空表示にして抜ける。
            if (dto.MaxHealth <= 0f)
            {
                _handle.TryComplete();
                _healthBarImage.fillAmount = 0f;
                return;
            }

            float newFillAmount = Mathf.Clamp01(dto.CurrentHealth / dto.MaxHealth);
            float currentFillAmount = _healthBarImage.fillAmount;

            if (currentFillAmount == newFillAmount)
            {
                return;
            }

            _handle.TryComplete();

            // 減少と増加でフラッシュ色を変え、被弾か回復かを一目で判別できるようにする。
            if (newFillAmount <= currentFillAmount)
            {
                _handle = LSequence.Create()
                    .Join(LMotion.Create(currentFillAmount, newFillAmount, FILL_DURATION_SECONDS)
                        .BindToFillAmount(_healthBarImage))
                    .Join(LMotion.Create(Color.red, Color.white, DAMAGE_FLASH_DURATION_SECONDS)
                        .WithLoops(DAMAGE_FLASH_LOOP_COUNT)
                        .BindToColor(_healthBarImage))
                    .Run();
            }
            else
            {
                _handle = LSequence.Create()
                    .Join(LMotion.Create(currentFillAmount, newFillAmount, FILL_DURATION_SECONDS)
                        .BindToFillAmount(_healthBarImage))
                    .Join(LMotion.Create(Color.green, Color.white, HEAL_FLASH_DURATION_SECONDS)
                        .BindToColor(_healthBarImage))
                    .Run();
            }
        }
    }
}
