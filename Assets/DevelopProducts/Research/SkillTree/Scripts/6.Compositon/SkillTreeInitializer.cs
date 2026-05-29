using System;
using System.Linq;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルツリーに関数クラスなどのDI
    /// </summary>
    public class SkillTreeInitializer : MonoBehaviour
    {
        private void Awake()
        {
            var nodePanelView = FindAnyObjectByType<NodeSelectPanelView>();

            if (_skillTreeRepository == null)
            {
                Debug.LogError($"{nameof(SkillTreeInitializer)}: {nameof(_skillTreeRepository)} is not assigned.");
                return;
            }
            _skillTreeRepository.Initialize();

            if (_skillTreeRepository.AllSkillNodes.Length != _nodeViews.Length)
            {
                Debug.LogWarning($"NodeViewの数とNodeAssetsの数が一致しません " +
                    $"View{_nodeViews.Length}:Assset{_skillTreeRepository.AllSkillNodes.Length}");
            }

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

            // NodeIdで昇順ソートする
            var sortedNodeAssets = _skillTreeRepository.AllSkillNodes.OrderBy(x => x.SkillNodeIdVO.Id).ToArray();
            var sortedNodeViews = _nodeViews.OrderBy(x => x.Id).ToArray();
            for (int i = 0; i < sortedNodeAssets.Length; i++)
            {
                if (sortedNodeAssets[i].SkillNodeIdVO.Id != sortedNodeViews[i].Id)
                {
                    throw new Exception($"NodeAssetIDに対応するNodeViewのIDのオブジェクトが見つからない " +
                        $"Asset{sortedNodeAssets[i].SkillNodeIdVO.Id} : View{sortedNodeViews[i].Id}");
                }

                sortedNodeViews[i].Initialize(nodeRegistry, nodePanelView);
            }

            var skillTreePanelView = FindAnyObjectByType<NodeSelectPanelView>();
            skillTreePanelView.Initialize(
                nodeUnlockController,
                nodeCanUnlockController,
                presenter,
                nodeVisibleController,
                _skillTreeRepository);
        }

        [SerializeField] private SkillTreeRepository _skillTreeRepository;
        [SerializeField] private NodeView[] _nodeViews;
        [SerializeField] private SkillPointRepository _skillPointRepository;
    }
}
