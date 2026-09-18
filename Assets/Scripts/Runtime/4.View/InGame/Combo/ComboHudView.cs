using LitMotion;
using LitMotion.Extensions;
using R3;
using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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
            if (_comboText == null || _comboRoot == null)
            {
                Debug.LogError($"[{nameof(ComboHudView)}] {nameof(_comboText)} または {nameof(_comboRoot)} が未設定です。", this);
                return;
            }

            _comboDisposable?.Dispose();
            _comboHudViewModel = viewModel;

            _comboDisposable = _comboHudViewModel.ComboCount
                .Subscribe(comboCount =>
                  {
                      if (_comboText == null || _comboRoot == null) { return; }

                      _handle.TryComplete();
                      bool isVisible = comboCount >= comboVisibleCount;
                      _comboRoot.SetActive(isVisible);
                      if (!isVisible)
                      {
                          _comboText.SetText(string.Empty);
                          return;
                      }

                      _comboText.SetText("{0}", comboCount);
                      _handle = LSequence.Create()
                        .Join(LMotion.Punch.Create(0f, 5f, 0.1f)
                            .WithFrequency(Random.Range(2, 5))
                            .BindToAnchoredPositionX(_comboText.rectTransform))
                        .Join(LMotion.Punch.Create(0f, 5f, 0.1f)
                            .WithFrequency(Random.Range(2, 5))
                            .BindToAnchoredPositionY(_comboText.rectTransform))
                        .Run();
                  });
        }

        [SerializeField, Tooltip("コンボ数を表示するテキストです。")]
        private TextMeshProUGUI _comboText;
        [SerializeField, Tooltip("数値とCOMBOラベルをまとめて表示切替するルートです。")]
        private GameObject _comboRoot;
        private ComboHudViewModel _comboHudViewModel;
        private IDisposable _comboDisposable;
        private MotionHandle _handle;
        /// <summary>
        ///    ビューが破棄される際に購読を解除します。
        /// </summary>
        private void OnDestroy()
        {
            _handle.TryCancel();
            _comboDisposable?.Dispose();
        }
    }
}
