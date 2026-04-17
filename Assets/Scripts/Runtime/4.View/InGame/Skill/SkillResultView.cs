using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     スキル結果を表示するビュークラス。
    /// </summary>
    public class SkillResultView : MonoBehaviour
    {
        /// <summary>
        ///     ViewModelを購読してスキル結果の変化を反映するためのメソッド。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Bind(SkillResultViewModel viewModel)
        {
            if (viewModel == null) throw new System.ArgumentNullException(nameof(viewModel));
            if (_viewModel != null)
            {
                _viewModel.OnChanged -= HandleChanged;
            }
            _viewModel = viewModel;
            _viewModel.OnChanged += HandleChanged;
        }

        [SerializeField] private TMP_Text _skillIdText;
        [SerializeField] private TMP_Text _skillPatternText;

        private SkillResultViewModel _viewModel;

        /// <summary>
        ///     ViewModelのスキル結果が変更されたときに呼び出されるハンドラメソッド。
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="skillPattern"></param>
        private void HandleChanged(int skillId, ReadOnlyMemory<int> skillPattern)
        {
            _skillIdText.text = $"Skill ID: {skillId}";
            _skillPatternText.text = $"Pattern: {string.Join(", ", skillPattern.ToArray())}";
        }

        private void Awake()
        {
            if (_skillIdText == null || _skillPatternText == null)
            {
                Debug.LogError("[SkillResultView] TMP_Text が未設定です。", this);
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
