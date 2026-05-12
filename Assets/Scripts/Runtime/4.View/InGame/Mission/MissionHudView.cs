using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     ミッションHUDの表示を制御するビュークラス。
    /// </summary>
    public class MissionHudView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="viewModel">ミッションHUDのビューモデル。</param>
        public void Initialize(MissionHudViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            // 既存の購読を破棄
            _mainMissionDisposable?.Dispose();
            _resultDisposable?.Dispose();

            _viewModel.OnEvaluationItemsUpdated -= ReBuildEvaluationItems;

            _viewModel = viewModel;

            _mainMissionDisposable = viewModel.MainMissionText.Subscribe(value =>
            {
                if (_mainMissionText != null)
                {
                    _mainMissionText.text = value;
                }
            });

            _resultDisposable = viewModel.ResultText.Subscribe(value =>
            {
                if (_resultText != null)
                {
                    _resultText.text = value;
                }
            });

            _viewModel.OnEvaluationItemsUpdated += ReBuildEvaluationItems;
        }

        [Header("メインミッション表示用UI")]
        [SerializeField, Tooltip("メインミッション表示用のテキスト。")] private TMP_Text _mainMissionText;
        [SerializeField, Tooltip("ミッション結果表示用のテキスト。")] private TMP_Text _resultText;

        [Header("評価ミッション表示用UI")]
        [SerializeField, Tooltip("評価項目の親となるRectTransform。")] private RectTransform _evaluationRoot;
        [SerializeField, Tooltip("評価項目のプレハブ。")] private MissionEvaluationItemView _evaluationItemPrefab;
        [SerializeField, Tooltip("評価項目の垂直方向の間隔。")] private float _evaluationItemSpacing;

        /// <summary> ビューモデル。 </summary>
        private MissionHudViewModel _viewModel;
        /// <summary> メインミッションテキスト購読解除用。 </summary>
        private IDisposable _mainMissionDisposable;
        /// <summary> 結果テキスト購読解除用。 </summary>
        private IDisposable _resultDisposable;
        /// <summary> 生成された評価項目のリスト。 </summary>
        private readonly List<MissionEvaluationItemView> _spawnedEvaluationItems = new();

        /// <summary>
        ///     破棄時の処理を行います。
        /// </summary>
        private void OnDestroy()
        {
            _mainMissionDisposable?.Dispose();
            _resultDisposable?.Dispose();

            if (_viewModel != null)
            {
                _viewModel.OnEvaluationItemsUpdated -= ReBuildEvaluationItems;
            }
        }

        /// <summary>
        ///     評価項目を再構築します。
        /// </summary>
        /// <param name="items">評価項目のリスト。</param>
        private void ReBuildEvaluationItems(IReadOnlyList<MissionEvaluationItemViewModel> items)
        {
            ClearEvaluationItems();

            if (items == null || _evaluationRoot == null || _evaluationItemPrefab == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                MissionEvaluationItemView view = Instantiate(_evaluationItemPrefab, _evaluationRoot);

                RectTransform rectTransform = view.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(0, -i * _evaluationItemSpacing);
                }

                view.Apply(items[i]);
                _spawnedEvaluationItems.Add(view);
            }
        }

        /// <summary>
        ///     表示中の評価項目をクリアします。
        /// </summary>
        private void ClearEvaluationItems()
        {
            foreach (var item in _spawnedEvaluationItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            _spawnedEvaluationItems.Clear();
        }
    }
}
