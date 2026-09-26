using KillChord.Runtime.View.Persistent.Localization;

using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.SkillTree;
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
            _settingShortcutButton = rootElement.Q<Button>(SETTING_SHORTCUT_BUTTON_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(SkillTreeScreenView)}] {SETTING_SHORTCUT_BUTTON_NAME} が見つかりませんでした。");

            // ツリーは下から上へ伸びるため、スクロール位置の初期化に使う。
            _treeScrollView = rootElement.Q<ScrollView>(TREE_SCROLL_VIEW_NAME);
            if (_treeScrollView != null)
            {
                _dragScrollManipulator = new ScrollViewDragManipulator(_treeScrollView);
            }

            RegisterButtonCallback();
            _pointsLabel = rootElement.Q<Label>("Points");
            _rebuildPointsLabel = rootElement.Q<Label>("RebuildPointsValueLabel")
                ?? throw new ArgumentNullException($"[{nameof(SkillTreeScreenView)}] RebuildPointsValueLabel が見つかりませんでした。");
            _listSeparatorLocalizedText = new LocalizedElementText(
                "UICommon", "ui.skill_tree.list_separator", text =>
                {
                    ListSeparator = text;
                    OnListSeparatorChanged?.Invoke();
                }, "、");
            Label localizedRebuildPointsHeading = rootElement.Q<Label>("RebuildPointsNameLabel");
            Label localizedUnlockPointsHeading = rootElement.Q<Label>("UnlockPointsNameLabel");
            _headingLocalizedTexts = new[]
            {
                new LocalizedElementText("UICommon", "ui.home.mod_points", text => localizedRebuildPointsHeading.text = text, localizedRebuildPointsHeading.text),
                new LocalizedElementText("UICommon", "ui.home.unlock_points", text => localizedUnlockPointsHeading.text = text, localizedUnlockPointsHeading.text)
            };
        }

        /// <summary>
        ///     画面を表示します。ツリーは下から上へ伸びるため最下部から見せ、
        ///     横方向はコンテンツ幅の中央を初期位置とします。
        /// </summary>
        public override System.Threading.Tasks.ValueTask Show(
            System.Threading.CancellationToken cancellationToken = default)
        {
            ScrollToInitialPosition();
            return base.Show(cancellationToken);
        }

        public override void Dispose()
        {
            _listSeparatorLocalizedText.Dispose();
            OnListSeparatorChanged = null;
            foreach (LocalizedElementText localizedText in _headingLocalizedTexts)
            {
                localizedText.Dispose();
            }
            base.Dispose();
            UnregisterButtonCallback();

            if (_dragScrollManipulator != null)
            {
                _dragScrollManipulator.target = null;
                _dragScrollManipulator = null;
            }
        }

        /// <summary>
        ///     ツリーのスクロール位置を初期状態(縦：最下部、横：中央)へ移動します。
        ///     初期フォーカス対象が見つかった場合は、この後SkillTreeViewportViewが
        ///     対象ノードへ位置を上書きします。
        /// </summary>
        /// <remarks>
        ///     contentContainer のレイアウトが確定するまで最大スクロール量が determined しないため、
        ///     レイアウト確定後に実行します。
        /// </remarks>
        private void ScrollToInitialPosition()
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

                float maxOffsetY = _treeScrollView.contentContainer.layout.height
                    - _treeScrollView.contentViewport.layout.height;
                float maxOffsetX = _treeScrollView.contentContainer.layout.width
                    - _treeScrollView.contentViewport.layout.width;
                _treeScrollView.scrollOffset = new UnityEngine.Vector2(
                    maxOffsetX > 0f ? maxOffsetX * 0.5f : 0f,
                    maxOffsetY > 0f ? maxOffsetY : 0f);
            });
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _settingShortcutButton.MakeNavigable();
            // Button.clicked/ClickEventはコントローラーの決定操作(NavigationSubmitEvent)には反応しないため、
            // MakeNavigable() とあわせて RegisterActivation() でクリックと決定操作を1つの処理へ統合する。
            _settingShortcutButtonActivation =
                _settingShortcutButton.RegisterActivation(HandleSettingShortcutButtonActivationHandler);
            // 画面左端のフォーカス移動チェーン(ツリー→設定→戻る)の終端として使うため、
            // キャンセル操作で戻れる画面だがフォーカス移動の対象に含める。
            _backButton.MakeNavigable();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButtonActivation?.Dispose();
            _settingShortcutButtonActivation?.Dispose();
        }

        /// <summary>
        ///     画面を閉じるボタンが作動したときの処理です。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     設定画面ショートカットボタンが作動したときの処理です。
        /// </summary>
        private void HandleSettingShortcutButtonActivationHandler()
        {
            OutGameUIEvent.OnShownSettingScreen?.Invoke();
        }

        /// <summary> スキル名一覧の区切り文字が変更された時に通知する。 </summary>
        public event Action OnListSeparatorChanged;

        /// <summary> 選択中言語のスキル名一覧の区切り文字。 </summary>
        public string ListSeparator { get; private set; } = "、";

        /// <summary> ヘッダーに現在の解放ポイントを表示する。 </summary>
        /// <param name="points"> 現在の解放ポイント。 </param>
        public void SetPoints(int points)
        {
            _pointsLabel.text = points.ToString();
        }

        /// <summary> ヘッダーに現在の改造ポイントを表示する。 </summary>
        /// <param name="rebuildPoints"> 現在の改造ポイント。 </param>
        public void SetRebuildPoints(int rebuildPoints)
        {
            _rebuildPointsLabel.text = rebuildPoints.ToString();
        }

        private readonly Label _pointsLabel;
        private readonly Label _rebuildPointsLabel;
        private readonly LocalizedElementText _listSeparatorLocalizedText;
        private readonly LocalizedElementText[] _headingLocalizedTexts;

        private const string BACKBUTTON_NAME = "BackButton";
        private const string SETTING_SHORTCUT_BUTTON_NAME = "SettingShortcutButton";

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        /// <inheritdoc />
        /// <remarks> 起点ノード(ツリー最下部)が無い場合は戻るボタンへフォールバックします。 </remarks>
        protected override VisualElement InitialFocusElement =>
            RootElement.Q<VisualElement>(className: UINavigationExtensions.INITIAL_FOCUS_CLASS_NAME)
            ?? _backButton;

        private const string TREE_SCROLL_VIEW_NAME = "SkillTreeContainer";

        private readonly Button _backButton;
        private readonly Button _settingShortcutButton;
        private readonly ScrollView _treeScrollView;
        private IDisposable _backButtonActivation;
        private IDisposable _settingShortcutButtonActivation;
        private ScrollViewDragManipulator _dragScrollManipulator;
    }
}
