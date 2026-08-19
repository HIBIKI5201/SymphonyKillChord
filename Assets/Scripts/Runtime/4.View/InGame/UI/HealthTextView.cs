using KillChord.Runtime.Adaptor.InGame.UI;
using LitMotion;
using LitMotion.Extensions;
using R3;
using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     現在HPと最大HPをテキストのみで表示するViewクラス。
    /// </summary>
    public class HealthTextView : MonoBehaviour
    {
        /// <summary>
        ///     依存関係構築、及びReactivePropertyの購読。
        /// </summary>
        /// <param name="vm">HP HUDのViewModel</param>
        public void Bind(IHealthHudViewModel vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm), "HPテキストのViewModelがNULL。");
            vm.HealthHudDTO
                .Subscribe(UpdateHealthText)
                .RegisterTo(destroyCancellationToken);
        }

        [SerializeField, Tooltip("現在HPを表示するテキスト")] 
        private TextMeshProUGUI _currentHealthText;
        [SerializeField, Tooltip("最大HPを表示するテキスト")] 
        private TextMeshProUGUI _maxHealthText;

        private MotionHandle _handle;
        private int _health;

        private void Awake()
        {
            if (_currentHealthText == null || _maxHealthText == null)
            {
                Debug.LogError($"[{nameof(HealthTextView)}] UIの参照が失われています。", this);
            }
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        /// <summary>
        ///     HPテキストを更新する。
        /// </summary>
        /// <param name="dto">HP HUD用のDTO</param>
        private void UpdateHealthText(HealthHudDTO dto)
        {
            // 参照欠落時は更新しない
            if (_currentHealthText == null || _maxHealthText == null) return;

            int health = Mathf.CeilToInt(dto.CurrentHealth);
            // 端数を切り上げて整数表示にする
            _currentHealthText.SetText("{0}", health);
            _maxHealthText.SetText("{0}", Mathf.CeilToInt(dto.MaxHealth));

            if (health == _health)
            {
                return;
            }

            _handle.TryComplete();
            if(health <= _health)
            {
                _handle = LSequence.Create()
                .Join(LMotion.Create(-10f, 0f, 0.1f)
                    .WithEase(Ease.InCubic)
                    .BindToAnchoredPositionY(_currentHealthText.rectTransform))
                .Join(LMotion.Create(Color.red, Color.white, 0.05f)
                    .WithLoops(4)
                    .BindToColor(_currentHealthText))
                .Run();
            }
            else
            {
                _handle = LMotion.Create(Color.green, Color.white, 0.2f)
                    .BindToColor(_currentHealthText);
            }
            _health = health;
        }
    }
}
