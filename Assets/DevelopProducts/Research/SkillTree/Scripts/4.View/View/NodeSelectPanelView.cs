using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    public class NodeSelectPanelView : MonoBehaviour
    {
        public void Initialize(NodeUnlockController nodeUnlockController, 
            NodeCanUnlockController nodeCanUnlockController,
            SkillNodePresenter skillNodePresenter)
        {
            _nodeUnlockController = nodeUnlockController;
            _nodeCanUnlockController = nodeCanUnlockController;
            _skillNodePresenter = skillNodePresenter;
        }

        public void SetNode(NodeView nodeView)
        {
            _currentNodeCost = _skillNodePresenter.GetTotalCost(nodeView.Id);
            _nodeNameText.text = $"必要コスト {_currentNodeCost}";
            _currentNodeId = nodeView.Id;
        }
        [SerializeField] private TMP_Text _nodeNameText;
        [SerializeField] private Button _unlockButton;
        [SerializeField] private SkillTreeRepository _skillTreeRepository;
        private int _currentNodeId = -1;
        private int _currentNodeCost = 0;
        private NodeUnlockController _nodeUnlockController;
        private NodeCanUnlockController _nodeCanUnlockController;
        private SkillNodePresenter _skillNodePresenter;
        private void OnUnlockButtonClicked()
        {
           var result = _nodeUnlockController.UnlockNode(_currentNodeId);
            if (result)
            {
                _nodeNameText.text = "Complete!";
            }
            CanUnlockNodes();
        }
        private void CanUnlockNodes()
        {
            var enableNodes = _skillTreeRepository.AllSkillNodes.Where(n => n.IsEnable && !n.IsUnlocked);
            foreach (var node in enableNodes)
            {
                _nodeCanUnlockController.CanUnlock(node.SkillNodeIdVO.Id);
            }
        }
        private void Awake()
        {
            _unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }
        private void Start()
        {
            CanUnlockNodes();
            var unlockedNodes = _skillTreeRepository.AllSkillNodes.Where(n => n.IsUnlocked);
            foreach (var node in unlockedNodes)
            {
                _skillNodePresenter.Unlock(node.SkillNodeIdVO.Id, true);
            }
        }
        private void OnDestroy()
        {
            _unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }
    }
}
