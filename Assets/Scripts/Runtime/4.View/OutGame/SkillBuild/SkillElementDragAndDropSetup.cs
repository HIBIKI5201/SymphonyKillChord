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
        ///     UIDocument を受け取り、ドラッグ&ドロップのセットアップを行う。
        /// </summary>
        /// <param name="uiDocument"> ドキュメントの UIDocument。 </param>
        public SkillElementDragAndDropSetup(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;

            var root = _uiDocument.rootVisualElement;
            SetupDraggables(root);
        }

        private readonly UIDocument _uiDocument;
        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const string SKILL_ELEMENT_CONTAINER_NAME = "skill-element-container";
        private const string SKILL_ELEMENT_SLOT_NAME = "skill-element-slot";

        /// <summary>
        ///    ドキュメントのルート要素を取得し、ドラッグ可能な要素とドロップターゲットをセットアップするメソッド。
        /// </summary>
        /// <param name="root"> ドキュメントのルート要素。 </param>
        private void SetupDraggables(VisualElement root)
        {
            // ドキュメントのルート要素から、ドラッグ可能な要素をクラス名で検索して取得する。
            List<VisualElement> draggables =
                root.Query<VisualElement>(className: DRAGGABLE_CLASS_NAME).ToList();

            Debug.Log($"{draggables.Count} 個のドラッグ可能な要素が見つかりました。");

            // 取得したドラッグ可能な要素に対して、ドラッグ&ドロップのマニピュレーターを追加する。
            foreach (var element in draggables)
            {
                var manipulator = new SkillElementDragAndDropManipulator(
                    element,
                    OnSkillElementDrop,
                    slotContainerName: SKILL_ELEMENT_CONTAINER_NAME,
                    slotName: SKILL_ELEMENT_SLOT_NAME);

                element.AddManipulator(manipulator);
            }
        }

        /// <summary>
        ///     スキル要素がドロップされたときの処理を行うメソッド。
        /// </summary>
        /// <param name="skill"> ドロップされたスキル要素の VisualElement。 </param>
        /// <param name="slot"> スキル要素がドロップされたスロットの VisualElement。 </param>
        private void OnSkillElementDrop(VisualElement skill, VisualElement slot)
        {
            // ドロップされたスキル要素とスロットを処理するロジックをここに実装する。
            if (slot == null)
            {
#if UNITY_EDITOR
                Debug.Log($"{skill.name} は元の位置に戻されました。");
#endif
                return;
            }

#if UNITY_EDITOR
            Debug.Log($"{skill.name} が {slot.name} にドロップされました。");
#endif
        }
    }
}
