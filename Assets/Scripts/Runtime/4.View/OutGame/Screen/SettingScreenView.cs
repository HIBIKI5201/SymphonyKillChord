using KillChord.Runtime.View.OutGame.Navigation;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     設定画面 View です。
    /// </summary>
    public sealed class SettingScreenView : ScreenViewBase
    {

        /// <summary> View を初期化します。 </summary>
        public SettingScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(SettingScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            _returnToTitleButton = Require<Button>(rootElement, RETURN_TO_TITLE_BUTTON_NAME);
            _soundCategoryButton = Require<Button>(rootElement, SOUND_CATEGORY_BUTTON_NAME);
            _cancelReturnToTitleButton = Require<Button>(rootElement, CANCEL_RETURN_BUTTON_NAME);
            _confirmReturnToTitleButton = Require<Button>(rootElement, CONFIRM_RETURN_BUTTON_NAME);
            _returnToTitleDialog = Require<VisualElement>(rootElement, RETURN_TO_TITLE_DIALOG_NAME);
            _outsideClickArea = Require<VisualElement>(rootElement, OUTSIDE_CLICK_AREA_NAME);

            RegisterButtonCallback();
            ResetReturnToTitleDialog();
        }

        /// <summary>
        ///     確認状態を初期化して設定画面を表示する。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            ResetReturnToTitleDialog();

            // 背面のホーム画面は表示されたままのため、フォーカスを設定画面内へ閉じ込める。
            _screenNavigationScope.Activate(RootElement);
            return base.Show(cancellationToken);
        }

        /// <summary>
        ///     フォーカスの閉じ込めを解除して設定画面を閉じる。
        /// </summary>
        public override ValueTask Hide(CancellationToken cancellationToken = default)
        {
            _dialogNavigationScope.Deactivate();
            _screenNavigationScope.Deactivate();
            return base.Hide(cancellationToken);
        }

        /// <summary>
        ///     登録済みコールバックを解除する。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _returnToTitleButton.clicked += ShowReturnToTitleDialog;
            _cancelReturnToTitleButton.clicked += HideReturnToTitleDialog;
            _confirmReturnToTitleButton.clicked += RequestReturnToTitle;
            OutGameUIEvent.OnReturnToTitleRequestCompleted += HandleReturnToTitleRequestCompleted;
            _outsideClickArea.RegisterCallback<PointerDownEvent>(HandleOutsidePointerDown);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _returnToTitleButton.clicked -= ShowReturnToTitleDialog;
            _cancelReturnToTitleButton.clicked -= HideReturnToTitleDialog;
            _confirmReturnToTitleButton.clicked -= RequestReturnToTitle;
            OutGameUIEvent.OnReturnToTitleRequestCompleted -= HandleReturnToTitleRequestCompleted;
            _outsideClickArea.UnregisterCallback<PointerDownEvent>(HandleOutsidePointerDown);
        }

        /// <summary>
        ///     画面を閉じるボタンがクリックされたときの処理です。
        /// </summary>
        private void OnBackButtonClicked(ClickEvent evt)
        {
            if (_isReturnToTitleDialogVisible || _isReturnToTitleRequested)
            {
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        private const string BACKBUTTON_NAME = "BackButton";
        private const string SOUND_CATEGORY_BUTTON_NAME = "SoundCategoryButton";
        private const string RETURN_TO_TITLE_BUTTON_NAME = "ReturnToTitleButton";
        private const string CANCEL_RETURN_BUTTON_NAME = "CancelReturnToTitleButton";
        private const string CONFIRM_RETURN_BUTTON_NAME = "ConfirmReturnToTitleButton";
        private const string RETURN_TO_TITLE_DIALOG_NAME = "ReturnToTitleDialog";
        private const string OUTSIDE_CLICK_AREA_NAME = "Root";

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _soundCategoryButton;

        /// <summary> 設定画面表示中、フォーカスを画面内へ閉じ込める。 </summary>
        private readonly ModalNavigationScope _screenNavigationScope = new();
        /// <summary> 確認ダイアログ表示中、フォーカスをダイアログ内へ閉じ込める。 </summary>
        private readonly ModalNavigationScope _dialogNavigationScope = new();

        private readonly Button _backButton;
        private readonly Button _soundCategoryButton;
        private readonly Button _returnToTitleButton;
        private readonly Button _cancelReturnToTitleButton;
        private readonly Button _confirmReturnToTitleButton;
        private readonly VisualElement _returnToTitleDialog;
        private readonly VisualElement _outsideClickArea;
        private bool _isReturnToTitleDialogVisible;
        private bool _isReturnToTitleRequested;

        /// <summary>
        ///     設定ウィンドウ外が押された場合に設定画面を閉じる。
        /// </summary>
        private void HandleOutsidePointerDown(PointerDownEvent pointerEvent)
        {
            if (!ReferenceEquals(pointerEvent.target, _outsideClickArea)
                || _isReturnToTitleDialogVisible
                || _isReturnToTitleRequested)
            {
                return;
            }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     タイトル復帰の確認ダイアログを表示する。
        /// </summary>
        private void ShowReturnToTitleDialog()
        {
            if (_isReturnToTitleRequested)
            {
                return;
            }

            _isReturnToTitleDialogVisible = true;
            _returnToTitleDialog.style.display = DisplayStyle.Flex;
            _confirmReturnToTitleButton.SetEnabled(true);

            // 背面の設定項目へフォーカスが抜けないようにする。
            _dialogNavigationScope.Activate(_returnToTitleDialog);
            _cancelReturnToTitleButton.Focus();
        }

        /// <summary>
        ///     タイトル復帰の確認ダイアログを閉じる。
        /// </summary>
        private void HideReturnToTitleDialog()
        {
            if (_isReturnToTitleRequested)
            {
                return;
            }

            _isReturnToTitleDialogVisible = false;
            _dialogNavigationScope.Deactivate();
            _returnToTitleDialog.style.display = DisplayStyle.None;
            _returnToTitleButton.Focus();
        }

        /// <summary>
        ///     タイトル画面への復帰を要求する。
        /// </summary>
        private void RequestReturnToTitle()
        {
            if (!_isReturnToTitleDialogVisible || _isReturnToTitleRequested)
            {
                return;
            }

            _isReturnToTitleRequested = true;
            _confirmReturnToTitleButton.SetEnabled(false);
            _cancelReturnToTitleButton.SetEnabled(false);
            OutGameUIEvent.OnReturnToTitleRequested?.Invoke();
        }

        /// <summary>
        ///     タイトル復帰失敗時に確認UIを再操作可能へ戻す。
        /// </summary>
        private void HandleReturnToTitleRequestCompleted(bool isSucceeded)
        {
            if (isSucceeded)
            {
                return;
            }

            _isReturnToTitleRequested = false;
            _confirmReturnToTitleButton.SetEnabled(true);
            _cancelReturnToTitleButton.SetEnabled(true);
            _confirmReturnToTitleButton.Focus();
        }

        /// <summary>
        ///     タイトル復帰の確認状態と操作可否を初期状態へ戻す。
        /// </summary>
        private void ResetReturnToTitleDialog()
        {
            _isReturnToTitleDialogVisible = false;
            _isReturnToTitleRequested = false;
            _returnToTitleDialog.style.display = DisplayStyle.None;
            _confirmReturnToTitleButton.SetEnabled(true);
            _cancelReturnToTitleButton.SetEnabled(true);
        }

        /// <summary>
        ///     必須UI要素を取得する。
        /// </summary>
        private static T Require<T>(VisualElement rootElement, string elementName)
            where T : VisualElement
        {
            return rootElement.Q<T>(elementName)
                ?? throw new System.InvalidOperationException(
                    $"[{nameof(SettingScreenView)}] {elementName} が見つかりませんでした。");
        }
    }
}
