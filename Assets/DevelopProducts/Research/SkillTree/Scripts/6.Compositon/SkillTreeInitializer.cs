using System.Linq;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class SkillTreeInitializer : MonoBehaviour
    {
        [SerializeField] private SkillTreeRepository _skillTreeRepository;
        [SerializeField] private NodeView[] _nodeViews;
        [SerializeField] private SkillPointReposiroty _skillPointRepository;

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
            var lockUsecase = new LockUsecase();
            var presenter = new SkillNodePresenter(nodeRegistry, skillTree, _skillTreeRepository);
            var nodeLockController = new NodeLockController(lockUsecase, _skillTreeRepository, skillTree, presenter);
            var nodeUnlockController = new NodeUnlockController(unlockUsecase, _skillTreeRepository, skillTree, presenter);

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
            skillTreePanelView.Initialize(nodeUnlockController, nodeCanUnlockController, nodeLockController, presenter);
        }
    }
}
