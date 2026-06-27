using KillChord.Runtime.Adaptor.InGame.Result;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Result
{
    public class StageResultView : MonoBehaviour
    {
        public void Initialize(StageResultViewModel viewModel, StageResultController controller)
        {
            UnsubscribeViewModel();
            ClearSubMissionItems();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            SubscribeViewModel();
            _isTransitioning = false;
        }

        /// <summary>
        ///     リザルト画面を表示する。
        /// </summary>
        public void Show()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            _isTransitioning = false;

            SetInteractionEnabled(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        ///     リザルト画面を非表示にする。
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            SetInteractionEnabled(false);
        }

        /// <summary>
        ///     完了ボタン押下時の処理。
        ///     ButtonのUnityEventから呼び出す。
        /// </summary>
        public async void OnCompleteButtonClicked()
        {
            if (_isTransitioning || _controller == null)
            {
                return;
            }

            _isTransitioning = true;

            SetInteractionEnabled(false);

            try
            {
                bool success = await _controller.CompleteAsync();

                if (!success)
                {
                    RestoreInteraction();
                }
            }
            catch (Exception exception)
            {
                RestoreInteraction();

                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     再出撃ボタン押下時の処理。
        ///     ButtonのUnityEventから呼び出す。
        /// </summary>
        public async void OnRetryButtonClicked()
        {
            if (_isTransitioning || _controller == null)
            {
                return;
            }

            _isTransitioning = true;

            SetInteractionEnabled(false);

            try
            {
                bool success = await _controller.RetryAsync();

                if (!success)
                {
                    RestoreInteraction();
                }
            }
            catch (Exception exception)
            {
                RestoreInteraction();

                Debug.LogException(exception, this);
            }
        }

        [Header("Root")]
        [SerializeField, Tooltip("リザルト画面全体を制御するCanvasGroup。")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("勝利時に表示するUIルート。")]
        private GameObject _victoryRoot;

        [SerializeField, Tooltip("敗北時に表示するUIルート。")]
        private GameObject _defeatRoot;

        [SerializeField, Tooltip("サブミッション一覧を表示するUIルート。")]
        private GameObject _subMissionRoot;

        [SerializeField, Tooltip("ランクを表示するUIルート。")]
        private GameObject _rankRoot;

        [SerializeField, Tooltip("敗北時のTipsを表示するUIルート。")]
        private GameObject _tipsRoot;

        [Header("Text")]
        [SerializeField, Tooltip("リザルトタイトルを表示するText。")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("挑戦したステージ名を表示するText。")]
        private TMP_Text _stageNameText;

        [SerializeField, Tooltip("メインミッションを表示するText。")]
        private TMP_Text _mainMissionText;

        [SerializeField, Tooltip("メインミッションの達成状態を表示するText。")]
        private TMP_Text _mainMissionStateText;

        [SerializeField, Tooltip("戦闘時間を表示するText。")]
        private TMP_Text _battleTimeText;

        [SerializeField, Tooltip("最大コンボ数を表示するText。")]
        private TMP_Text _maxComboText;

        [SerializeField, Tooltip("リザルトランクを表示するText。")]
        private TMP_Text _rankText;

        [SerializeField, Tooltip("敗北時の攻略Tipsを表示するText。")]
        private TMP_Text _tipsText;

        [Header("Sub Mission")]
        [SerializeField, Tooltip("サブミッション項目を生成する親Transform。")]
        private Transform _subMissionContentRoot;

        [SerializeField, Tooltip("サブミッション1件分の表示Prefab。")]
        private StageResultMissionItemView _subMissionItemPrefab;

        [Header("Message")]
        [SerializeField, Tooltip("勝利時に表示するリザルトタイトル。")]
        private string _victoryTitle = "Mission Complete";

        [SerializeField, Tooltip("敗北時に表示するリザルトタイトル。")]
        private string _defeatTitle = "Mission Failed";

        private StageResultViewModel _viewModel;
        private StageResultController _controller;
        private bool _isTransitioning;
        private IDisposable _stageNameDisposable;
        private IDisposable _mainMissionDisposable;
        private IDisposable _mainMissionStateDisposable;
        private IDisposable _battleTimeDisposable;
        private IDisposable _maxComboDisposable;
        private IDisposable _rankDisposable;
        private IDisposable _tipsDisposable;
        private IDisposable _resultTypeDisposable;
        private readonly List<StageResultMissionItemView> _spawnedSubMissionItems = new();

        private void Awake()
        {
            Hide();
        }

        private void OnDestroy()
        {
            UnsubscribeViewModel();
        }


        /// <summary>
        ///     ViewModelが保持する表示値を購読する。
        /// </summary>
        private void SubscribeViewModel()
        {
            _stageNameDisposable =
                _viewModel.StageNameText.Subscribe(
                    value => SetText(_stageNameText, value));

            _mainMissionDisposable =
                _viewModel.MainMissionText.Subscribe(
                    value => SetText(_mainMissionText, value));

            _mainMissionStateDisposable =
                _viewModel.MainMissionStateText.Subscribe(
                    value => SetText(_mainMissionStateText, value));

            _battleTimeDisposable =
                _viewModel.BattleTimeText.Subscribe(
                    value => SetText(_battleTimeText, value));

            _maxComboDisposable =
                _viewModel.MaxComboText.Subscribe(
                    value => SetText(_maxComboText, value));

            _rankDisposable =
                _viewModel.RankText.Subscribe(
                    value => SetText(_rankText, value));

            _tipsDisposable =
                _viewModel.TipsText.Subscribe(
                    value => SetText(_tipsText, value));

            _resultTypeDisposable =
                _viewModel.ResultType.Subscribe(
                    ApplyResultType);

            _viewModel.OnSubMissionItemsUpdated +=
                RebuildSubMissionItems;
        }

        /// <summary>
        ///     ViewModelの購読を解除する。
        /// </summary>
        private void UnsubscribeViewModel()
        {
            _stageNameDisposable?.Dispose();
            _mainMissionDisposable?.Dispose();
            _mainMissionStateDisposable?.Dispose();
            _battleTimeDisposable?.Dispose();
            _maxComboDisposable?.Dispose();
            _rankDisposable?.Dispose();
            _tipsDisposable?.Dispose();
            _resultTypeDisposable?.Dispose();

            _stageNameDisposable = null;
            _mainMissionDisposable = null;
            _mainMissionStateDisposable = null;
            _battleTimeDisposable = null;
            _maxComboDisposable = null;
            _rankDisposable = null;
            _tipsDisposable = null;
            _resultTypeDisposable = null;

            if (_viewModel != null)
            {
                _viewModel.OnSubMissionItemsUpdated -= RebuildSubMissionItems;
            }
        }

        /// <summary>
        ///     リザルトの種類に応じて表示するUIを切り替える。
        /// </summary>
        /// <param name="resultType">
        ///     勝利または敗北のリザルト種別。
        /// </param>
        private void ApplyResultType(
            StageResultType resultType)
        {
            bool isVictory =
                resultType == StageResultType.Victory;

            _victoryRoot.SetActive(isVictory);
            _defeatRoot.SetActive(!isVictory);
            _subMissionRoot.SetActive(isVictory);
            _rankRoot.SetActive(isVictory);
            _tipsRoot.SetActive(!isVictory);

            SetText(_titleText, isVictory ? _victoryTitle : _defeatTitle);
        }

        /// <summary>
        ///     サブミッション一覧の表示を再構築する。
        /// </summary>
        /// <param name="items"> 表示するサブミッションViewModel一覧。 </param>
        private void RebuildSubMissionItems(
            IReadOnlyList<StageResultMissionItemViewModel> items)
        {
            ClearSubMissionItems();

            if (items == null || _subMissionContentRoot == null || _subMissionItemPrefab == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                StageResultMissionItemView itemView =
                    Instantiate(_subMissionItemPrefab, _subMissionContentRoot);

                itemView.Apply(items[i]);

                _spawnedSubMissionItems.Add(itemView);
            }
        }

        private void ClearSubMissionItems()
        {
            for (int i = 0; i < _spawnedSubMissionItems.Count; i++)
            {
                StageResultMissionItemView itemView = _spawnedSubMissionItems[i];

                if (itemView == null)
                {
                    continue;
                }

                Destroy(itemView.gameObject);
            }

            _spawnedSubMissionItems.Clear();
        }

        /// <summary>
        ///     シーン遷移失敗後にリザルト画面の操作を復帰する。
        /// </summary>
        private void RestoreInteraction()
        {
            _isTransitioning = false;

            SetInteractionEnabled(true);
        }

        /// <summary>
        ///     リザルト画面全体の操作可否を変更する。
        /// </summary>
        /// <param name="isEnabled"> 操作可能にする場合はtrue。 </param>
        private void SetInteractionEnabled(bool isEnabled)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = isEnabled;
            _canvasGroup.blocksRaycasts = isEnabled;
        }

        /// <summary>
        ///     Textへ表示文を設定する。
        /// </summary>
        /// <param name="text"> 設定対象のText。 </param>
        /// <param name="value"> 表示する文字列。 </param>
        private static void SetText(TMP_Text text, string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value ?? string.Empty;
        }
    }
}
