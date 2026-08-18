using KillChord.Runtime.Adaptor.InGame.UI;
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

        private void Awake()
        {
            if (_currentHealthText == null || _maxHealthText == null)
            {
                Debug.LogError($"[{nameof(HealthTextView)}] UIの参照が失われています。", this);
            }
        }

        /// <summary>
        ///     HPテキストを更新する。
        /// </summary>
        /// <param name="dto">HP HUD用のDTO</param>
        private void UpdateHealthText(HealthHudDTO dto)
        {
            // 参照欠落時は更新しない
            if (_currentHealthText == null || _maxHealthText == null) return;

            // 端数を切り上げて整数表示にする
            _currentHealthText.SetText("{0}", Mathf.CeilToInt(dto.CurrentHealth));
            _maxHealthText.SetText("{0}", Mathf.CeilToInt(dto.MaxHealth));
        }
    }
}
