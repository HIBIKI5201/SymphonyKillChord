using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     クレジット画面の View クラス。
    /// </summary>
    public class CreditScreenView : ScreenViewBase, IMemberListApplicable
    {
        /// <summary>
        ///    クレジット画面の View を初期化します。
        /// </summary>
        /// <param name="rootElement"> クレジット画面のルート要素です。 </param>
        /// <param name="outGameUIEvent"> アウトゲーム UI イベントです。 </param>
        /// <param name="hierarchicalNavigationScope"> クレジット画面の階層ごとにフォーカスを管理するクラスです。 </param>
        public CreditScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent,
            HierarchicalNavigationScope hierarchicalNavigationScope)
            : base(rootElement, outGameUIEvent)
        {
            Initialize(rootElement, hierarchicalNavigationScope);
            RegisterButtonCallbacks();
        }

        /// <summary>
        ///     タブ選択状態へ戻してクレジット画面を表示する。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            NormalizeActiveTab();
            _navigationScope.ResetToRootLevel();
            return base.Show(cancellationToken);
        }

        /// <summary>
        ///   クレジット画面の View のリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            UnregisterButtonCallbacks();
            _navigationScope.Dispose();
            base.Dispose();
        }

        /// <summary>
        ///     制作メンバー一覧を UI へ反映します。
        /// </summary>
        /// <param name="members"> 反映する制作メンバー DTO の一覧です。 </param>
        public void ApplyMemberList(IReadOnlyList<MemberViewDTO> members)
        {
            if (_memberScrollView == null)
            {
                return;
            }

            // UXML のプレースホルダや前回の反映結果が残らないよう、毎回作り直す。
            _memberScrollView.Clear();

            if (members == null)
            {
                return;
            }

            for (int i = 0; i < members.Count; i++)
            {
                _memberScrollView.Add(CreateMemberElement(members[i]));
            }
        }

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement =>
            _tabView.activeTab?.tabHeader ?? _productionTeamTab.tabHeader;

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private const string BACK_BUTTON_NAME = "BackButton";
        private const string BACK_GROUND_NAME = "BackGround";
        private const string MENBER_SCROLL_VIEW_NAME = "MemberScrollView";
        private const string PRODUCTION_TEAM_TAB_NAME = "ProductionTeam";
        private const string ASSETS_USED_TAB_NAME = "AssetsUsed";
        private const float MEMBER_SCROLL_STEP = 80f;

        private const string MEMBER_CONTAINER_CLASS = "member-container";
        private const string MEMBER_ROLE_LABEL_CLASS = "member-role-label";
        private const string MEMBER_NAME_LABEL_CLASS = "member-name-label";
        private const string MEMBER_AFFILIATION_LABEL_CLASS = "member-affiliation-label";

        private Button _backButton;
        private VisualElement _backGround;
        private TabView _tabView;
        private Tab _productionTeamTab;
        private Tab _assetsUsedTab;
        private ScrollView _memberScrollView;
        private ListView _assetsUsedListView;
        private HierarchicalNavigationScope _navigationScope;
        private IDisposable _backButtonActivation;

        /// <summary>
        ///     クレジット画面の UI 要素を初期化します。
        /// </summary>
        /// <param name="rootElement"> クレジット画面のルート要素です。 </param>
        /// <param name="hierarchicalNavigationScope"> クレジット画面の階層ごとにフォーカスを管理するクラスです。 </param>
        /// <exception cref="NullReferenceException"> 必要な UI 要素が見つからない場合に発生します。 </exception>
        private void Initialize(VisualElement rootElement, HierarchicalNavigationScope hierarchicalNavigationScope)
        {
            if (rootElement == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(CreditScreenView)}: Root VisualElementがnullです。");
#endif
                return;
            }
            if (hierarchicalNavigationScope == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(CreditScreenView)}: HierarchicalNavigationScopeがnullです。");
