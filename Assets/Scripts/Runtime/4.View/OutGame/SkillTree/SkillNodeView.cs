using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Utility.OutGame;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードのViewクラス。
    /// </summary>
    public class SkillNodeView : ISkillNodeViewModel, IDisposable
    {
        public SkillNodeView(VisualElement root, int nodeId, OutGameUIEvent outGameUIEvent)
        {
            _root = root;
            _nodeId = nodeId;
            _outGameUIEvent = outGameUIEvent;

            _root.MakeNavigable();
            _activationRegistration = _root.RegisterActivation(HandleActivationHandler);

            SetLocked();
        }

        public void Dispose()
        {
            _activationRegistration.Dispose();
        }

        /// <summary> このノードの要素を取得します。初期フォーカスの設定に使用します。 </summary>
        public VisualElement RootElement => _root;

        /// <summary>
        ///     スキルノードを解放済みにする。
        /// </summary>
        public void SetUnlocked()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_LOCKED);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_UNLOCKED);
        }

        /// <summary>
        ///     スキルノードを未解放にする。
        /// </summary>
        public void SetLocked()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_UNLOCKED);
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED);
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_LOCKED);
        }

        /// <summary>
        ///     スキルノードを選択済みにする。
        /// </summary>
        public void SetSelected()
        {
            _root.AddToClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED);
        }

        /// <summary>
        ///     スキルノードを未選択にする。
        /// </summary>
        public void SetUnSelected()
        {
            _root.RemoveFromClassList(UssClassNameConstants.USS_CLASS_SKILL_NODE_SELECTED);
        }

        private readonly int _nodeId;
        private readonly VisualElement _root;
        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly IDisposable _activationRegistration;

        /// <summary>
        ///     スキルノードが作動した時の処理。
        /// </summary>
        private void HandleActivationHandler()
        {
            SetSelected();
            _outGameUIEvent.OnSkillNodeSelected?.Invoke(_root.name);
        }

    }
}
