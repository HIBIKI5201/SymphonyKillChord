using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using System.Linq;
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

            if (_dragScrollManipulator != null)
            {
                _dragScrollManipulator.target = null;
                _dragScrollManipulator = null;
            }
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

            // 同じ役職はまとめて表示するため、括弧補足を除いたベース役職名でグループ化する。
            // GroupBy は先頭出現順を維持するため、CSV の並び順はそのまま保たれる。
            foreach (IGrouping<string, MemberViewDTO> group in
                members.GroupBy(member => SplitAtFullWidthParenthesis(member.ClassName).Body))
            {
                _memberScrollView.Add(CreateRoleGroupElement(group.Key, group));
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

        private const string MEMBER_ROLE_GROUP_CLASS = "member-role-group";
        private const string MEMBER_ROLE_LABEL_CLASS = "member-role-label";
        private const string MEMBER_NAME_ROW_CLASS = "member-name-row";
        private const string MEMBER_NAME_ITEM_CLASS = "member-name-item";
        private const string MEMBER_NAME_CAPTION_CLASS = "member-name-caption";
        private const string MEMBER_NAME_LABEL_CLASS = "member-name-label";

        private const char OPEN_PARENTHESIS = '（';
        private const char CLOSE_PARENTHESIS = '）';

        /// <summary>
        ///     役職補足に含まれていれば、その補足全体の代わりに行のグループキーとして使う部門名。
        ///     (例: 「背景リードデザイナー」は「背景」キーとして、同じ「背景」の人と同じ行にまとまる)
        /// </summary>
        private static readonly string[] DetailGroupKeywords = { "キャラクター", "背景", "武器" };

        /// <summary>
        ///     部門名からは自動判定できないが同じ行にまとめたい、役職と役職補足の組み合わせ。
        ///     キー: (ベース役職, 役職補足) → まとめる行のグループキー。
        /// </summary>
        private static readonly Dictionary<(string Role, string Detail), string> ManualRowGroupOverrides = new()
        {
            [("プログラマー", "テクニカルアーティスト")] = "テクニカルアーティスト・ウェブデザイナー",
            [("プログラマー", "ウェブデザイナー")] = "テクニカルアーティスト・ウェブデザイナー",
            [("イラストレーター", "背景")] = "背景・ロゴUI",
            [("イラストレーター", "ロゴ,UI")] = "背景・ロゴUI",
        };

        /// <summary>
        ///     この人数以下の役職グループは、部門(役職補足)が異なっていても改行せず1行にまとめる。
        /// </summary>
        private const int MAX_MEMBERS_WITHOUT_LINE_BREAK = 3;

        private Button _backButton;
        private VisualElement _backGround;
        private TabView _tabView;
        private Tab _productionTeamTab;
        private Tab _assetsUsedTab;
        private ScrollView _memberScrollView;
        private ListView _assetsUsedListView;
        private HierarchicalNavigationScope _navigationScope;
        private IDisposable _backButtonActivation;
        private ScrollViewDragManipulator _dragScrollManipulator;

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

            _dragScrollManipulator = new ScrollViewDragManipulator(_memberScrollView);
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
        ///     役職1グループ分の表示要素を生成します。役職名を1回だけ表示し、
        ///     その下に同じ役職のメンバー名を横並びで配置します。
        /// </summary>
        /// <param name="role"> 役職名です。 </param>
        /// <param name="members"> その役職に属する制作メンバー DTO の一覧です。 </param>
        /// <returns> 生成した表示要素です。 </returns>
        private static VisualElement CreateRoleGroupElement(string role, IEnumerable<MemberViewDTO> members)
        {
            var groupContainer = new VisualElement();
            groupContainer.AddToClassList(MEMBER_ROLE_GROUP_CLASS);

            var roleLabel = new Label(role);
            roleLabel.AddToClassList(MEMBER_ROLE_LABEL_CLASS);
            groupContainer.Add(roleLabel);

            List<MemberViewDTO> memberList = members.ToList();

            if (memberList.Count <= MAX_MEMBERS_WITHOUT_LINE_BREAK)
            {
                // 人数が少ないグループは、部門(役職補足)が異なっていても1行にまとめる。
                groupContainer.Add(CreateNameRowElement(memberList));
            }
            else
            {
                // 部門(役職補足)が異なるメンバーは行を分けて表示する。
                // GroupBy は先頭出現順を維持するため、行の並び順は CSV の並び順に従う。
                foreach (IGrouping<string, MemberViewDTO> detailGroup in
                    memberList.GroupBy(member => GetDetailGroupKey(role, SplitAtFullWidthParenthesis(member.ClassName).Detail)))
                {
                    groupContainer.Add(CreateNameRowElement(detailGroup));
                }
            }

            return groupContainer;
        }

        /// <summary>
        ///     1行分の名前表示要素を生成します。
        /// </summary>
        /// <param name="members"> その行に含めるメンバー DTO の一覧です。 </param>
        /// <returns> 生成した行の表示要素です。 </returns>
        private static VisualElement CreateNameRowElement(IEnumerable<MemberViewDTO> members)
        {
            var nameRow = new VisualElement();
            nameRow.AddToClassList(MEMBER_NAME_ROW_CLASS);

            foreach (MemberViewDTO member in members)
            {
                string displayName = SplitAtFullWidthParenthesis(member.Name).Body;
                string roleDetail = SplitAtFullWidthParenthesis(member.ClassName).Detail;
                nameRow.Add(CreateNameItemElement(displayName, roleDetail));
            }

            return nameRow;
        }

        /// <summary>
        ///     役職補足を行分けのためのグループキーへ変換します。
        ///     まず<see cref="ManualRowGroupOverrides"/>の手動指定を確認し、無ければ
        ///     既知の部門名(<see cref="DetailGroupKeywords"/>)を含むかどうかで判定し、
        ///     それにも該当しなければ補足文字列そのものをキーにします。
        /// </summary>
        /// <param name="role"> ベース役職名です。 </param>
        /// <param name="roleDetail"> 役職の括弧補足です。 </param>
        /// <returns> 行のグループキーです。 </returns>
        private static string GetDetailGroupKey(string role, string roleDetail)
        {
            if (ManualRowGroupOverrides.TryGetValue((role, roleDetail), out string overrideKey))
            {
                return overrideKey;
            }

            foreach (string keyword in DetailGroupKeywords)
            {
                if (roleDetail.Contains(keyword, StringComparison.Ordinal))
                {
                    return keyword;
                }
            }

            return roleDetail;
        }

        /// <summary>
        ///     1人分の名前表示要素を生成します。役職に部門などの補足がある場合は、
        ///     その補足を上段に小さく、氏名を下段に大きく表示します。
        /// </summary>
        /// <param name="displayName"> 括弧を除いた表示用の氏名です。 </param>
        /// <param name="roleDetail"> 役職の括弧補足です。無い場合は空文字列です。 </param>
        /// <returns> 生成した表示要素です。 </returns>
        private static VisualElement CreateNameItemElement(string displayName, string roleDetail)
        {
            var nameItem = new VisualElement();
            nameItem.AddToClassList(MEMBER_NAME_ITEM_CLASS);

            if (!string.IsNullOrEmpty(roleDetail))
            {
                var captionLabel = new Label(roleDetail);
                captionLabel.AddToClassList(MEMBER_NAME_CAPTION_CLASS);
                nameItem.Add(captionLabel);
            }

            var nameLabel = new Label(displayName);
            nameLabel.AddToClassList(MEMBER_NAME_LABEL_CLASS);
            nameItem.Add(nameLabel);

            return nameItem;
        }

        /// <summary>
        ///     文字列を全角括弧の前後で本体と補足に分離します。
        ///     全角括弧が無い場合は補足無しとして文字列全体をそのまま本体として返します。
        /// </summary>
        /// <param name="raw"> 分離対象の文字列です。 </param>
        /// <returns> 本体と補足のタプルです。補足が無い場合は空文字列になります。 </returns>
        private static (string Body, string Detail) SplitAtFullWidthParenthesis(string raw)
        {
            int openIndex = raw.IndexOf(OPEN_PARENTHESIS);
            int closeIndex = raw.IndexOf(CLOSE_PARENTHESIS, openIndex + 1);

            if (openIndex < 0 || closeIndex < 0 || closeIndex <= openIndex)
            {
                return (raw, string.Empty);
            }

            string body = raw[..openIndex].TrimEnd();
            string detail = raw[(openIndex + 1)..closeIndex];
            return (body, detail);
        }
    }
}