#endif
            }

            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {BACK_BUTTON_NAME}が見つかりません。");
            _backGround = rootElement.Q<VisualElement>(BACK_GROUND_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {BACK_GROUND_NAME}が見つかりません。");
            _tabView = rootElement.Q<TabView>()
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: TabViewが見つかりません。");
            _productionTeamTab = rootElement.Q<Tab>(PRODUCTION_TEAM_TAB_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {PRODUCTION_TEAM_TAB_NAME}が見つかりません。");
            _assetsUsedTab = rootElement.Q<Tab>(ASSETS_USED_TAB_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {ASSETS_USED_TAB_NAME}が見つかりません。");
            _memberScrollView = rootElement.Q<ScrollView>(MENBER_SCROLL_VIEW_NAME)
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: {MENBER_SCROLL_VIEW_NAME}が見つかりません。");
            _assetsUsedListView = _assetsUsedTab.Q<ListView>()
                ?? throw new NullReferenceException($"{nameof(CreditScreenView)}: 使用アセットのListViewが見つかりません。");
            _navigationScope = hierarchicalNavigationScope;
            _navigationScope.SetRootLevel(new VisualElement[]
            {
                _productionTeamTab.tabHeader,
                _assetsUsedTab.tabHeader,
            });
            _navigationScope.AddChildLevel(
                _productionTeamTab.tabHeader,
                new VisualElement[]
                {
                    _memberScrollView,
                },
                _memberScrollView);
            _navigationScope.AddChildLevel(
                _assetsUsedTab.tabHeader,
                new VisualElement[]
                {
                    _assetsUsedListView,
                },
                _assetsUsedListView);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
            _backGround.RegisterCallback<PointerDownEvent>(OnPointDownEvent);
            _productionTeamTab.tabHeader.RegisterCallback<ClickEvent>(HandleProductionTeamTabClickedHandler);
            _productionTeamTab.tabHeader.RegisterCallback<NavigationSubmitEvent>(HandleProductionTeamTabSubmittedHandler);
            _assetsUsedTab.tabHeader.RegisterCallback<ClickEvent>(HandleAssetsUsedTabClickedHandler);
            _assetsUsedTab.tabHeader.RegisterCallback<NavigationSubmitEvent>(HandleAssetsUsedTabSubmittedHandler);
            _memberScrollView.RegisterCallback<NavigationMoveEvent>(HandleMemberScrollNavigationHandler);
        }

        /// <summary>
        ///     各ボタンのコールバックを登録解除します。
        /// </summary>
        private void UnregisterButtonCallbacks()
        {
            _backButtonActivation?.Dispose();
            _backGround.UnregisterCallback<PointerDownEvent>(OnPointDownEvent);
            _productionTeamTab.tabHeader.UnregisterCallback<ClickEvent>(HandleProductionTeamTabClickedHandler);
            _productionTeamTab.tabHeader.UnregisterCallback<NavigationSubmitEvent>(HandleProductionTeamTabSubmittedHandler);
            _assetsUsedTab.tabHeader.UnregisterCallback<ClickEvent>(HandleAssetsUsedTabClickedHandler);
            _assetsUsedTab.tabHeader.UnregisterCallback<NavigationSubmitEvent>(HandleAssetsUsedTabSubmittedHandler);
            _memberScrollView.UnregisterCallback<NavigationMoveEvent>(HandleMemberScrollNavigationHandler);
        }

        /// <summary>
        ///     制作メンバータブがクリックされた時、一覧の内容操作へ移動する。
        /// </summary>
        /// <param name="clickEvent"> クリックイベント。 </param>
        private void HandleProductionTeamTabClickedHandler(ClickEvent clickEvent)
        {
            SelectTab(_productionTeamTab);
        }

        /// <summary>
        ///     制作メンバータブが決定された時、一覧の内容操作へ移動する。
        /// </summary>
        /// <param name="navigationEvent"> ナビゲーション決定イベント。 </param>
        private void HandleProductionTeamTabSubmittedHandler(NavigationSubmitEvent navigationEvent)
        {
            SelectTab(_productionTeamTab);
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     使用アセットタブがクリックされた時、一覧の内容操作へ移動する。
        /// </summary>
        /// <param name="clickEvent"> クリックイベント。 </param>
        private void HandleAssetsUsedTabClickedHandler(ClickEvent clickEvent)
        {
            SelectTab(_assetsUsedTab);
        }

        /// <summary>
        ///     使用アセットタブが決定された時、一覧の内容操作へ移動する。
        /// </summary>
        /// <param name="navigationEvent"> ナビゲーション決定イベント。 </param>
        private void HandleAssetsUsedTabSubmittedHandler(NavigationSubmitEvent navigationEvent)
        {
            SelectTab(_assetsUsedTab);
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     制作メンバー一覧への上下入力をスクロールへ変換する。
        /// </summary>
        /// <param name="navigationEvent"> ナビゲーション移動イベント。 </param>
        private void HandleMemberScrollNavigationHandler(NavigationMoveEvent navigationEvent)
        {
            float scrollDelta;
            switch (navigationEvent.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    scrollDelta = -MEMBER_SCROLL_STEP;
                    break;
                case NavigationMoveEvent.Direction.Down:
                    scrollDelta = MEMBER_SCROLL_STEP;
                    break;
                default:
                    return;
            }

            Vector2 scrollOffset = _memberScrollView.scrollOffset;
            scrollOffset.y = Mathf.Clamp(
                scrollOffset.y + scrollDelta,
                _memberScrollView.verticalScroller.lowValue,
                _memberScrollView.verticalScroller.highValue);
            _memberScrollView.scrollOffset = scrollOffset;
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     戻るボタンが押されたときの処理。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     バックグラウンドが押されたときの処理。
        /// </summary>
        /// <param name="evt"> ポインター押下イベント。 </param>
        private void OnPointDownEvent(PointerDownEvent evt)
        {
            // バックグラウンドの子要素が押された場合は処理を行わない
            if (evt.target != evt.currentTarget) { return; }

            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     指定タブを選択し、そのタブの内容操作へ切り替える。
        /// </summary>
        /// <param name="tab"> 選択するタブ。 </param>
        private void SelectTab(Tab tab)
        {
            _tabView.activeTab = tab;
            _navigationScope.EnterLevel(tab.tabHeader);
        }

        /// <summary>
        ///     現在表示中のタブを既知のタブへ正規化する。
        /// </summary>
        private void NormalizeActiveTab()
        {
            if (ReferenceEquals(_tabView.activeTab, _assetsUsedTab))
            {
                return;
            }

            _tabView.activeTab = _productionTeamTab;
        }

        /// <summary>
        ///     制作メンバー 1 人分の表示要素を生成します。
        /// </summary>
        /// <param name="member"> 表示する制作メンバー DTO です。 </param>
        /// <returns> 生成した表示要素です。 </returns>
        private static VisualElement CreateMemberElement(in MemberViewDTO member)
        {
            var memberContainer = new VisualElement();
            memberContainer.AddToClassList(MEMBER_CONTAINER_CLASS);

            var roleLabel = new Label(member.ClassName);
            roleLabel.AddToClassList(MEMBER_ROLE_LABEL_CLASS);

            var nameLabel = new Label(member.Name);
            nameLabel.AddToClassList(MEMBER_NAME_LABEL_CLASS);

            var affiliationLabel = new Label(member.AffiliationName);
            affiliationLabel.AddToClassList(MEMBER_AFFILIATION_LABEL_CLASS);

            memberContainer.Add(roleLabel);
            memberContainer.Add(nameLabel);
            memberContainer.Add(affiliationLabel);
            return memberContainer;
        }
    }
}
