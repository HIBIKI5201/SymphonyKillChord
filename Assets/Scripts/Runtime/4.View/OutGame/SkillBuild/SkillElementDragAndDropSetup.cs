using KillChord.Runtime.Adaptor.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面のスキル UI のドラッグ&ドロップのセットアップを担当するクラス。
    /// </summary>
    public class SkillElementDragAndDropSetup
    {
        /// <summary>
        ///     SkillElementDragAndDropSetup クラスのコンストラクタ。
        ///     UIDocument を受け取り、既存要素へのドラッグ&ドロップのセットアップを行う。
        /// </summary>
        /// <param name="uiDocument"> ドキュメントの UIDocument。 </param>
        /// <param name="skillBuildViewModel"> 一時スロット状態を保持する ViewModel。 </param>
        public SkillElementDragAndDropSetup(UIDocument uiDocument, ISkillBuildViewModel skillBuildViewModel)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _skillBuildViewModel = skillBuildViewModel ?? throw new ArgumentNullException(nameof(skillBuildViewModel));

            VisualElement root = _uiDocument.rootVisualElement;
            SetupDraggables(root);
        }

        private readonly UIDocument _uiDocument;
        private readonly ISkillBuildViewModel _skillBuildViewModel;

        private const string DRAGGABLE_CLASSNAME = "draggable";
        private const string SKILL_ELEMENT_CONTAINER_CLASSNAME = "skill-element-container";
        private const string SKILL_ELEMENT_SLOT_CLASSNAME = "skill-element-slot";
        /// <summary>
        ///     単一のスキル要素にドラッグ&ドロップ操作を設定する。
        ///     新規スキル入手時など、動的に追加された要素に対して呼び出す。
        /// </summary>
        /// <param name="element"> セットアップ対象の VisualElement。 </param>
        public void SetupDraggable(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            SkillElementDragAndDropManipulator manipulator = new SkillElementDragAndDropManipulator(
                element,
                OnSkillElementDrop,
                slotContainerName: SKILL_ELEMENT_CONTAINER_CLASSNAME,
                slotName: SKILL_ELEMENT_SLOT_CLASSNAME);

            element.AddManipulator(manipulator);
        }

        /// <summary>
        ///    ドキュメントのルート要素を取得し、既存のドラッグ可能な要素とドロップターゲットをセットアップするメソッド。
        /// </summary>
        /// <param name="root"> ドキュメントのルート要素。 </param>
        private void SetupDraggables(VisualElement root)
        {
            List<VisualElement> draggables =
                root.Query<VisualElement>(className: DRAGGABLE_CLASSNAME).ToList();

            for (int i = 0; i < draggables.Count; i++)
            {
                SetupDraggable(draggables[i]);
            }
        }

        /// <summary>
        ///     スキル要素がドロップされたときの処理を行うメソッド。
        /// </summary>
        /// <param name="skill"> ドロップされたスキル要素の VisualElement。 </param>
        /// <param name="slot"> スキル要素がドロップされたスロットの VisualElement。 </param>
        private void OnSkillElementDrop(VisualElement skill, VisualElement slot)
        {
            if (slot == null)
            {
#if UNITY_EDITOR
                Debug.Log($"{skill?.name} は元の位置に戻されました。");
#endif
                return;
            }

            if (skill?.userData is not int skillId)
            {
                Debug.LogError(
                    $"[{nameof(SkillElementDragAndDropSetup)}] ドロップされた要素からスキル ID を取得できませんでした。");
                return;
            }

            int? destinationSlotIndex = FindSlotIndex(slot);
            _skillBuildViewModel.ApplyDrop(skillId, destinationSlotIndex);

#if UNITY_EDITOR
            Debug.Log($"{skill?.name} が {slot.name} にドロップされました。");
#endif
        }

        /// <summary>
        ///     ドロップ先要素に対応するスロット番号を取得する。
        /// </summary>
        /// <param name="dropTarget"> ドロップ先。 </param>
        /// <returns> スロット番号。一覧の場合は null。 </returns>
        private int? FindSlotIndex(VisualElement dropTarget)
        {
            if (!dropTarget.ClassListContains(SKILL_ELEMENT_SLOT_CLASSNAME))
            {
                return null;
            }

            VisualElement root = _uiDocument.rootVisualElement;
            List<VisualElement> slots = root.Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASSNAME).ToList();
            for (int i = 0; i < slots.Count; i++)
            {
                if (ReferenceEquals(slots[i], dropTarget))
                {
                    return i;
                }
            }

            throw new InvalidOperationException(
                $"[{nameof(SkillElementDragAndDropSetup)}] ドロップ先スロットがルート要素内に見つかりません。");
        }
    }
}
