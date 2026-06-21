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

        private VisualElement _root;
    }
}
