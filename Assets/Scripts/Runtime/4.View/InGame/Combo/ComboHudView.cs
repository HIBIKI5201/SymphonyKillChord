using R3;
using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Combo
{
    /// <summary>
    ///     コンボを表示するビュークラス。
    /// </summary>
    public class ComboHudView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="viewModel"> 購読するコンボHUDのViewModelです。 </param>
        /// <param name="comboVisibleCount"> コンボ表示を開始する最小コンボ数です。 </param>
        public void Initialize(ComboHudViewModel viewModel, int comboVisibleCount)
        {
            if(_comboText == null)
            {
                Debug.LogError($"{_comboText}がNullです。");
            }

            _comboDisposable?.Dispose();
            _comboHudViewModel = viewModel;

            _comboDisposable = _comboHudViewModel.ComboCount
                .Subscribe(comboCount =>
                  {
                      if (_comboText == null) { return; }

                      if (comboCount < comboVisibleCount)
                      {
                          _comboText.SetText(string.Empty);
                      }
                      else
                      {
                          _comboText.SetText("{0}コンボ", comboCount);
                      }
                  }).RegisterTo(destroyCancellationToken);
        }

        [SerializeField] private TextMeshProUGUI _comboText;
        private ComboHudViewModel _comboHudViewModel;
        private IDisposable _comboDisposable;
    }
}
