
using KillChord.Runtime.View.OutGame.Navigation;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     研究画面 View。
    /// </summary>
    public sealed class SkillTreeScreenView : ScreenViewBase
    {

        /// <summary> View を初期化します。 </summary>
        public SkillTreeScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACKBUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(SkillTreeScreenView)}] {BACKBUTTON_NAME} が見つかりませんでした。");

            // ツリーは下から上へ伸びるため、スクロール位置の初期化に使う。
            _treeScrollView = rootElement.Q<ScrollView>(TREE_SCROLL_VIEW_NAME);

            RegisterButtonCallback();
        }

        /// <summary>
        ///     画面を表示します。ツリーは下から上へ伸びるため、最下部から見せます。
        /// </summary>
        public override System.Threading.Tasks.ValueTask Show(
            System.Threading.CancellationToken cancellationToken = default)
        {
            ScrollToBottom();
            return base.Show(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     ツリーのスクロール位置を最下部へ移動します。
        /// </summary>
        /// <remarks>
        ///     contentContainer のレイアウトが確定するまで最大スクロール量が determined しないため、
        ///     レイアウト確定後に実行します。
        /// </remarks>
        private void ScrollToBottom()
        {
            if (_treeScrollView == null)
            {
                return;
            }

            _treeScrollView.schedule.Execute(() =>
            {
                if (_treeScrollView.panel == null)
                {
                    return;
                }

                float maxOffset = _treeScrollView.contentContainer.layout.height
                    - _treeScrollView.contentViewport.layout.height;
                _treeScrollView.scrollOffset = new UnityEngine.Vector2(
                    _treeScrollView.scrollOffset.x,
                    maxOffset > 0f ? maxOffset : 0f);
            });
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButtonActivation?.Dispose();
        }

        /// <summary>
        ///     画面を閉じるボタンが作動したときの処理です。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        private const string BACKBUTTON_NAME = "BackButton";

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        /// <inheritdoc />
        /// <remarks> 起点ノード(ツリー最下部)が無い場合は戻るボタンへフォールバックします。 </remarks>
        protected override VisualElement InitialFocusElement =>
            RootElement.Q<VisualElement>(className: UINavigationExtensions.INITIAL_FOCUS_CLASS_NAME)
            ?? _backButton;

        private const string TREE_SCROLL_VIEW_NAME = "SkillTreeContainer";

        private readonly Button _backButton;
        private readonly ScrollView _treeScrollView;
        private IDisposable _backButtonActivation;
    }
}
