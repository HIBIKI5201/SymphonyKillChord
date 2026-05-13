using System.Linq;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class SkillTreeInitializer : MonoBehaviour
    {
        [SerializeField] private SkillTreeRepository _skillTreeRepository;
        [SerializeField] private NodeView[] _nodeViews;
        [SerializeField] private SkillPointReposiroty _skillPointRepository;

        private NodeRegistry _nodeRegistry;
        private NodeCanUnlockController _nodeCanUnlockController;

        private void Awake()
        {
            if (_skillTreeRepository == null)
            {
                Debug.LogError($"{nameof(SkillTreeInitializer)}: {nameof(_skillTreeRepository)} is not assigned.");
                return;
            }

            _skillTreeRepository.Initialize();

            _nodeRegistry = new NodeRegistry();
            var algorithmService = new SkillPathSearchService();
            var canUnlockService = new SkillCanUnlockService();
            var canUnlockUsecase = new CanUnlockUsecase(canUnlockService, _skillPointRepository);
            var presenter = new SkillNodePresenter(_nodeRegistry);

            var skillTree = new SkillTreeEntity(_skillTreeRepository.SkillNodeEntities);
            _nodeCanUnlockController = new NodeCanUnlockController(
                _skillTreeRepository,
                algorithmService,
                canUnlockUsecase,
                presenter,
                skillTree);

            foreach (var nodeView in _nodeViews.Where(v => v != null))
            {
                nodeView.Initialize(_nodeRegistry, _nodeCanUnlockController);
            }
        }
    }
}
