using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.Navigation;
using System;
using System.Collections.Generic;
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

            // スキル一覧は再構築されるため、破棄済みの要素を溜め込まないよう先に取り除く。
            RemoveDetachedSkillElements();

            element.MakeNavigable();
            element.RegisterCallback<FocusInEvent>(HandleSkillElementFocusInHandler);
            element.RegisterCallback<NavigationSubmitEvent>(HandleSkillElementSubmitHandler);
            _skillElements.Add(element);
        }

        /// <summary>
        ///     持ち上げ状態を解除する。
        /// </summary>
        public void ClearCarry()
        {
            CarriedSkillId = null;
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
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].UnregisterCallback<FocusInEvent>(HandleSlotFocusInHandler);
                _slots[i].UnregisterCallback<NavigationSubmitEvent>(HandleSlotSubmitHandler);
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
        private const int EMPTY_SKILL_ID = -1;

        private readonly VisualElement _rootElement;
        private readonly ISkillBuildViewModel _skillBuildViewModel;
        private readonly IUISoundEffectCommand _soundEffectCommand;
        private readonly List<VisualElement> _skillElements = new List<VisualElement>();
        private readonly List<VisualElement> _slots = new List<VisualElement>();

        /// <summary>
        ///     コントローラーのフォーカス対象を詳細表示へ反映する。
        /// </summary>
        /// <param name="evt"> フォーカスイベント。 </param>
        private void HandleSkillElementFocusInHandler(FocusInEvent evt)
        {
            if (!CarriedSkillId.HasValue &&
                evt.currentTarget is VisualElement element &&
                element.userData is int skillId)
            {
                _skillBuildViewModel.SelectSkill(skillId);
            }
        }

        /// <summary>
        ///     スキル一覧要素の決定操作を持ち上げ状態へ変換する。
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

            BeginCarry(skillId, element);

            (sourceSlot ?? FindSlotToFocus())?.FocusDeferred();
            evt.StopPropagation();
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

                _skillElements[i].UnregisterCallback<FocusInEvent>(HandleSkillElementFocusInHandler);
                _skillElements[i].UnregisterCallback<NavigationSubmitEvent>(HandleSkillElementSubmitHandler);
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
