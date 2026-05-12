using KillChord.Runtime.Adaptor.InGame.UI;
using LitMotion;
using LitMotion.Extensions;
using R3;
using SymphonyFrameWork.Attribute;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     HPバーHUDのViewクラス。
    /// </summary>
    public class HealthHudView : MonoBehaviour
    {
        /// <summary>
        ///     依存関係構築、及びReactivePropertyの購読。
        /// </summary>
        /// <param name="vm"></param>
        public void Bind(IHealthHudViewModel vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm), "HP HUDのViewModelがNULL。");
            _vm = vm;
            _vm.HealthHudDTO.Subscribe(UpdateHpHud).RegisterTo(destroyCancellationToken);
        }

        private void Awake()
        {
            if (_healthBarImage == null
                || _healthBarImageRed == null
                || _currentHealthText == null
                || _maxHealthText == null)
            {
                Debug.LogError("[HealthHudView] UIの参照が失っています。", this);
            }
        }

        [SerializeField] private Image _healthBarImage;
        [SerializeField] private Image _healthBarImageRed;
        [SerializeField] private TextMeshProUGUI _currentHealthText;
        [SerializeField] private TextMeshProUGUI _maxHealthText;

        [Header("アニメーション関連")]
        [SerializeField, Tooltip("HP変化分の反映アニメーションの長さ（秒）")]
        private float _healthBarTransitionDur = 0.5f;
        [SerializeField, Tooltip("HP変化後アニメーション開始までの間隔（秒）")]
        private float _animationStartDelay = 0.5f;

#if UNITY_EDITOR
        [Header("デバッグ用")]
        [SerializeField]
        private bool _debugFlg = false;
        [DisplayText("ZキーでHPゲージ減少、Xキーで満タン")]
        [SerializeField]
        private float _decreasePercent = 0.2f;
#endif

        private IHealthHudViewModel _vm;

        // MotionHandleのキャッシュ。ダメージが連続発生する場合に制御用
        private MotionHandle _healthBarDamageMotion;

        /// <summary>
        ///     HUDを更新する。
        /// </summary>
        /// <param name="dto">HP HUD用のDTO</param>
        private void UpdateHpHud(HealthHudDTO dto)
        {
            if (_healthBarImage == null || _healthBarImageRed == null || _currentHealthText == null || _maxHealthText == null) return;

            PlayHealthHudDamageMotion(dto);
        }

        /// <summary>
        ///     ダメージ時のHUDアニメーションを再生する。
        /// </summary>
        private void PlayHealthHudDamageMotion(in HealthHudDTO dto)
        {
            // UIの数値を更新
            _currentHealthText.SetText("{0}", Mathf.CeilToInt(dto.CurrentHealth));
            _maxHealthText.SetText("{0}", Mathf.CeilToInt(dto.MaxHealth));

            // 緑ゲージ即減少
            _healthBarImage.fillAmount = dto.MaxHealth > 0f ?
                Mathf.Clamp01(dto.CurrentHealth / dto.MaxHealth) : 0f;

            // 赤ゲージが追いつく
            // 連続ダメージの場合、再生中のアニメーションを停止し、新たにアニメーションを開始する
            _healthBarDamageMotion.TryCancel();
            _healthBarDamageMotion = LMotion.Create(_healthBarImageRed.fillAmount, _healthBarImage.fillAmount, _healthBarTransitionDur)
                .WithDelay(_animationStartDelay)
                .WithEase(Ease.OutExpo)
                .BindToFillAmount(_healthBarImageRed);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!_debugFlg) return;
            if (Keyboard.current == null) return;
            if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                _healthBarImage.fillAmount = _healthBarImage.fillAmount < _decreasePercent ? 0f : _healthBarImage.fillAmount - _decreasePercent;
                _healthBarDamageMotion.TryCancel();
                _healthBarDamageMotion = LMotion.Create(_healthBarImageRed.fillAmount, _healthBarImage.fillAmount, _healthBarTransitionDur)
                    .WithDelay(_animationStartDelay)
                    .WithEase(Ease.OutExpo)
                    .BindToFillAmount(_healthBarImageRed);
            }
            else if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                _healthBarImage.fillAmount = 1f;
                _healthBarImageRed.fillAmount = 1f;
            }
        }
#endif
    }
}
