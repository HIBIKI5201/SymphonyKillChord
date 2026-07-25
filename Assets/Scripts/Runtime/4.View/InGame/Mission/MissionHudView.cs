using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

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

            // 既存のViewModelがある場合だけイベント解除
            if (_viewModel != null)
            {
                _viewModel.OnEvaluationItemsUpdated -= ReBuildEvaluationItems;
            }

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
        [SerializeField, Min(1), Tooltip("評価項目プールの初期生成数。")] private int _evaluationItemPoolDefaultCapacity = 4;
        [SerializeField, Min(1), Tooltip("評価項目プールの最大保持数。")] private int _evaluationItemPoolMaxSize = 16;

        /// <summary> ビューモデル。 </summary>
        private MissionHudViewModel _viewModel;
        /// <summary> メインミッションテキスト購読解除用。 </summary>
        private IDisposable _mainMissionDisposable;
        /// <summary> 結果テキスト購読解除用。 </summary>
        private IDisposable _resultDisposable;
        /// <summary> 表示中の評価項目のリスト。 </summary>
        private readonly List<MissionEvaluationItemView> _spawnedEvaluationItems = new();
        /// <summary> 評価項目のオブジェクトプール。 </summary>
        private IObjectPool<MissionEvaluationItemView> _evaluationItemPool;

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

            _evaluationItemPool?.Clear();
        }

        /// <summary>
        ///     評価項目を再構築します。
        /// </summary>
        /// <param name="items">評価項目のリスト。</param>
        private void ReBuildEvaluationItems(IReadOnlyList<MissionEvaluationItemViewModel> items)
        {
            ReleaseEvaluationItems();

            if (items == null || _evaluationRoot == null || _evaluationItemPrefab == null)
            {
                return;
            }

            EnsureEvaluationItemPoolInitialized();

            for (int i = 0; i < items.Count; i++)
            {
                MissionEvaluationItemView view = _evaluationItemPool.Get();

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
        ///     表示中の評価項目をすべてプールへ戻します。
        /// </summary>
        private void ReleaseEvaluationItems()
        {
            foreach (MissionEvaluationItemView item in _spawnedEvaluationItems)
            {
                if (item != null && _evaluationItemPool != null)
                {
                    _evaluationItemPool.Release(item);
                }
            }

            _spawnedEvaluationItems.Clear();
        }

        /// <summary>
        ///     評価項目プールを必要時に初期化します。
        /// </summary>
        private void EnsureEvaluationItemPoolInitialized()
        {
            if (_evaluationItemPool != null)
            {
                return;
            }

            _evaluationItemPool = new ObjectPool<MissionEvaluationItemView>(
                CreateEvaluationItem,
                OnGetEvaluationItem,
                OnReleaseEvaluationItem,
                OnDestroyEvaluationItem,
                true,
                _evaluationItemPoolDefaultCapacity,
                _evaluationItemPoolMaxSize);
        }

        /// <summary>
        ///     新しい評価項目を生成します。
        /// </summary>
        /// <returns> 生成した評価項目のビュー。 </returns>
        private MissionEvaluationItemView CreateEvaluationItem()
        {
            return Instantiate(_evaluationItemPrefab, _evaluationRoot);
        }

        /// <summary>
        ///     プールから取り出した評価項目を有効化します。
        /// </summary>
        /// <param name="view"> 対象の評価項目ビュー。 </param>
        private void OnGetEvaluationItem(MissionEvaluationItemView view)
        {
            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(true);
        }

        /// <summary>
        ///     プールへ戻す評価項目を非表示にします。
        /// </summary>
        /// <param name="view"> 対象の評価項目ビュー。 </param>
        private void OnReleaseEvaluationItem(MissionEvaluationItemView view)
        {
            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(false);
        }

        /// <summary>
        ///     プール上限を超えた評価項目を破棄します。
        /// </summary>
        /// <param name="view"> 対象の評価項目ビュー。 </param>
        private void OnDestroyEvaluationItem(MissionEvaluationItemView view)
        {
            if (view == null)
            {
                return;
            }

            Destroy(view.gameObject);
        }
    }
}
