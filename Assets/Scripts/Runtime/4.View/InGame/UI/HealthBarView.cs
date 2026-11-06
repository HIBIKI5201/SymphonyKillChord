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
    ///     被弾時は減った分を赤い残像として残して遅れて縮め、ゲージを白く点滅させる。
    ///     回復時は緑のフラッシュを重ねて増加を伝える。
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

        /// <summary> 被弾後、赤い残像が縮み始めるまでの待ち時間（秒）。 </summary>
        private const float DAMAGE_TRAIL_DELAY_SECONDS = 0.5f;

        /// <summary> 赤い残像が現在HPまで縮みきるまでの時間（秒）。 </summary>
        private const float DAMAGE_TRAIL_SHRINK_SECONDS = 0.3f;

        /// <summary> 被弾フラッシュ1回分の時間（秒）。 </summary>
        private const float DAMAGE_FLASH_DURATION_SECONDS = 0.05f;

        /// <summary> 被弾フラッシュの点滅回数。 </summary>
        private const int DAMAGE_FLASH_LOOP_COUNT = 6;

        /// <summary> 回復フラッシュが元の色へ戻るまでの時間（秒）。 </summary>
        private const float HEAL_FLASH_DURATION_SECONDS = 3f;

        [SerializeField, Tooltip("現在HPを表示するImage")]
        private Image _healthBarImage;

        [SerializeField, Tooltip("減った分のHPを遅れて縮めて示す赤い残像のImage。ゲージの後ろに置く")]
        private Image _damageTrailImage;

        [SerializeField, Tooltip("被弾時にゲージへ重ねて白く点滅させるImage。ゲージの前に置く")]
        private Image _damageFlashImage;

        private MotionHandle _handle;
        private MotionHandle _trailHandle;

        /// <summary>
        ///     インスペクター参照の設定漏れを検知する。
        /// </summary>
        private void Awake()
        {
            if (_healthBarImage == null)
            {
                Debug.LogError($"[{nameof(HealthBarView)}] {nameof(_healthBarImage)} が未設定です。", this);
            }

            if (_damageTrailImage == null)
            {
                Debug.LogError($"[{nameof(HealthBarView)}] {nameof(_damageTrailImage)} が未設定です。", this);
            }

            if (_damageFlashImage == null)
            {
                Debug.LogError($"[{nameof(HealthBarView)}] {nameof(_damageFlashImage)} が未設定です。", this);
            }
        }

        /// <summary>
        ///     破棄時に再生中のアニメーションを停止する。
        /// </summary>
        private void OnDestroy()
        {
            _handle.TryCancel();
            _trailHandle.TryCancel();
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
                _trailHandle.TryCancel();
                _healthBarImage.fillAmount = 0f;
                SyncOverlayFill(0f);
                return;
            }

            float newFillAmount = Mathf.Clamp01(dto.CurrentHealth / dto.MaxHealth);
            float currentFillAmount = _healthBarImage.fillAmount;

            if (currentFillAmount == newFillAmount)
            {
                return;
            }

            _handle.TryComplete();

            // 減少と増加で演出を変え、被弾か回復かを一目で判別できるようにする。
            if (newFillAmount <= currentFillAmount)
            {
                PlayDamageTrail(currentFillAmount, newFillAmount);

                MotionSequenceBuilder sequence = LSequence.Create()
                    .Join(LMotion.Create(currentFillAmount, newFillAmount, FILL_DURATION_SECONDS)
                        .BindToFillAmount(_healthBarImage));

                // 白いImageを重ねて点滅させ、ゲージの本来の色（緑）と交互に見せる。
                if (_damageFlashImage != null)
                {
                    sequence = sequence
                        .Join(LMotion.Create(currentFillAmount, newFillAmount, FILL_DURATION_SECONDS)
                            .BindToFillAmount(_damageFlashImage))
                        .Join(LMotion.Create(1f, 0f, DAMAGE_FLASH_DURATION_SECONDS)
                            .WithLoops(DAMAGE_FLASH_LOOP_COUNT)
                            .BindToColorA(_damageFlashImage));
                }

                _handle = sequence.Run();
            }
            else
            {
                // 回復中は残像を見せない。ゲージの後ろへ隠し、次の被弾の起点にする。
                _trailHandle.TryCancel();
                SyncOverlayFill(currentFillAmount);

                _handle = LSequence.Create()
                    .Join(LMotion.Create(currentFillAmount, newFillAmount, FILL_DURATION_SECONDS)
                        .BindToFillAmount(_healthBarImage))
                    .Join(LMotion.Create(Color.green, Color.white, HEAL_FLASH_DURATION_SECONDS)
                        .BindToColor(_healthBarImage))
                    .Run();
            }
        }

        /// <summary>
        ///     減った分を赤い残像として残し、待ち時間の後に新しい値まで縮める。
        ///     連続で被弾したときは、残像を縮みかけの位置から伸ばし直さずに待ち時間だけやり直す。
        /// </summary>
        /// <param name="currentFillAmount"> 被弾前のゲージの充填量。 </param>
        /// <param name="newFillAmount"> 被弾後のゲージの充填量。 </param>
        private void PlayDamageTrail(float currentFillAmount, float newFillAmount)
        {
            if (_damageTrailImage == null)
            {
                return;
            }

            _trailHandle.TryCancel();

            // 回復直後などで残像がゲージより短いときは、被弾前の位置から始める。
            float trailStart = Mathf.Max(_damageTrailImage.fillAmount, currentFillAmount);
            _damageTrailImage.fillAmount = trailStart;

            _trailHandle = LMotion.Create(trailStart, newFillAmount, DAMAGE_TRAIL_SHRINK_SECONDS)
                .WithDelay(DAMAGE_TRAIL_DELAY_SECONDS)
                .BindToFillAmount(_damageTrailImage);
        }

        /// <summary>
        ///     残像と点滅用のImageの充填量をそろえる。
        /// </summary>
        /// <param name="fillAmount"> 設定する充填量。 </param>
        private void SyncOverlayFill(float fillAmount)
        {
            if (_damageTrailImage != null)
            {
                _damageTrailImage.fillAmount = fillAmount;
            }

            if (_damageFlashImage != null)
            {
                _damageFlashImage.fillAmount = fillAmount;
            }
        }
    }
}
