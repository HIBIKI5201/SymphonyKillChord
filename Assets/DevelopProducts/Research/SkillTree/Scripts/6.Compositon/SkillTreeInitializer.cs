using UnityEngine;

namespace DevelopProducts.SkillTree
{
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
            var skillTree = new SkillTreeEntity(_skillTreeRepository.AllSkillNodes);
            var canUnlockUsecase = new CanUnlockUsecase(canUnlockService, _skillPointRepository);
            var unlockUsecase = new UnlockUsecase(canUnlockService, _skillPointRepository);
            var presenter = new SkillNodePresenter(nodeRegistry, skillTree, _skillTreeRepository);
            var nodeUnlockController = new NodeUnlockController(unlockUsecase, _skillTreeRepository, skillTree, presenter);
            var nodeVisibleController = new NodeVisibleController(_skillTreeRepository, presenter);

            var nodeCanUnlockController = new NodeCanUnlockController(
                  _skillTreeRepository,
                  canUnlockUsecase,
                  presenter,
                  skillTree);

            foreach (var nodeView in _nodeViews)
            {
                nodeView.Initialize(nodeRegistry);
            }

            var skillTreePanelView = FindAnyObjectByType<NodeSelectPanelView>();
            skillTreePanelView.Initialize(nodeUnlockController, nodeCanUnlockController, presenter, nodeVisibleController);
        }
    }
}
