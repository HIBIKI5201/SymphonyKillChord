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
        public SkillElementDragAndDropSetup(UIDocument uiDocument, SkillBuildViewModel skillBuildViewModel)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _skillBuildViewModel = skillBuildViewModel ?? throw new ArgumentNullException(nameof(skillBuildViewModel));

            VisualElement root = _uiDocument.rootVisualElement;
            SetupDraggables(root);
        }

        private readonly UIDocument _uiDocument;
        private readonly SkillBuildViewModel _skillBuildViewModel;

        private const string DRAGGABLE_CLASSNAME = "draggable";
        private const string SKILL_ELEMENT_CONTAINER_CLASSNAME = "skill-element-container";
        private const string SKILL_ELEMENT_SLOT_CLASSNAME = "skill-element-slot";
        private const string SKILL_LABEL_NAME = "skill-label";
        private const int EMPTY_SKILL_ID = -1;
        private const string EMPTY_SKILL_LABEL = "未設定";

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
            // ドロップ結果に応じた UI 更新後の状態を走査し、
            // 一時的なスロット状態を ViewModel へ同期する。
            SyncTemporarySlotStateFromUi();

#if UNITY_EDITOR
            if (slot == null)
            {
                Debug.Log($"{skill?.name} は元の位置に戻されました。");
                return;
            }

            Debug.Log($"{skill?.name} が {slot.name} にドロップされました。");
#endif
        }

        /// <summary>
        ///     現在の UI 上のスロット状態を ViewModel に同期する。
        ///     セーブボタン押下時にこの一時状態が SkillBuildDefinition へ反映される。
        /// </summary>
        private void SyncTemporarySlotStateFromUi()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                return;
            }

            List<VisualElement> slots = root.Query<VisualElement>(className: SKILL_ELEMENT_SLOT_CLASSNAME).ToList();

            for (int i = 0; i < slots.Count; i++)
            {
                VisualElement slot = slots[i];

                // 非表示中のスロットはまだ解放されていないため、
                // スキルがドロップされていない（＝元の位置に戻された）とみなす。
                if (slot.resolvedStyle.display == DisplayStyle.None)
                {
                    continue;
                }

                VisualElement skillElement = slot.Q<VisualElement>(className: DRAGGABLE_CLASSNAME);

                int skillId = EMPTY_SKILL_ID;
                string skillLabel = EMPTY_SKILL_LABEL;

                // ドロップされたスロットにドラッグ要素が存在する場合、その userData からスキル ID を取得し、スキル名ラベルを読み取る。
                if (skillElement != null)
                {
                    if (skillElement.userData is int storedSkillId)
                    {
                        skillId = storedSkillId;
                    }

                    Label skillLabelElement = skillElement.Q<Label>(SKILL_LABEL_NAME);
                    skillLabel = skillLabelElement?.text ?? EMPTY_SKILL_LABEL;
                }

                _skillBuildViewModel.UpdateSlot(i, skillId, skillLabel);
            }
        }
    }
}
