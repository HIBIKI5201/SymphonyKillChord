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
        /// <exception cref="ArgumentNullException"> viewModelがnullの場合にスローされます。 </exception>
        public void Initialize(ComboHudViewModel viewModel, int comboVisibleCount)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            _comboVisibleCount = comboVisibleCount;

            _comboDisposable?.Dispose();
            _comboHudViewModel = viewModel;

            _comboDisposable = _comboHudViewModel.ComboCount
                .Subscribe(comboCount =>
                  {
                      if (_comboText == null) { return; }

                      if (comboCount < _comboVisibleCount)
                      {
                          _comboText.SetText(string.Empty);
                      }
                      else
                      {
                          _comboText.SetText($"{comboCount}コンボ");
                      }
                  }).RegisterTo(destroyCancellationToken);
        }

        [SerializeField] private TextMeshProUGUI _comboText;
        private int _comboVisibleCount;
        private ComboHudViewModel _comboHudViewModel;
        private IDisposable _comboDisposable;
    }
}
