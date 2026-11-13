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
    [AddComponentMenu("KillChord/View/Combo/ComboHudView")]
    public class ComboHudView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="viewModel"> 購読するコンボHUDのViewModelです。 </param>
        /// <param name="comboVisibleCount"> コンボ表示を開始する最小コンボ数です。 </param>
        /// <param name="isTierEffectEnabled"> コンボ数の段階に合わせて色と演出を変える場合はtrueです。 </param>
        public void Initialize(ComboHudViewModel viewModel, int comboVisibleCount, bool isTierEffectEnabled = false)
        {
            // 表示に必要な参照を確認する。
            if (_comboText == null)
            {
                Debug.LogError($"[{nameof(ComboHudView)}] {nameof(_comboText)}が未設定です。", this);
                return;
            }

            _comboRoot = _comboText.rectTransform.parent?.gameObject;
            if (_comboRoot == null)
            {
                Debug.LogError($"[{nameof(ComboHudView)}] コンボ表示ルートを取得できません。", this);
                return;
            }

            // 以前の購読を解除してから、コンボ数の変化を購読する。
            _comboDisposable?.Dispose();
            _comboHudViewModel = viewModel;

            _comboDisposable = _comboHudViewModel.ComboCount
                .Subscribe(comboCount =>
                  {
                      if (_comboText == null || _comboRoot == null) { return; }

                      _handle.TryComplete();
                      // 表示する最小のコンボ数に満たない間は非表示にする。
                      bool isVisible = comboCount >= comboVisibleCount;
                      _comboRoot.SetActive(isVisible);
                      if (!isVisible)
                      {
                          _comboText.SetText(string.Empty);
                          return;
                      }

                      // コンボ数を表示し、文字を揺らす演出を再生する。
                      _comboText.SetText("{0}", comboCount);
                      if (isTierEffectEnabled && TryPlayTierEffect(comboCount))
                      {
                          return;
                      }

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

        private const float TIER_PUNCH_SCALE = 0.6f;
        private const float TIER_PUNCH_DURATION = 0.25f;
        private const int TIER_PUNCH_FREQUENCY = 3;

        [SerializeField, Tooltip("コンボ数を表示するテキストです。")]
        private TextMeshProUGUI _comboText;

        [SerializeField, Tooltip("コンボの段階が上がるコンボ数（小さい順）。Justのときだけ増えるコンボの試遊で使う。")]
        private int[] _tierThresholds = { 10, 25, 50 };

        [SerializeField, Tooltip("段階ごとの文字の色。先頭が段階に届く前の色で、以降は段階の順。")]
        private Color[] _tierColors =
        {
            Color.white,
            new Color(0.55f, 0.9f, 1f),
            new Color(1f, 0.85f, 0.35f),
            new Color(1f, 0.45f, 0.85f),
        };

        private int _currentTier;
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

        /// <summary>
        ///     コンボ数の段階に合わせて文字の色を変え、段階が上がったときは大きく弾ませる。
        /// </summary>
        /// <param name="comboCount"> 今のコンボ数です。 </param>
        /// <returns> 段階が上がって演出を再生した場合はtrue。 </returns>
        private bool TryPlayTierEffect(int comboCount)
        {
            int tier = CalculateTier(comboCount);
            bool isTierUp = tier > _currentTier;
            _currentTier = tier;
            if (_tierColors != null && _tierColors.Length > 0)
            {
                _comboText.color = _tierColors[Mathf.Min(tier, _tierColors.Length - 1)];
            }

            if (!isTierUp)
            {
                return false;
            }

            // 段階に届いたことが分かるよう、通常の揺れより大きく拡大して弾ませる。
            _handle = LMotion.Punch.Create(Vector3.one, Vector3.one * TIER_PUNCH_SCALE, TIER_PUNCH_DURATION)
                .WithFrequency(TIER_PUNCH_FREQUENCY)
                .BindToLocalScale(_comboText.rectTransform);
            return true;
        }

        /// <summary>
        ///     コンボ数がどの段階にあるかを求める。
        /// </summary>
        /// <param name="comboCount"> 今のコンボ数です。 </param>
        /// <returns> 届いた段階の数です。どの段階にも届いていなければ0です。 </returns>
        private int CalculateTier(int comboCount)
        {
            if (_tierThresholds == null)
            {
                return 0;
            }

            int tier = 0;
            for (int i = 0; i < _tierThresholds.Length; i++)
            {
                if (comboCount >= _tierThresholds[i])
                {
                    tier = i + 1;
                }
            }

            return tier;
        }
    }
}
