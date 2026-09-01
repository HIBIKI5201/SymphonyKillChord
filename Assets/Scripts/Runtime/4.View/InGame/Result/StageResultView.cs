using KillChord.Runtime.Adaptor.InGame.Result;
using LitMotion;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///    ステージリザルト画面の表示を制御するViewクラス。
    /// </summary>
    public class StageResultView : MonoBehaviour
    {
        /// <summary>
        ///    ViewModelとControllerを設定して初期化する。
        /// </summary>
        /// <param name="viewModel">ステージリザルトのViewModel。</param>
        /// <param name="controller">ステージリザルトのController。</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Initialize(StageResultViewModel viewModel, StageResultController controller)
        {
            UnsubscribeViewModel();
            ClearSubMissionItems();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            SubscribeViewModel();
            _isTransitioning = false;
            _isUiSlided = false;
        }

        /// <summary>
        ///     リザルト画面を表示する。
        /// </summary>
        public void Show()
        {
            HideInGameCanvas();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            _isTransitioning = false;
            _isUiSlided = false;

            SetInteractionEnabled(true);
            SelectButton(_completeButton);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;


            PlayTextSlideIn();
        }

        /// <summary>
        ///     リザルト画面を非表示にする。
        /// </summary>
        public void Hide()
        {
            StopTextSlideIn();
            StopCountUps(true);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            SetInteractionEnabled(false);
            ClearSelection();
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
                    RestoreInteraction(_completeButton);
                }
            }
            catch (Exception exception)
            {
                RestoreInteraction(_completeButton);

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
                    RestoreInteraction(_retryButton);
                }
            }
            catch (Exception exception)
            {
                RestoreInteraction(_retryButton);

                Debug.LogException(exception, this);
            }
        }

        private const int SECOND_PER_MINUTE = 60;

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

        [SerializeField, Tooltip("インゲームのCanvas。")]
        private Canvas[] _inGameCanvas;

        [Header("Button")]
        [SerializeField, Tooltip("ホームへ戻る完了ボタン。")]
        private Button _completeButton;

        [SerializeField, Tooltip("同じステージへ再出撃するボタン。")]
        private Button _retryButton;

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

        [Header("Animation")]
        [SerializeField, Tooltip("テキストを左から右方向へスライドインさせる演出の設定。")]
        private ResultTextSlideInSetting _textSlideIn = new();

        [SerializeField, Tooltip("スライドイン演出から除外するUI。指定した対象と、その配下のTextをまとめて除外する。")]
        private Transform[] _textSlideInExcludes;

        [SerializeField, Tooltip("数値をカウントアップ表示させる演出の設定。")]
        private ResultCountUpSetting _countUpSetting = new();

        private readonly Dictionary<TMP_Text, (float Value, Func<float, string> Formatter)> _pendingCountUpValues = new();
        private readonly Dictionary<TMP_Text, (MotionHandle Handle, float Value, Func<float, string> Formatter)> _countUpHandles = new();
        private readonly List<TMP_Text> _countUpKeysBuffer = new();
        private StageResultViewModel _viewModel;
        private StageResultController _controller;
        private bool _isTransitioning;
        private bool _isUiSlided;
        private IDisposable _stageNameDisposable;
        private IDisposable _mainMissionDisposable;
        private IDisposable _mainMissionStateDisposable;
        private IDisposable _battleTimeDisposable;
        private IDisposable _maxComboDisposable;
        private IDisposable _rankDisposable;
        private IDisposable _tipsDisposable;
        private IDisposable _resultTypeDisposable;
        private readonly List<StageResultMissionItemView> _spawnedSubMissionItems = new();
        private readonly List<MotionHandle> _slideInHandles = new();
        private readonly Dictionary<RectTransform, Vector2> _slideInOriginalPositions = new();
        private readonly List<TMP_Text> _slideInTexts = new();
        private readonly List<TMP_Text> _slideInTextBuffer = new();
        private readonly List<RectTransform> _releasedSlideInTargets = new();

        private void Awake()
        {
            Hide();
        }

        private void OnDestroy()
        {
            UnsubscribeViewModel();

            ResultTextSlideIn.Stop(_slideInHandles);
            StopCountUps(false);
        }

        /// <summary>
        ///     表示中のテキストを左から右方向へスライドインさせる。
        /// </summary>
        private void PlayTextSlideIn()
        {
            StopTextSlideIn();
            StopCountUps(true);
            _isUiSlided = false;

            if (_textSlideIn == null || !_textSlideIn.IsEnabled)
            {
                OnTextSlideInCompleted();
                return;
            }

            CollectSlideInTexts();

            if (_slideInTexts.Count == 0)
            {
                OnTextSlideInCompleted();
                return;
            }

            // 全テキストのスライドインが完了した時点でカウントアップを開始する。
            int remainingSlideIns = _slideInTexts.Count;

            for (int i = 0; i < _slideInTexts.Count; i++)
            {
                RectTransform rectTransform = _slideInTexts[i].rectTransform;

                ResultTextSlideIn.Play(
                    rectTransform,
                    GetSlideInOriginalPosition(rectTransform),
                    _textSlideIn,
                    i * _textSlideIn.Interval,
                    _slideInHandles,
                    () =>
                    {
                        remainingSlideIns--;

                        if (remainingSlideIns <= 0)
                        {
                            OnTextSlideInCompleted();
                        }
                    });
            }
        }

        /// <summary>
        ///     再生中のスライドインを停止し、本来の表示状態へ戻す。
        /// </summary>
        private void StopTextSlideIn()
        {
            ResultTextSlideIn.Stop(_slideInHandles);

            _releasedSlideInTargets.Clear();

            foreach (KeyValuePair<RectTransform, Vector2> entry in _slideInOriginalPositions)
            {
                // サブミッション項目は作り直されるため、破棄済みの対象を溜め込まない。
                if (entry.Key == null)
                {
                    _releasedSlideInTargets.Add(entry.Key);
                    continue;
                }

                entry.Key.TryGetComponent(out CanvasGroup canvasGroup);

                ResultTextSlideIn.ApplyEndState(entry.Key, entry.Value, canvasGroup);
            }

            for (int i = 0; i < _releasedSlideInTargets.Count; i++)
            {
                _slideInOriginalPositions.Remove(_releasedSlideInTargets[i]);
            }

            _releasedSlideInTargets.Clear();
        }

        /// <summary>
        ///     リザルト画面配下の表示中のTextを、画面上側から順に集める。
        /// </summary>
        private void CollectSlideInTexts()
        {
            _slideInTexts.Clear();

            Transform root = _canvasGroup != null ? _canvasGroup.transform : transform;

            root.GetComponentsInChildren(true, _slideInTextBuffer);

            for (int i = 0; i < _slideInTextBuffer.Count; i++)
            {
                TMP_Text text = _slideInTextBuffer[i];

                // 勝敗で片方のルートが非表示になるため、表示中のTextだけ動かす。
                if (text == null || !text.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (IsExcludedFromSlideIn(text.transform))
                {
                    continue;
                }

                if (IsLayoutControlled(text.rectTransform))
                {
                    continue;
                }

                _slideInTexts.Add(text);
            }

            // ヒエラルキー順は見た目の並びと一致しないため、画面上側から順に流す。
            _slideInTexts.Sort(CompareByScreenTopToBottom);
        }

        /// <summary>
        ///     スライドイン演出の除外指定に含まれるかを判定する。
        /// </summary>
        /// <param name="target"> 判定対象のTransform。 </param>
        /// <returns> 除外対象ならtrue。 </returns>
        private bool IsExcludedFromSlideIn(Transform target)
        {
            if (_textSlideInExcludes == null)
            {
                return false;
            }

            for (int i = 0; i < _textSlideInExcludes.Length; i++)
            {
                Transform exclude = _textSlideInExcludes[i];

                if (exclude == null)
                {
                    continue;
                }

                if (target.IsChildOf(exclude))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     親のLayoutGroupにanchoredPositionを制御されるかを判定する。
        ///     制御される要素を動かすとレイアウト更新と競合するため、演出対象から外す。
        /// </summary>
        /// <param name="rectTransform"> 判定対象のRectTransform。 </param>
        /// <returns> レイアウト制御下ならtrue。 </returns>
        private static bool IsLayoutControlled(RectTransform rectTransform)
        {
            return rectTransform.parent != null
                   && rectTransform.parent.TryGetComponent(out LayoutGroup _);
        }

        /// <summary>
        ///     画面上側のTextが先に来るように比較する。
        /// </summary>
        /// <param name="left"> 比較元のText。 </param>
        /// <param name="right"> 比較先のText。 </param>
        /// <returns> 並び順の比較結果。 </returns>
        private static int CompareByScreenTopToBottom(TMP_Text left, TMP_Text right)
        {
            return right.rectTransform.position.y.CompareTo(
                left.rectTransform.position.y);
        }

        /// <summary>
        ///     スライドインの終点となる本来のanchoredPositionを取得する。
        ///     演出で位置を書き換えるため、初回の値をそのまま保持し続ける。
        /// </summary>
        /// <param name="rectTransform"> 対象のRectTransform。 </param>
        /// <returns> 本来のanchoredPosition。 </returns>
        private Vector2 GetSlideInOriginalPosition(RectTransform rectTransform)
        {
            if (!_slideInOriginalPositions.TryGetValue(rectTransform, out Vector2 original))
            {
                original = rectTransform.anchoredPosition;

                _slideInOriginalPositions[rectTransform] = original;
            }

            return original;
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
                _viewModel.BattleTimeSeconds.Subscribe(
                    value => SetCountUp(_battleTimeText, value, FormatBattleTime));

            _maxComboDisposable =
                _viewModel.MaxCombo.Subscribe(
                    value => SetCountUp(_maxComboText, value, FormatMaxCombo));

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
        private void ApplyResultType(StageResultType resultType)
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

            // 表示後に一覧が差し替わった場合も、生成し直した項目を演出へ乗せる。
            if (_canvasGroup != null && _canvasGroup.alpha > 0f)
            {
                PlayTextSlideIn();
            }
        }

        /// <summary>
        ///    サブミッション一覧の表示をクリアする。
        /// </summary>
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
        /// <param name="focusTarget"> フォーカスを戻すボタン。 </param>
        private void RestoreInteraction(Button focusTarget)
        {
            _isTransitioning = false;

            SetInteractionEnabled(true);
            SelectButton(focusTarget);
        }

        /// <summary>
        ///     指定したボタンへEventSystemのフォーカスを移す。
        /// </summary>
        /// <param name="button"> フォーカス対象のボタン。 </param>
        private static void SelectButton(Button button)
        {
            EventSystem eventSystem = EventSystem.current;

            if (eventSystem == null || button == null || !button.IsActive() || !button.IsInteractable())
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(button.gameObject);
        }

        /// <summary>
        ///     リザルト画面内に残っているEventSystemの選択を解除する。
        /// </summary>
        private void ClearSelection()
        {
            EventSystem eventSystem = EventSystem.current;
            GameObject selectedObject = eventSystem != null
                ? eventSystem.currentSelectedGameObject
                : null;

            if (selectedObject == null || !selectedObject.transform.IsChildOf(transform))
            {
                return;
            }

            eventSystem.SetSelectedGameObject(null);
        }

        /// <summary>
        ///     InGameCanvasを閉じる。
        /// </summary>
        private void HideInGameCanvas()
        {
            if (_inGameCanvas == null)
            {
                return;
            }

            for (int i = 0; i < _inGameCanvas.Length; i++)
            {
                Canvas canvas = _inGameCanvas[i];

                if (canvas == null)
                {
                    continue;
                }

                canvas.gameObject.SetActive(false);
            }
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

        /// <summary>
        ///     数値をカウントアップ表示する。スライドイン演出が完了するまでは値を保留する。
        /// </summary>
        /// <param name="text"> 設定対象のText。 </param>
        /// <param name="value"> 表示する最終値。 </param>
        /// <param name="formatter"> 値を表示文字列へ変換する関数。 </param>
        private void SetCountUp(TMP_Text text, float value, Func<float, string> formatter)
        {
            if (text == null || formatter == null)
            {
                return;
            }

            if (!_isUiSlided)
            {
                _pendingCountUpValues[text] = (value, formatter);
                return;
            }

            PlayCountUp(text, value, formatter);
        }

        /// <summary>
        ///     0から目標値までカウントアップさせる。同じTextに対する再生中の演出は打ち切って上書きする。
        /// </summary>
        /// <param name="text"> 設定対象のText。 </param>
        /// <param name="targetValue"> カウントアップの最終値。 </param>
        /// <param name="formatter"> 値を表示文字列へ変換する関数。 </param>
        private void PlayCountUp(TMP_Text text, float targetValue, Func<float, string> formatter)
        {
            CancelCountUp(text, false);

            if (_countUpSetting == null || !_countUpSetting.IsEnabled || _countUpSetting.Duration <= 0f)
            {
                text.text = formatter(targetValue);
                return;
            }

            text.text = formatter(0f);

            MotionHandle handle = LMotion.Create(0f, targetValue, _countUpSetting.Duration)
                .WithEase(_countUpSetting.Ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => _countUpHandles.Remove(text))
                .Bind(currentValue => text.text = formatter(currentValue))
                .AddTo(text.gameObject);

            _countUpHandles[text] = (handle, targetValue, formatter);
        }

        /// <summary>
        ///     指定Textのカウントアップ演出を打ち切る。
        /// </summary>
        /// <param name="text"> 対象のText。 </param>
        /// <param name="snapToFinal"> 打ち切り時に最終値を表示へ反映するか。 </param>
        private void CancelCountUp(TMP_Text text, bool snapToFinal)
        {
            if (text == null || !_countUpHandles.TryGetValue(text, out var entry))
            {
                return;
            }

            entry.Handle.TryCancel();
            _countUpHandles.Remove(text);

            if (snapToFinal)
            {
                text.text = entry.Formatter(entry.Value);
            }
        }

        /// <summary>
        ///     再生中の全カウントアップ演出を打ち切る。
        /// </summary>
        /// <param name="snapToFinal"> 打ち切り時に各Textへ最終値を反映するか。 </param>
        private void StopCountUps(bool snapToFinal)
        {
            if (_countUpHandles.Count == 0)
            {
                return;
            }

            _countUpKeysBuffer.Clear();
            _countUpKeysBuffer.AddRange(_countUpHandles.Keys);

            for (int i = 0; i < _countUpKeysBuffer.Count; i++)
            {
                CancelCountUp(_countUpKeysBuffer[i], snapToFinal);
            }

            _countUpKeysBuffer.Clear();
        }

        /// <summary>
        ///     経過時間を「mm:ss」形式の文字列へ変換する。
        /// </summary>
        /// <param name="elapsedSeconds"> 経過時間（秒）。 </param>
        /// <returns> 「mm:ss」形式の文字列。 </returns>
        private static string FormatBattleTime(float elapsedSeconds)
        {
            // 経過時間は繰り上げずに切り捨てる。
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));

            int minutes = totalSeconds / SECOND_PER_MINUTE;
            int seconds = totalSeconds % SECOND_PER_MINUTE;

            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        ///     最大コンボ数を表示文字列へ変換する。
        /// </summary>
        /// <param name="value"> コンボ数。 </param>
        /// <returns> 表示文字列。 </returns>
        private static string FormatMaxCombo(float value)
        {
            return Mathf.Max(0, Mathf.RoundToInt(value)).ToString();
        }

        /// <summary>
        ///     スライドイン演出の完了を受けて、保留していたカウントアップを開始する。
        /// </summary>
        private void OnTextSlideInCompleted()
        {
            if (_isUiSlided)
            {
                return;
            }

            _isUiSlided = true;

            foreach (KeyValuePair<TMP_Text, (float Value, Func<float, string> Formatter)> pair in _pendingCountUpValues)
            {
                if (pair.Key != null)
                {
                    PlayCountUp(pair.Key, pair.Value.Value, pair.Value.Formatter);
                }
            }

            _pendingCountUpValues.Clear();
        }
    }
}
