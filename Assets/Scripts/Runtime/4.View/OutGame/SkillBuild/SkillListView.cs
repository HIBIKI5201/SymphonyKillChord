using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     入手済みスキル一覧を表示する View。
    /// </summary>
    public sealed class SkillListView : IDisposable
    {
        /// <summary>
        ///     SkillListView を初期化する。
        /// </summary>
        /// <param name="scrollView"> カード要素を追加する表示先(ScrollView内のコンテンツコンテナ)。 </param>
        /// <param name="scrollViewControl"> スクロール操作対象の ScrollView 本体。ドラッグスクロール設定に使う。 </param>
        /// <param name="skillElementTemplate"> 各要素のテンプレート。 </param>
        /// <param name="onSkillElementCreated"> 要素生成時のコールバック。 </param>
        /// <param name="soundEffectCommand"> UI操作音の再生コマンド。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillListView(
            VisualElement scrollView,
            ScrollView scrollViewControl,
            VisualTreeAsset skillElementTemplate,
            Action<VisualElement> onSkillElementCreated,
            IUISoundEffectCommand soundEffectCommand)
        {
            _scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
            _scrollViewControl = scrollViewControl;
            _skillElementTemplate = skillElementTemplate ?? throw new ArgumentNullException(nameof(skillElementTemplate));
            _onSkillElementCreated = onSkillElementCreated;
            _soundEffectCommand = soundEffectCommand;
            ConfigureScrollView();
        }

        /// <summary> スキル選択時にスキル ID を通知する。 </summary>
        public event Action<int> OnSkillSelected;

        /// <summary> ジャンルバッジ選択時にジャンル ID を通知する。 </summary>
        public event Action<int> OnGenreBadgeSelected;

        /// <summary>
        ///     スキル一覧を再構築する。解放済みが前半、未開放が後半になるよう、
        ///     境界に区切り線を挿入する。
        /// </summary>
        /// <param name="skills"> 表示するスキル一覧。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetSkills(IReadOnlyList<SkillViewData> skills)
        {
            if (skills == null)
            {
                throw new ArgumentNullException(nameof(skills));
            }

            // 呼び出し元(Presenter)が渡す順序に関わらず、常にこの一覧側で
            // 「解放済みを番号昇順→未開放を番号昇順」に並び替える。
            List<SkillViewData> orderedSkills = skills
                .OrderBy(data => data.IsUnlocked ? 0 : 1)
                .ThenBy(data => ExtractSkillNumber(data.DisplayName))
                .ToList();

            Clear();
            _unlockedGroupContainer = CreateGroupContainer();
            _lockedGroupContainer = CreateGroupContainer();

            for (int i = 0; i < orderedSkills.Count; i++)
            {
                SkillViewData data = orderedSkills[i];

                TemplateContainer rootElement = _skillElementTemplate.Instantiate();
                SkillElementView skillElementView = new(rootElement, _soundEffectCommand);
                skillElementView.Bind(in data);
                skillElementView.OnSelected += HandleSkillSelectedHandler;
                skillElementView.OnGenreBadgeSelected += HandleGenreBadgeSelectedHandler;
                _skillElementViews.Add(skillElementView);

                if (data.IsUnlocked)
                {
                    RemoveLeftMarginIfFirstInGroup(_unlockedGroupContainer, rootElement);
                    _unlockedGroupContainer.Add(rootElement);
                }
                else
                {
                    RemoveLeftMarginIfFirstInGroup(_lockedGroupContainer, rootElement);
                    _lockedGroupContainer.Add(rootElement);
                }

                // 未解放カードもコントローラーで選択し、詳細を確認できるようにする。
                _onSkillElementCreated?.Invoke(rootElement);
            }

            bool hasUnlockedSkill = _unlockedGroupContainer.childCount > 0;
            bool hasLockedSkill = _lockedGroupContainer.childCount > 0;

            if (hasUnlockedSkill)
            {
                _scrollView.Add(_unlockedGroupContainer);
            }

            if (hasUnlockedSkill && hasLockedSkill)
            {
                _groupDivider = CreateGroupDivider();
                _scrollView.Add(_groupDivider);
            }

            if (hasLockedSkill)
            {
                _scrollView.Add(_lockedGroupContainer);
            }
        }

        /// <summary>
        ///     ジャンルによる絞り込みを適用する。
        /// </summary>
        /// <param name="genreId"> 絞り込むジャンル ID。null の場合は全件表示。 </param>
        public void ApplyGenreFilter(int? genreId)
        {
            int visibleUnlockedCount = 0;
            int visibleLockedCount = 0;

            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillElementView view = _skillElementViews[i];
                SkillViewData? data = view.CurrentData;
                bool isVisible = !genreId.HasValue ||
                    (data.HasValue && Contains(data.Value.GenreIds, genreId.Value));
                view.RootElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

                if (isVisible && data.HasValue)
                {
                    if (data.Value.IsUnlocked)
                    {
                        visibleUnlockedCount++;
                    }
                    else
                    {
                        visibleLockedCount++;
                    }
                }
            }

            SetGroupVisibility(_unlockedGroupContainer, visibleUnlockedCount > 0);
            SetGroupVisibility(_lockedGroupContainer, visibleLockedCount > 0);
            SetGroupVisibility(_groupDivider, visibleUnlockedCount > 0 && visibleLockedCount > 0);
        }

        /// <summary>
        ///     装備中スキルのバッジ表示を更新する。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備中スキル ID の集合。 </param>
        public void SetEquippedSkillIds(IReadOnlyCollection<int> equippedSkillIds)
        {
            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillElementView view = _skillElementViews[i];
                SkillViewData? data = view.CurrentData;
                bool isEquipped = data.HasValue &&
                    equippedSkillIds != null &&
                    equippedSkillIds.Contains(data.Value.SkillId);
                view.SetEquipped(isEquipped);
            }
        }

        /// <summary>
        ///     選択表示を更新する。
        /// </summary>
        /// <param name="selectedSkillId"> 明示選択中のスキル ID。 </param>
        public void SetSelectedSkill(int? selectedSkillId)
        {
            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillViewData? data = _skillElementViews[i].CurrentData;
                bool isSelected =
                    selectedSkillId.HasValue &&
                    data.HasValue &&
                    data.Value.SkillId == selectedSkillId.Value;
                _skillElementViews[i].SetSelected(isSelected);
            }
        }

        /// <summary>
        ///     一覧要素を破棄する。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _skillElementViews.Count; i++)
            {
                SkillElementView view = _skillElementViews[i];
                view.OnSelected -= HandleSkillSelectedHandler;
                view.OnGenreBadgeSelected -= HandleGenreBadgeSelectedHandler;
                view.Dispose();
                view.RootElement.RemoveFromHierarchy();
            }

            _skillElementViews.Clear();
            _scrollView.Clear();
            _unlockedGroupContainer = null;
            _lockedGroupContainer = null;
            _groupDivider = null;
        }

        /// <summary>
        ///     一覧を破棄する。
        /// </summary>
        public void Dispose()
        {
            Clear();
            if (_dragManipulator != null && _scrollViewControl != null)
            {
                _scrollViewControl.RemoveManipulator(_dragManipulator);
                _dragManipulator = null;
            }

            OnSkillSelected = null;
            OnGenreBadgeSelected = null;
        }

        private const string GROUP_DIVIDER_CLASS_NAME = "skill-group-divider";
        private const string GROUP_CONTAINER_CLASS_NAME = "skill-group-container";
        private const string DRAGGABLE_CLASS_NAME = "draggable";

        private readonly VisualElement _scrollView;
        private readonly ScrollView _scrollViewControl;
        private readonly VisualTreeAsset _skillElementTemplate;
        private readonly Action<VisualElement> _onSkillElementCreated;
        private readonly IUISoundEffectCommand _soundEffectCommand;
        private readonly List<SkillElementView> _skillElementViews = new();
        private VisualElement _unlockedGroupContainer;
        private VisualElement _lockedGroupContainer;
        private VisualElement _groupDivider;
        private ScrollViewDragManipulator _dragManipulator;

        /// <summary>
        ///     ScrollView の基本設定を行う。
        /// </summary>
        private void ConfigureScrollView()
        {
            if (_scrollViewControl == null)
            {
                return;
            }

            _scrollViewControl.mode = ScrollViewMode.Horizontal;
            _scrollViewControl.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            _scrollViewControl.verticalScrollerVisibility = ScrollerVisibility.Hidden;

            // 一覧のフォーカスはスキル要素で受け、スクロール操作部品へ移動させない。
            _scrollView.ExcludeFromNavigation();
            _scrollViewControl.ExcludeFromNavigation();
            _scrollViewControl.contentViewport.ExcludeFromNavigation();
            _scrollViewControl.contentContainer.ExcludeFromNavigation();
            ExcludeScrollerFromNavigation(_scrollViewControl.horizontalScroller);
            ExcludeScrollerFromNavigation(_scrollViewControl.verticalScroller);

            // スクロールバーのつまみをドラッグしなくても一覧をドラッグしてスクロールできるようにする。
            // ただしスキルカード(装備ドラッグ対象、draggable クラス)上から始まった操作は、
            // 装備ドラッグと競合しないようスクロールドラッグの対象から除外する。
            _dragManipulator = new ScrollViewDragManipulator(
                _scrollViewControl,
                ScrollDragAxis.Horizontal,
                shouldIgnorePointerDownTarget: element => HasDraggableAncestor(element, _scrollViewControl));
        }

        /// <summary>
        ///     スクロールバーと内部のスライダー・ボタンをフォーカス移動から除外する。
        /// </summary>
        /// <param name="scroller"> 対象のスクロールバー。 </param>
        private static void ExcludeScrollerFromNavigation(Scroller scroller)
        {
            scroller.ExcludeFromNavigation();
            scroller.Query<VisualElement>().ForEach(element => element.ExcludeFromNavigation());
        }

        /// <summary>
        ///     指定要素、またはその祖先(ScrollView 自身は含まない)に
        ///     装備ドラッグ対象を示す draggable クラスが付与されているか判定する。
        /// </summary>
        /// <param name="element"> 判定対象要素。 </param>
        /// <param name="boundary"> 探索を打ち切る境界要素(この要素自体は含めない)。 </param>
        /// <returns> 装備ドラッグ対象の子孫である場合は true。 </returns>
        private static bool HasDraggableAncestor(VisualElement element, VisualElement boundary)
        {
            VisualElement current = element;
            while (current != null && current != boundary)
            {
                if (current.ClassListContains(DRAGGABLE_CLASS_NAME))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        /// <summary>
        ///     子要素からの選択通知を転送する。
        /// </summary>
        /// <param name="skillId"> スキル ID。 </param>
        private void HandleSkillSelectedHandler(int skillId)
        {
            OnSkillSelected?.Invoke(skillId);
        }

        /// <summary>
        ///     子要素からのジャンルバッジ選択通知を転送する。
        /// </summary>
        /// <param name="genreId"> 選択されたジャンル ID。 </param>
        private void HandleGenreBadgeSelectedHandler(int genreId)
        {
            OnGenreBadgeSelected?.Invoke(genreId);
        }

        /// <summary>
        ///     グループ枠(または区切り線)の表示・非表示を切り替える。
        /// </summary>
        /// <param name="element"> 対象要素。存在しない場合は何もしない。 </param>
        /// <param name="isVisible"> 表示する場合は true。 </param>
        private static void SetGroupVisibility(VisualElement element, bool isVisible)
        {
            if (element == null)
            {
                return;
            }

            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        ///     解放済み/未開放のグループを区切る縦線を生成する。
        /// </summary>
        /// <returns> 区切り線要素。 </returns>
        private static VisualElement CreateGroupDivider()
        {
            VisualElement divider = new();
            divider.AddToClassList(GROUP_DIVIDER_CLASS_NAME);
            return divider;
        }

        /// <summary>
        ///     解放済み/未開放それぞれのスキルを囲む枠を生成する。
        /// </summary>
        /// <returns> グループ枠要素。 </returns>
        private static VisualElement CreateGroupContainer()
        {
            VisualElement container = new();
            container.AddToClassList(GROUP_CONTAINER_CLASS_NAME);
            return container;
        }

        /// <summary>
        ///     グループ枠の先頭に追加される要素の場合、draggable クラスが持つ左余白(4%)を
        ///     打ち消し、グループ枠の左端に隙間なく詰まるようにする。
        /// </summary>
        /// <param name="groupContainer"> 追加先のグループ枠。 </param>
        /// <param name="element"> 追加しようとしている要素。 </param>
        private static void RemoveLeftMarginIfFirstInGroup(VisualElement groupContainer, VisualElement element)
        {
            if (groupContainer.childCount == 0)
            {
                element.style.marginLeft = 0;
            }
        }

        /// <summary>
        ///     表示名末尾の数字を「スキル番号」として抽出する。
        /// </summary>
        /// <param name="displayName"> 表示名(例: "スキル13")。 </param>
        /// <returns> 抽出した番号。数字が見つからない場合は int.MaxValue(末尾へ)。 </returns>
        private static int ExtractSkillNumber(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return int.MaxValue;
            }

            int end = displayName.Length;
            int start = end;
            while (start > 0 && char.IsDigit(displayName[start - 1]))
            {
                start--;
            }

            if (start == end)
            {
                return int.MaxValue;
            }

            return int.TryParse(displayName.Substring(start, end - start), out int number)
                ? number
                : int.MaxValue;
        }

        /// <summary>
        ///     ジャンル ID 配列に指定ジャンル ID が含まれるか判定する。
        /// </summary>
        /// <param name="genreIds"> ジャンル ID 配列。 </param>
        /// <param name="genreId"> 判定対象のジャンル ID。 </param>
        /// <returns> 含まれる場合は true。 </returns>
        private static bool Contains(int[] genreIds, int genreId)
        {
            if (genreIds == null)
            {
                return false;
            }

            for (int i = 0; i < genreIds.Length; i++)
            {
                if (genreIds[i] == genreId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
