using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    /// スキル結果を表示するビュークラス。
    /// </summary>
    public class SkillResultView : MonoBehaviour
    {
        /// <summary>
        /// ViewModelをバインドして変更イベントを購読する。
        /// </summary>
        /// <param name="viewModel">バインドするViewModel</param>
        /// <exception cref="System.ArgumentNullException">viewModelがnullの場合</exception>
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
        /// ViewModel変更時に呼ばれるハンドラ。表示を更新する。
        /// </summary>
        /// <param name="skillId">スキルID</param>
        /// <param name="skillPattern">パターン（配列）</param>
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

        private void Start()
        {
            _skillIdText.text = "Skill ID: N/A";
            _skillPatternText.text = "Pattern: N/A";
        }
    }
}
