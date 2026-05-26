using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    [RequireComponent(typeof(Button))]
    public class NodeView : MonoBehaviour
    {
        public int Id => _skillNodeAsset.Id;
        public bool IsUnlocked => _isUnlocked;
        public void Initialize(NodeRegistry nodeRegistry)
        {
            _icon = GetComponent<Image>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => _nodeSelectPanelView.SetNode(this));
            _nodeSelectPanelView = FindAnyObjectByType<NodeSelectPanelView>();

            _nodeVM = new NodeViewModel();
            _nodeVM.CanUnlock += Canlock;
            _nodeVM.Unlocked += Unlock;
            _nodeVM.Visible += CanVisible;
            _nodeRegistry = nodeRegistry;
            _nodeRegistry.Register(_skillNodeAsset.Id, _nodeVM);
        }
        public void Canlock(bool canlock)
        {
            if (_isUnlocked)
            {
                return;
            }
            _icon.color = canlock ? Color.yellow : Color.white;
            Debug.Log($"NodeView: CanUnlock changed for SkillNodeId {_skillNodeAsset.Id}, canlock: {canlock}");
        }
        public void Unlock(bool isUnlock)
        {
            Debug.Log($"ノードがアンロックされたかどうか: SkillNodeId {_skillNodeAsset.Id}, isUnlock: {isUnlock}");
            if (isUnlock)
            {
                _isUnlocked = true;
                _icon.color = Color.green;
            }
        }
        public void CanVisible(bool isLock)
        {
            Debug.Log($"ノードを表示または非表示: SkillNodeId {_skillNodeAsset.Id}, isLock: {isLock}");
            this.gameObject.SetActive(isLock);
        }
        [SerializeField] private SkillNodeAsset _skillNodeAsset;
        private Image _icon;
        private NodeViewModel _nodeVM;
        private NodeSelectPanelView _nodeSelectPanelView;
        private NodeRegistry _nodeRegistry;
        private Button _button;
        private bool _isUnlocked;
        private void OnDestroy()
        {
            if(_nodeVM == null) return;
            _nodeVM.CanUnlock -= Canlock;
            _nodeVM.Unlocked -= Unlock;
            _nodeVM.Visible -= CanVisible;
            _button.onClick.RemoveListener(() => _nodeSelectPanelView.SetNode(this));
        }

    }
}
