using UnityEngine;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルツリーに関数クラスなどのDI
    /// </summary>
    public class SkillTreeInitializer : MonoBehaviour
    {
        [SerializeField] private SkillTreeRepository _skillTreeRepository;
        [SerializeField] private NodeView[] _nodeViews;
        [SerializeField] private SkillPointRepository _skillPointRepository;

        private void Awake()
        {
            if (_skillTreeRepository == null)
            {
                Debug.LogError($"{nameof(SkillTreeInitializer)}: {nameof(_skillTreeRepository)} is not assigned.");
                return;
            }

            _skillTreeRepository.Initialize();

            var nodeRegistry = new NodeRegistry();
            var canUnlockService = new SkillCanUnlockService();
            var canUnlockUsecase = new CanUnlockUsecase(canUnlockService, _skillPointRepository);
            var unlockUsecase = new UnlockUsecase(canUnlockService, _skillPointRepository);
            var presenter = new SkillNodePresenter(nodeRegistry, _skillTreeRepository);
            var nodeUnlockController = new NodeUnlockController(unlockUsecase, _skillTreeRepository, presenter);
            var nodeVisibleController = new NodeVisibleController(_skillTreeRepository, presenter);

            var nodeCanUnlockController = new NodeCanUnlockController(
                  _skillTreeRepository,
                  canUnlockUsecase,
                  presenter);

            var nodes = _skillTreeRepository.AllSkillNodes;
            for (int i = 0; i < _nodeViews.Length; i++)
            {
                _nodeViews[i].Initialize(nodeRegistry, nodes[i].SkillNodeIdVO.Id);
            }

            var skillTreePanelView = FindAnyObjectByType<NodeSelectPanelView>();
            skillTreePanelView.Initialize(
                nodeUnlockController, 
                nodeCanUnlockController, 
                presenter, 
                nodeVisibleController, 
                _skillTreeRepository);
        }
    }
}
