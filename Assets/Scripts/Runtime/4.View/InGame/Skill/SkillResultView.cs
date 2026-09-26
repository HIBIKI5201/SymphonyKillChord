using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary> スキル結果を表示するビュークラス。 </summary>
    public class SkillResultView : MonoBehaviour
    {
        /// <summary> ViewModelをバインドして変更イベントを購読する。 </summary>
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

        [SerializeField, Tooltip("発動したスキルの ID を表示するテキスト。")] private TMP_Text _skillIdText;
        [SerializeField, Tooltip("発動したスキルのパターンを表示するテキスト。")] private TMP_Text _skillPatternText;

        private SkillResultViewModel _viewModel;

        /// <summary> ViewModel変更時に呼ばれるハンドラ。表示を更新する。 </summary>
        /// <param name="skillId">スキルID</param>
        /// <param name="skillPattern">パターン（配列）</param>
        private void HandleChanged(int skillId, ReadOnlyMemory<int> skillPattern)
        {
            _skillIdText.text = $"Skill ID: {skillId}";
            _skillPatternText.text = $"Pattern: {string.Join(", ", skillPattern.ToArray())}";
        }

        /// <summary>
        ///     テキストの参照を確認し、未設定の場合はコンポーネントを無効にする。
        /// </summary>
        private void Awake()
        {
            if (_skillIdText == null || _skillPatternText == null)
            {
                Debug.LogError("[SkillResultView] TMP_Text が未設定です。", this);
                enabled = false;
            }
        }

        /// <summary>
        ///     ViewModel の変更イベントの購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            if (_viewModel != null)
                _viewModel.OnChanged -= HandleChanged;

        }

        /// <summary>
        ///     テキストを未発動の表示で初期化する。
        /// </summary>
        private void Start()
        {
            _skillIdText.text = "Skill ID: N/A";
            _skillPatternText.text = "Pattern: N/A";
        }
    }
}
