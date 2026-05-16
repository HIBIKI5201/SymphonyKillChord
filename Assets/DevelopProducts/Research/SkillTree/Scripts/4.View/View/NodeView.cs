using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    [RequireComponent(typeof(Button))]
    public class NodeView : MonoBehaviour
    {
        public int Id => _skillNodeAsset.Id;
        public void Initialize(NodeRegistry nodeRegistry)
        {
            _nodeVM = new NodeViewModel();
            _nodeVM.CanUnlock += Canlock;
            _nodeVM.Unlocked += Unlock;
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
            Debug.Log($"ノードがアンロックされました: SkillNodeId {_skillNodeAsset.Id}, isUnlock: {isUnlock}");
            if (isUnlock)
            {
                _isUnlocked = true;
                _icon.color = Color.green;
            }

        }
        [SerializeField] private SkillNodeAsset _skillNodeAsset;
        private Image _icon;
        private NodeViewModel _nodeVM;
        private NodeSelectPanelView _nodeSelectPanelView;
        private NodeRegistry _nodeRegistry;
        private Button _button;
        private bool _isUnlocked;
        private void Awake()
        {
            _icon = GetComponent<Image>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => _nodeSelectPanelView.SetNode(this));
            _nodeSelectPanelView = FindAnyObjectByType<NodeSelectPanelView>();
        }
        private void OnDestroy()
        {
            _nodeVM.CanUnlock -= Canlock;
            _nodeVM.Unlocked -= Unlock;
            _button.onClick.RemoveListener(() => _nodeSelectPanelView.SetNode(this));
        }

    }
}
