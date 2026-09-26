using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Utility.OutGame;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードの接続線のViewクラス。
    /// </summary>
    public class SkillNodeConnView : ISkillNodeConnViewModel
    {
        /// <summary>
        ///     ノード間の接続線の要素を指定して、未通過の見た目で生成する。
        /// </summary>
        public SkillNodeConnView(VisualElement root)
        {
            _root = root;
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_CONN_NOT_PASSED);
        }

        /// <summary>
        ///     接続線を通過済みに設定する。
        /// </summary>
        public void SetPassed()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_CONN_NOT_PASSED);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_CONN_PASSED);
        }

        /// <summary>
        ///     接続線を未通過に設定する。
        /// </summary>
        public void SetNotPassed()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_CONN_PASSED);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_CONN_NOT_PASSED);
        }

        private VisualElement _root;
    }
}
