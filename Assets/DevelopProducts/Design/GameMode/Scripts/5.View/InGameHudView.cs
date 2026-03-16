using DevelopProducts.Design.GameMode.Adaptor;
using TMPro;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.View
{
    /// <summary>
    ///     UI表示するクラス。
    ///     StageHudViewModelを受け取り、HP、経過時間、結果、評価数を表示する役割を持つ。
    /// </summary>
    public class InGameHudView
    {
        public void View(StageHudViewModel viewModel)
        {
            _hpText.text = $"HP: {viewModel.CurrentPlayerHp} / {viewModel.MaxPlayerHp}";
            _timeText.text = $"Time: {viewModel.ElapsedTime:F2}s";
            _resultText.text = $"Result: {viewModel.ResultText}";
            _evaluationText.text = $"Evaluation: {viewModel.EvaluationCount}";
        }

        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _evaluationText;
    }
}
