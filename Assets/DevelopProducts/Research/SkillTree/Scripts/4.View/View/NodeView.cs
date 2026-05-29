using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードのView
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NodeView : MonoBehaviour
    {
        /// <summary>ノードViewのID</summary>
        public int Id => _id;
        /// <summary>解放されているかどうかのbool値</summary>
        public bool IsUnlocked => _isUnlocked;
        /// <summary>
        ///     初期化メソッド
        /// </summary>
        /// <param name="nodeRegistry"></param>
        /// <param name="panelView"></param>
        public void Initialize(NodeRegistry nodeRegistry, NodeSelectPanelView panelView)
        {
            _nodeRegistry = nodeRegistry;

            _icon = GetComponent<Image>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => _nodeSelectPanelView.SetNode(this));
            _nodeSelectPanelView = panelView;

            _nodeVM = new NodeViewModel();
            _nodeVM.CanUnlock += Canlock;
            _nodeVM.Unlocked += Unlock;
            _nodeVM.Visible += CanVisible;

            _nodeRegistry.Register(_id, _nodeVM);
        }
        /// <summary>
        ///     可視化されている且つ解放可能だったら色を変える
        /// </summary>
        /// <param name="canlock"></param>
        public void Canlock(bool canlock)
        {
            if (_isUnlocked)
            {
                return;
            }
            _icon.color = canlock ? Color.yellow : Color.white;
            Debug.Log($"NodeView: CanUnlock changed for SkillNodeId {_id}, canlock: {canlock}");
        }

        /// <summary>
        ///     ノードを解放する
        /// </summary>
        /// <param name="isUnlock"></param>
        public void Unlock(bool isUnlock)
        {
            Debug.Log($"ノードがアンロックされたかどうか: SkillNodeId {_id}, isUnlock: {isUnlock}");
            if (isUnlock)
            {
                _isUnlocked = true;
                _icon.color = Color.green;
            }
        }
        /// <summary>
        ///     可視化させる
        /// </summary>
        /// <param name="isLock"></param>
        public void CanVisible(bool isLock)
        {
            Debug.Log($"ノードを表示または非表示: SkillNodeId {_id}, isLock: {isLock}");
            this.gameObject.SetActive(isLock);
        }
        [Header("NodeViewの番号")]
        [SerializeField] private int _id;
        private Image _icon;
        private NodeViewModel _nodeVM;
        private NodeSelectPanelView _nodeSelectPanelView;
        private NodeRegistry _nodeRegistry;
        private Button _button;
        private bool _isUnlocked;
        private void OnDestroy()
        {
            if (_nodeVM == null) return;
            _nodeVM.CanUnlock -= Canlock;
            _nodeVM.Unlocked -= Unlock;
            _nodeVM.Visible -= CanVisible;
            _button.onClick.RemoveListener(() => _nodeSelectPanelView.SetNode(this));
        }

    }
}
