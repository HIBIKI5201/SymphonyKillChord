using KillChord.Runtime.Adaptor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    public class NodeView : MonoBehaviour, IPointerClickHandler
    {
        public ICanUnlockVM CanUnlockVM => _canUnlockViewModel;
        public void Initialize(NodeRegistry nodeRegistry, NodeCanUnlockController nodeCanUnlockController)
        {
            _canUnlockViewModel = new CanUnlockViewModel();
            _canUnlockViewModel.CanUnlock += Canlock;
            _nodeRegistry = nodeRegistry;
            _nodeRegistry.Register(_skillNodeAsset.Id, _canUnlockViewModel);
            _nodeCanUnlockController = nodeCanUnlockController;
        }
        public void OnPointerClick(PointerEventData eventData)
        {

        }
        public void Canlock(bool canlock)
        {
            if (canlock)
                _icon.color = Color.yellow;
            Debug.Log($"NodeView: CanUnlock changed for SkillNodeId {_skillNodeAsset.Id}, canlock: {canlock}");
        }
        public void Unlock()
        {
            _icon.color = Color.green;
        }
        [SerializeField] private SkillNodeAsset _skillNodeAsset;
        private Image _icon;
        private CanUnlockViewModel _canUnlockViewModel;
        private NodeSelectPanelView _nodeSelectPanelView;
        private NodeRegistry _nodeRegistry;
        private NodeCanUnlockController _nodeCanUnlockController;
        private void Awake()
        {
            _icon = GetComponent<Image>();
            _nodeSelectPanelView = FindAnyObjectByType<NodeSelectPanelView>();
        }
        private void Start()
        {
            _nodeCanUnlockController.CanUnlock(_skillNodeAsset.Id);
        }
        private void OnDestroy()
        {
            _canUnlockViewModel.CanUnlock -= Canlock;
        }

    }
}
