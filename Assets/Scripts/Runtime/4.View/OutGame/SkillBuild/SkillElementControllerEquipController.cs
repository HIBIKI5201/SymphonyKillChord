using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面のスキル UI にコントローラーでの装備操作を設定するクラス。
    /// </summary>
    public sealed class SkillElementControllerEquipController : IDisposable
    {
        /// <summary>
        ///     SkillElementControllerEquipController クラスのコンストラクタ。
        ///     既存のスキル一覧要素と装備スロットへコントローラー操作を設定する。
        /// </summary>
        /// <param name="uiDocument"> ドキュメントの UIDocument。 </param>
        /// <param name="skillBuildViewModel"> 一時スロット状態を保持する ViewModel。 </param>
        /// <param name="soundEffectCommand"> UI操作音の再生コマンド。 </param>
        /// <exception cref="ArgumentNullException"> uiDocument または skillBuildViewModel が null の場合にスローされる。 </exception>
        /// <exception cref="ArgumentException"> uiDocument のルート要素が存在しない場合にスローされる。 </exception>
        public SkillElementControllerEquipController(
            UIDocument uiDocument,
            ISkillBuildViewModel skillBuildViewModel,
            IUISoundEffectCommand soundEffectCommand)
        {
            if (uiDocument == null)
            {
                throw new ArgumentNullException(nameof(uiDocument));
            }

            _skillBuildViewModel = skillBuildViewModel ?? throw new ArgumentNullException(nameof(skillBuildViewModel));
            _soundEffectCommand = soundEffectCommand;

            VisualElement root = uiDocument.rootVisualElement
                ?? throw new ArgumentException("UIDocument のルート要素が見つかりません。", nameof(uiDocument));
            _rootElement = root;
            _skillLevelUpButton = root.Q<VisualElement>(SKILL_LEVEL_UP_BUTTON_NAME);
            _skillBuildSaveButton = root.Q<VisualElement>(SKILL_BUILD_SAVE_BUTTON_NAME);
            _skillScrollView = root.Q<ScrollView>(SKILL_SCROLL_VIEW_NAME);
            List<VisualElement> skillElements =
                root.Query<VisualElement>(className: DRAGGABLE_CLASS_NAME).ToList();
            List<VisualElement> slots =
                root.Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASS_NAME).ToList();

            for (int i = 0; i < skillElements.Count; i++)
            {
                SetupSkillElement(skillElements[i]);
            }

            for (int i = 0; i < slots.Count; i++)
            {
                SetupSlot(slots[i]);
            }

            // 画面側のキャンセル(=閉じる)より先に持ち上げ解除を処理するため、
            // バブリングではなくトリクルダウンで購読する。
            root.RegisterCallback<NavigationCancelEvent>(
                HandleNavigationCancelHandler, TrickleDown.TrickleDown);
        }

        /// <summary>
        ///     動的に追加されたスキル要素へコントローラー操作を設定する。
        /// </summary>
        /// <param name="element"> セットアップ対象の VisualElement。 </param>
        public void SetupSkillElement(VisualElement element)
        {
            if (element == null || _skillElements.Contains(element))
            {
                return;
            }

            // 生成直後はまだ ScrollView へ未接続(.panel == null)のことがあるため、
            // ここでは破棄済み要素の掃除を行わない。掃除は一覧再構築が完了した後
            // (RestoreFocusIfLost 経由)にまとめて行う。詳しくは RemoveDetachedSkillElements
            // のコメントを参照。

            element.MakeNavigable();
            element.RegisterCallback<FocusInEvent>(HandleSkillElementFocusInHandler);
            element.RegisterCallback<NavigationSubmitEvent>(HandleSkillElementSubmitHandler);
            element.RegisterCallback<NavigationMoveEvent>(HandleSkillElementNavigationMoveHandler);
            _skillElements.Add(element);
        }

        /// <summary>
        ///     一覧の再構築が完了した後に、破棄済み要素の掃除とフォーカス復元を行う。
        ///     <para>
        ///         スキル一覧はスロット変更のたびにカード要素ごと作り直されるため、
        ///         再構築前にカードへフォーカスしていた場合はフォーカス先が破棄されて
        ///         失われる。そのままだとコントローラーの決定/キャンセル/移動操作が
        ///         一切反応しなくなるため、呼び出し側(一覧再構築の完了通知)から都度確認する。
        ///     </para>
        /// </summary>
        public void RestoreFocusIfLost()
        {
            // 一覧の再構築(カードをまとめてグループ枠へ追加した後、最後にその枠を
            // まとめて ScrollView へ Add する作り)がここまでに完了しているため、
            // 前回分の破棄済み要素は確実に .panel == null になっている。
            // SetupSkillElement 実行中(まだグループ枠が未接続の段階)に掃除すると、
            // 破棄されていない直前の兄弟カードまで誤って「破棄済み」と判定し、
            // 登録直後のイベントハンドラーを剥がしてしまうため、ここでのみ行う。
            RemoveDetachedSkillElements();

            VisualElement focusedElement =
                _rootElement.panel?.focusController?.focusedElement as VisualElement;
            if (focusedElement != null && focusedElement.panel != null)
            {
                return;
            }

            // 選択中スキルに対応するカードが見つかった場合のみ再フォーカスする。
            // 見つからない場合に一覧先頭などへ適当にフォーカスすると、マウス操作中
            // (本来どこにもフォーカスが無い状態が正常)にも関わらず毎回同じ要素へ
            // 強制的にフォーカス・選択させてしまうため、何もしない方が安全。
            int? selectedSkillId = _skillBuildViewModel.ExplicitlySelectedSkillId.CurrentValue;
            if (!selectedSkillId.HasValue)
            {
                return;
            }

            FindSkillElement(selectedSkillId.Value)?.FocusDeferred();
        }

        /// <summary>
        ///     持ち上げ状態を解除する。
        /// </summary>
        public void ClearCarry()
        {
            CarriedSkillId = null;
            _pendingConfirmSkillElement = null;
            for (int i = 0; i < _skillElements.Count; i++)
            {
                _skillElements[i].RemoveFromClassList(CARRIED_CLASS_NAME);
            }
        }

        /// <summary>
        ///     登録したイベント購読を解除する。
        /// </summary>
        public void Dispose()
        {
            _rootElement.UnregisterCallback<NavigationCancelEvent>(
                HandleNavigationCancelHandler, TrickleDown.TrickleDown);

            for (int i = 0; i < _skillElements.Count; i++)
            {
                _skillElements[i].UnregisterCallback<FocusInEvent>(HandleSkillElementFocusInHandler);
                _skillElements[i].UnregisterCallback<NavigationSubmitEvent>(HandleSkillElementSubmitHandler);
                _skillElements[i].UnregisterCallback<NavigationMoveEvent>(HandleSkillElementNavigationMoveHandler);
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].UnregisterCallback<FocusInEvent>(HandleSlotFocusInHandler);
                _slots[i].UnregisterCallback<NavigationSubmitEvent>(HandleSlotSubmitHandler);
                _slots[i].UnregisterCallback<NavigationMoveEvent>(HandleLeftNavigationMoveHandler);
            }

            ClearCarry();
            _skillElements.Clear();
            _slots.Clear();
        }

        /// <summary> 現在持ち上げているスキル ID。持ち上げていない場合は null。 </summary>
        public int? CarriedSkillId { get; private set; }

        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const string SKILL_ELEMENT_SLOT_CLASS_NAME = "skill-element-slot";
        private const string CARRIED_CLASS_NAME = "is-carried";
        private const string SKILL_LEVEL_UP_BUTTON_NAME = "SkillLevelUpButton";
        private const string SKILL_BUILD_SAVE_BUTTON_NAME = "SkillBuildSaveButton";
        private const string SKILL_SCROLL_VIEW_NAME = "SkillScrollView";
        private const int EMPTY_SKILL_ID = -1;

        private readonly VisualElement _rootElement;
        private readonly VisualElement _skillLevelUpButton;
        private readonly VisualElement _skillBuildSaveButton;
        private readonly ScrollView _skillScrollView;
        private readonly ISkillBuildViewModel _skillBuildViewModel;
        private readonly IUISoundEffectCommand _soundEffectCommand;
        private readonly List<VisualElement> _skillElements = new List<VisualElement>();
        private readonly List<VisualElement> _slots = new List<VisualElement>();
        /// <summary> 1回目の決定で「選択確定」のみ行ったスキル要素。2回目の決定で持ち上げに移る。 </summary>
        private VisualElement _pendingConfirmSkillElement;

        /// <summary>
        ///     コントローラーのフォーカス対象を詳細表示へ反映する。
        /// </summary>
        /// <param name="evt"> フォーカスイベント。 </param>
        private void HandleSkillElementFocusInHandler(FocusInEvent evt)
        {
            if (evt.currentTarget is not VisualElement focusedElement)
            {
                return;
            }

            if (!ReferenceEquals(_pendingConfirmSkillElement, focusedElement))
            {
                // 別要素へフォーカスが移った時点で、1回目の決定による選択確定状態を解除する。
                _pendingConfirmSkillElement = null;
            }

            // 一覧の左右移動は自前で解決しており(HandleSkillElementNavigationMoveHandler)、
            // Unity標準のフォーカス追従スクロールも一緒に無効化されているため、
            // フォーカスが移るあらゆる経路(左右移動、一覧再構築後の復帰など)をここ1箇所でカバーする。
            EnsureSkillElementVisible(focusedElement);

            if (!CarriedSkillId.HasValue && focusedElement.userData is int skillId)
            {
                _skillBuildViewModel.SelectSkill(skillId);
            }
        }

        /// <summary>
        ///     スキル一覧要素の決定操作を処理する。
        ///     <para>
        ///         1回目の決定では選択を確定して詳細表示するだけに留め、
        ///         同じ要素へもう一度決定操作を行った時に初めて持ち上げ状態へ移る。
        ///         誤って一覧を見ているだけで持ち上げが始まらないようにするため。
        ///     </para>
        /// </summary>
        /// <param name="evt"> ナビゲーション決定イベント。 </param>
        private void HandleSkillElementSubmitHandler(NavigationSubmitEvent evt)
        {
            if (evt.currentTarget is not VisualElement element ||
                element.userData is not int skillId)
            {
                return;
            }

            VisualElement sourceSlot = FindContainingSlot(element);
            if (CarriedSkillId.HasValue && sourceSlot != null)
            {
                ApplyCarriedSkillToSlot(sourceSlot);
                evt.StopPropagation();
                return;
            }

            if (!CarriedSkillId.HasValue && !ReferenceEquals(_pendingConfirmSkillElement, element))
            {
                // 1回目の決定: 選択を確定するだけで、まだ持ち上げない。
                _pendingConfirmSkillElement = element;
                _skillBuildViewModel.SelectSkill(skillId);
                evt.StopPropagation();
                return;
            }

            // 2回目の決定: ここで初めて持ち上げ状態へ移る。
            _pendingConfirmSkillElement = null;
            BeginCarry(skillId, element);

            (sourceSlot ?? FindSlotToFocus())?.FocusDeferred();
            evt.StopPropagation();
        }

        /// <summary>
        ///     左入力を強化ボタンへの移動、右入力を表示順で次のスキルへの移動として解決する。
        ///     <para>
        ///         一覧は横スクロールの1行に多数のカードが並ぶため、UI Toolkit標準の
        ///         自動ナビゲーションでは一部の要素にしか移動できないことがある。
        ///         生成順(=表示順)を保持している<see cref="_skillElements"/>を使って自前で解決する。
        ///     </para>
        /// </summary>
        /// <param name="evt"> ナビゲーション移動イベント。 </param>
        private void HandleSkillElementNavigationMoveHandler(NavigationMoveEvent evt)
        {
            if (evt.currentTarget is not VisualElement element)
            {
                return;
            }

            if (evt.direction == NavigationMoveEvent.Direction.Left)
            {
                if (TryFocusVisibleNeighborSkillElement(element, -1, evt))
                {
                    return;
                }

                // 一番左の要素で左入力した場合のみ、強化ボタンへフォーカスする。
                HandleLeftNavigationMoveHandler(evt);
                return;
            }

            // スキル一覧のどのスキルから下入力しても、編成保存ボタンへ移動する。
            if (evt.direction == NavigationMoveEvent.Direction.Down &&
                TryFocus(_skillBuildSaveButton))
            {
                evt.StopPropagation();
                element.panel?.focusController?.IgnoreEvent(evt);
                return;
            }

            if (evt.direction != NavigationMoveEvent.Direction.Right)
            {
                return;
            }

            if (!TryFocusVisibleNeighborSkillElement(element, 1, evt))
            {
                // 移動先がない場合も入力を消費し、スクロール領域へフォーカスを渡さない。
                evt.StopPropagation();
                element.panel?.focusController?.IgnoreEvent(evt);
            }
        }

        /// <summary>
        ///     表示順で隣接するスキル要素へフォーカスを移動する。
        /// </summary>
        /// <param name="element"> 移動元の要素。 </param>
        /// <param name="step"> 探索方向。+1で右隣、-1で左隣を探す。 </param>
        /// <param name="evt"> ナビゲーション移動イベント。 </param>
        /// <returns> 隣接するスキル要素へ移動できた場合はtrue。 </returns>
        private bool TryFocusVisibleNeighborSkillElement(
            VisualElement element,
            int step,
            NavigationMoveEvent evt)
        {
            VisualElement next = FindVisibleNeighborSkillElement(element, step);
            NavigationDebugLog.Log(
                $"[SkillListNav] from={NavigationDebugLog.Describe(element)} step={step} "
                + $"index={_skillElements.IndexOf(element)} count={_skillElements.Count} -> {NavigationDebugLog.Describe(next)}");

            if (next == null)
            {
                return false;
            }

            next.Focus();
            evt.StopPropagation();
            element.panel?.focusController?.IgnoreEvent(evt);
            return true;
        }

        /// <summary>
        ///     装備スロットとスキル要素で、左隣に要素が無い場合の左入力で強化ボタンへフォーカスする。
        /// </summary>
        /// <param name="evt"> ナビゲーション移動イベント。 </param>
        private void HandleLeftNavigationMoveHandler(NavigationMoveEvent evt)
        {
            if (evt.direction != NavigationMoveEvent.Direction.Left ||
                evt.currentTarget is not VisualElement element)
            {
                return;
            }

            // 強化できない場合は現在の要素に留める。
            TryFocus(_skillLevelUpButton);
            evt.StopPropagation();
            element.panel?.focusController?.IgnoreEvent(evt);
        }

        /// <summary>
        ///     装備スロットの決定操作で持ち上げ中のスキルを装備する。
        /// </summary>
        /// <param name="evt"> ナビゲーション決定イベント。 </param>
        private void HandleSlotSubmitHandler(NavigationSubmitEvent evt)
        {
            if (evt.currentTarget is not VisualElement slot)
            {
                return;
            }

            int slotIndex = FindSlotIndex(slot);
            if (!CarriedSkillId.HasValue)
            {
                if (!TryFindSlotSkillId(slotIndex, out int slotSkillId))
                {
                    return;
                }

                BeginCarry(slotSkillId, FindSkillElement(slotSkillId));
                evt.StopPropagation();
                return;
            }

            ApplyCarriedSkillToSlot(slot);
            evt.StopPropagation();
        }

        /// <summary>
        ///     装備スロットへフォーカスした時、装備中のスキルを選択状態へ反映する。
        /// </summary>
        /// <param name="evt"> フォーカスイベント。 </param>
        private void HandleSlotFocusInHandler(FocusInEvent evt)
        {
            // 持ち上げ中は移動先スロットの内容ではなく、持ち上げたスキルの詳細を維持する。
            if (CarriedSkillId.HasValue ||
                evt.currentTarget is not VisualElement slot)
            {
                return;
            }

            int slotIndex = FindSlotIndex(slot);
            if (TryFindSlotSkillId(slotIndex, out int skillId))
            {
                _skillBuildViewModel.SelectSkill(skillId);
            }
        }

        /// <summary>
        ///     持ち上げ中のキャンセル操作で持ち上げ状態を解除する。
        /// </summary>
        /// <param name="evt"> ナビゲーションキャンセルイベント。 </param>
        private void HandleNavigationCancelHandler(NavigationCancelEvent evt)
        {
            if (!CarriedSkillId.HasValue)
            {
                return;
            }

            ClearCarry();
            if (evt.target is VisualElement focusedElement)
            {
                SelectFocusedElementSkill(focusedElement);
            }

            evt.StopPropagation();
        }

        /// <summary>
        ///     指定したスキル要素がスクロール表示範囲外にある場合、見える位置まで
        ///     最小限だけスクロール位置を補正する。
        ///     <para>
        ///         <see cref="HandleSkillElementNavigationMoveHandler"/> がUnity標準のフォーカス移動処理
        ///         (これに伴う既定の追従スクロールも含む)を横取りして自前解決しているため、
        ///         左右移動で選択が画面外に出ても何もしなければ一切追従しない。ここで明示的に補正する。
        ///     </para>
        ///     <para>
        ///         フォーカス直後はまだ「フォーカス枠のボーダー付与」によるレイアウト更新が
        ///         反映されておらず、その場で worldBound を読むと1手前の位置を参照してしまい、
        ///         カード1つ分スクロールが足りずにはみ出る。<see cref="UINavigationExtensions.FocusDeferred"/>
        ///         と同じ理由のため、次のレイアウト確定後まで補正を遅延させる。
        ///     </para>
        /// </summary>
        /// <param name="element"> 可視範囲に収めたい要素。 </param>
        private void EnsureSkillElementVisible(VisualElement element)
        {
            if (element == null || element.panel == null)
            {
                return;
            }

            element.schedule.Execute(() => ApplyEnsureSkillElementVisible(element));
        }

        /// <summary>
        ///     <see cref="EnsureSkillElementVisible"/> の実処理。レイアウト確定後に呼ばれる想定。
        /// </summary>
        /// <param name="element"> 可視範囲に収めたい要素。 </param>
        private void ApplyEnsureSkillElementVisible(VisualElement element)
        {
            if (_skillScrollView == null || element.panel == null)
            {
                return;
            }

            Rect elementBounds = element.worldBound;
            Rect viewportBounds = _skillScrollView.contentViewport.worldBound;
            if (!IsValidRect(elementBounds) || !IsValidRect(viewportBounds))
            {
                return;
            }

            float scrollOffsetX = _skillScrollView.scrollOffset.x;
            if (elementBounds.xMin < viewportBounds.xMin)
            {
                scrollOffsetX -= viewportBounds.xMin - elementBounds.xMin;
            }
            else if (elementBounds.xMax > viewportBounds.xMax)
            {
                scrollOffsetX += elementBounds.xMax - viewportBounds.xMax;
            }
            else
            {
                // 既に表示範囲内に収まっている場合は何もしない。
                return;
            }

            _skillScrollView.scrollOffset = new Vector2(
                ClampScrollOffsetX(scrollOffsetX), _skillScrollView.scrollOffset.y);
        }

        /// <summary>
        ///     X方向のスクロールオフセットを現在のスクロール範囲へクランプする。
        /// </summary>
        /// <param name="rawScrollOffsetX"> クランプ前のスクロールオフセットX。 </param>
        /// <returns> クランプ後のスクロールオフセットX。範囲が取得できない場合は入力値をそのまま返す。 </returns>
        private float ClampScrollOffsetX(float rawScrollOffsetX)
        {
            float lowValue = _skillScrollView.horizontalScroller.lowValue;
            float highValue = _skillScrollView.horizontalScroller.highValue;
            if (!IsFinite(lowValue) || !IsFinite(highValue))
            {
                return rawScrollOffsetX;
            }

            return Mathf.Clamp(rawScrollOffsetX, lowValue, highValue);
        }

        /// <summary>
        ///     レイアウト矩形が座標計算に使用可能か判定する。
        /// </summary>
        /// <param name="rect"> 判定する矩形。 </param>
        /// <returns> すべての値が有限の場合は true。 </returns>
        private static bool IsValidRect(Rect rect)
        {
            return IsFinite(rect.x) && IsFinite(rect.y) && IsFinite(rect.width) && IsFinite(rect.height);
        }

        /// <summary>
        ///     値が有限(NaN・無限大でない)か判定する。
        /// </summary>
        /// <param name="value"> 判定する値。 </param>
        /// <returns> 有限の場合は true。 </returns>
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        /// <summary>
        ///     パネルから外れた(破棄された)スキル要素を管理対象から取り除く。
        /// </summary>
        private void RemoveDetachedSkillElements()
        {
            for (int i = _skillElements.Count - 1; i >= 0; i--)
            {
                if (_skillElements[i].panel != null)
                {
                    continue;
                }

                if (ReferenceEquals(_pendingConfirmSkillElement, _skillElements[i]))
                {
                    _pendingConfirmSkillElement = null;
                }

                _skillElements[i].UnregisterCallback<FocusInEvent>(HandleSkillElementFocusInHandler);
                _skillElements[i].UnregisterCallback<NavigationSubmitEvent>(HandleSkillElementSubmitHandler);
                _skillElements[i].UnregisterCallback<NavigationMoveEvent>(HandleSkillElementNavigationMoveHandler);
                _skillElements.RemoveAt(i);
            }
        }

        /// <summary>
        ///     装備スロットへコントローラー操作を設定する。
        /// </summary>
        /// <param name="slot"> セットアップ対象の装備スロット。 </param>
        private void SetupSlot(VisualElement slot)
        {
            slot.MakeNavigable();
            slot.RegisterCallback<FocusInEvent>(HandleSlotFocusInHandler);
            slot.RegisterCallback<NavigationSubmitEvent>(HandleSlotSubmitHandler);
            slot.RegisterCallback<NavigationMoveEvent>(HandleLeftNavigationMoveHandler);
            _slots.Add(slot);
        }

        /// <summary>
        ///     指定したスキルをコントローラーでの持ち上げ状態にする。
        /// </summary>
        /// <param name="skillId"> 持ち上げるスキル ID。 </param>
        /// <param name="element"> 持ち上げるスキル要素。 </param>
        private void BeginCarry(int skillId, VisualElement element)
        {
            CarriedSkillId = skillId;
            _skillBuildViewModel.SelectSkill(skillId);
            _soundEffectCommand?.Play(UISoundEffectKind.Select);

            for (int i = 0; i < _skillElements.Count; i++)
            {
                _skillElements[i].EnableInClassList(
                    CARRIED_CLASS_NAME,
                    ReferenceEquals(_skillElements[i], element));
            }
        }

        /// <summary>
        ///     持ち上げ中のスキルを指定したスロットへ反映する。
        /// </summary>
        /// <param name="slot"> 反映先の装備スロット。 </param>
        private void ApplyCarriedSkillToSlot(VisualElement slot)
        {
            if (!CarriedSkillId.HasValue)
            {
                return;
            }

            int carriedSkillId = CarriedSkillId.Value;
            int slotIndex = FindSlotIndex(slot);
            bool playsSkillSetSound = IsDifferentSkillSet(carriedSkillId, slotIndex);
            _skillBuildViewModel.ApplyDrop(carriedSkillId, slotIndex);
            if (playsSkillSetSound)
            {
                _soundEffectCommand?.Play(UISoundEffectKind.SkillSet);
            }

            ClearCarry();
            slot.FocusDeferred();
        }

        /// <summary>
        ///     要素がフォーカス可能な状態の場合のみフォーカスする。
        ///     <para>
        ///         レベルMax等でボタンが無効化されている場合、.Focus() は何もせず
        ///         静かに失敗する。それに気づかずイベントを消費してしまうと、
        ///         入力だけ飲み込んで何も起きない状態になるため、事前に判定する。
        ///     </para>
        /// </summary>
        /// <param name="target"> フォーカスさせたい要素。 </param>
        /// <returns> 実際にフォーカスした場合は true。 </returns>
        private static bool TryFocus(VisualElement target)
        {
            if (target == null ||
                !target.focusable ||
                !target.enabledInHierarchy ||
                target.resolvedStyle.display == DisplayStyle.None)
            {
                return false;
            }

            target.Focus();
            return true;
        }

        /// <summary>
        ///     フォーカス中の要素に対応するスキルを選択状態へ反映する。
        /// </summary>
        /// <param name="element"> フォーカス中の要素。 </param>
        private void SelectFocusedElementSkill(VisualElement element)
        {
            if (element.userData is int skillId)
            {
                _skillBuildViewModel.SelectSkill(skillId);
                return;
            }

            VisualElement slot = _slots.Contains(element)
                ? element
                : FindContainingSlot(element);
            if (slot == null)
            {
                return;
            }

            int slotIndex = FindSlotIndex(slot);
            if (TryFindSlotSkillId(slotIndex, out skillId))
            {
                _skillBuildViewModel.SelectSkill(skillId);
            }
        }

        /// <summary>
        ///     表示順で指定要素からstep方向に進み、最初に見つかる表示中のスキル要素を返す。
        /// </summary>
        /// <param name="current"> 探索の起点となる要素。 </param>
        /// <param name="step"> 探索方向(-1または1)。 </param>
        /// <returns> 見つかった要素。無ければnull。 </returns>
        private VisualElement FindVisibleNeighborSkillElement(VisualElement current, int step)
        {
            int index = _skillElements.IndexOf(current);
            if (index < 0)
            {
                return null;
            }

            for (int i = index + step; i >= 0 && i < _skillElements.Count; i += step)
            {
                VisualElement candidate = _skillElements[i];
                if (candidate.resolvedStyle.display != DisplayStyle.None)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        ///     持ち上げたスキルの置き先として、最初にフォーカスすべき装備スロットを返します。
        /// </summary>
        /// <returns> フォーカス先のスロットです。対象が無い場合はnullです。 </returns>
        private VisualElement FindSlotToFocus()
        {
            if (_slots.Count == 0)
            {
                return null;
            }

            IReadOnlyList<SkillBuildSlotState> slotStates = _skillBuildViewModel.Slots.CurrentValue;
            VisualElement emptySlot = null;
            int emptySlotIndex = int.MaxValue;
            for (int i = 0; i < _slots.Count; i++)
            {
                VisualElement slotElement = _slots[i];
                int slotIndex = FindSlotIndex(slotElement);
                for (int j = 0; j < slotStates.Count; j++)
                {
                    SkillBuildSlotState slotState = slotStates[j];
                    if (slotState.SlotIndex != slotIndex ||
                        slotState.CurrentSkillId != EMPTY_SKILL_ID)
                    {
                        continue;
                    }

                    if (slotIndex < emptySlotIndex)
                    {
                        emptySlot = slotElement;
                        emptySlotIndex = slotIndex;
                    }

                    break;
                }
            }

            return emptySlot ?? _slots[0];
        }

        /// <summary>
        ///     指定した要素を内包する装備スロットを取得する。
        /// </summary>
        /// <param name="element"> 検索対象の要素。 </param>
        /// <returns> 内包するスロット。スロット外の場合は null。 </returns>
        private VisualElement FindContainingSlot(VisualElement element)
        {
            VisualElement current = element?.parent;
            while (current != null)
            {
                for (int i = 0; i < _slots.Count; i++)
                {
                    if (ReferenceEquals(current, _slots[i]))
                    {
                        return current;
                    }
                }

                current = current.parent;
            }

            return null;
        }

        /// <summary>
        ///     指定したスキル ID の表示要素を取得する。
        /// </summary>
        /// <param name="skillId"> 検索対象のスキル ID。 </param>
        /// <returns> 対応する表示要素。見つからない場合は null。 </returns>
        private VisualElement FindSkillElement(int skillId)
        {
            for (int i = 0; i < _skillElements.Count; i++)
            {
                VisualElement element = _skillElements[i];
                if (element.panel != null && element.userData is int elementSkillId && elementSkillId == skillId)
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary>
        ///     指定したスロットに装備中のスキル ID を取得する。
        /// </summary>
        /// <param name="slotIndex"> スロット番号。 </param>
        /// <param name="skillId"> 装備中のスキル ID。 </param>
        /// <returns> スキルが装備されている場合は true。 </returns>
        private bool TryFindSlotSkillId(int slotIndex, out int skillId)
        {
            IReadOnlyList<SkillBuildSlotState> slots = _skillBuildViewModel.Slots.CurrentValue;
            for (int i = 0; i < slots.Count; i++)
            {
                SkillBuildSlotState slot = slots[i];
                if (slot.SlotIndex == slotIndex && slot.CurrentSkillId != EMPTY_SKILL_ID)
                {
                    skillId = slot.CurrentSkillId;
                    return true;
                }
            }

            skillId = EMPTY_SKILL_ID;
            return false;
        }

        /// <summary>
        ///     装備先要素に対応するスロット番号を取得する。
        /// </summary>
        /// <param name="slot"> 装備先スロット。 </param>
        /// <returns> スロット番号。 </returns>
        private int FindSlotIndex(VisualElement slot)
        {
            VisualElement root = _rootElement;
            List<VisualElement> slots =
                root.Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASS_NAME).ToList();
            for (int i = 0; i < slots.Count; i++)
            {
                if (ReferenceEquals(slots[i], slot))
                {
                    return i;
                }
            }

            throw new InvalidOperationException(
                $"[{nameof(SkillElementControllerEquipController)}] 装備先スロットがルート要素内に見つかりません。");
        }

        /// <summary>
        ///     移動先スロットへ別のスキルがセットされるか判定する。
        /// </summary>
        /// <param name="skillId"> 装備するスキル ID。 </param>
        /// <param name="destinationSlotIndex"> 移動先スロット番号。 </param>
        /// <returns> 移動先スロットの内容が変わる場合は true。 </returns>
        private bool IsDifferentSkillSet(int skillId, int destinationSlotIndex)
        {
            IReadOnlyList<SkillBuildSlotState> slots = _skillBuildViewModel.Slots.CurrentValue;
            for (int i = 0; i < slots.Count; i++)
            {
                SkillBuildSlotState slot = slots[i];
                if (slot.SlotIndex == destinationSlotIndex)
                {
                    return slot.CurrentSkillId != skillId;
                }
            }

            return false;
        }
    }
}
