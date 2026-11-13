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
    [AddComponentMenu("KillChord/View/UI/HealthTextView")]
    public sealed class HealthTextView : MonoBehaviour
    {
        /// <summary>
        ///     依存関係構築、及びReactivePropertyの購読。
        /// </summary>
        /// <param name="vm">HP HUDのViewModel</param>
        public void Bind(IHealthHudViewModel vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm), "HPテキストのViewModelがNULL。");
            vm.CurrentHealth
                .Subscribe(UpdateCurrentHealthText)
                .RegisterTo(destroyCancellationToken);
            vm.MaxHealth
                .Subscribe(UpdateMaxHealthText)
                .RegisterTo(destroyCancellationToken);
        }

        [SerializeField, Tooltip("現在HPを表示するテキスト")] 
        private TextMeshProUGUI _currentHealthText;
        [SerializeField, Tooltip("最大HPを表示するテキスト")] 
        private TextMeshProUGUI _maxHealthText;

        private MotionHandle _handle;
        private int _health;

        /// <summary>
        ///     UI の参照が設定されているかを確認する。
        /// </summary>
        private void Awake()
        {
            if (_currentHealthText == null || _maxHealthText == null)
            {
                Debug.LogError($"[{nameof(HealthTextView)}] UIの参照が失われています。", this);
            }
        }

        /// <summary>
        ///     再生中のモーションを止める。
        /// </summary>
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        /// <summary>
        ///     最大HPのテキストを更新する。
        /// </summary>
        /// <param name="maxHealth"> 最大HP。 </param>
        private void UpdateMaxHealthText(float maxHealth)
        {
            // 参照欠落時は更新しない
            if (_maxHealthText == null) { return; }

            _maxHealthText.SetText("{0}", Mathf.CeilToInt(maxHealth));
        }

        /// <summary>
        ///     現在HPのテキストを更新し、増減に応じた演出を再生する。
        /// </summary>
        /// <param name="currentHealth"> 現在HP。 </param>
        private void UpdateCurrentHealthText(float currentHealth)
        {
            // 参照欠落時は更新しない
            if (_currentHealthText == null) { return; }

            int health = Mathf.CeilToInt(currentHealth);
            // 端数を切り上げて整数表示にする
            _currentHealthText.SetText("{0}", health);

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
