using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     攻撃結果を表示するビュークラス。
    /// </summary>
    public class AttackResultView : MonoBehaviour
    {
        /// <summary>
        ///     ViewModelを購読して攻撃結果の変化を反映するためのメソッド。
        /// </summary>
        /// <param name="viewModel"></param>
        public void Bind(AttackResultViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (_viewModel != null)
            {
                _viewModel.OnChanged -= HandleChanged;
            }

            _viewModel = viewModel;
            _viewModel.OnChanged += HandleChanged;
        }

        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _criticalText;

        private AttackResultViewModel _viewModel;

        /// <summary>
        ///     ViewModelから攻撃結果の変化を受け取り、UIテキストを更新するメソッド。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="isCritical"></param>
        private void HandleChanged(float damage, bool isCritical)
        {
            _damageText.text = damage.ToString();
            _criticalText.text = isCritical ? "Critical!" : "";
        }

        private void Awake()
        {
            if (_damageText == null || _criticalText == null)
            {
                Debug.LogError("[AttackResultView] TMP_Text が未設定です。", this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
                _viewModel.OnChanged -= HandleChanged;
        }
    }
}
