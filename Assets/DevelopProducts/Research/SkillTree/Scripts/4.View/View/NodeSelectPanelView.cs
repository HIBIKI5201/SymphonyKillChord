using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルツリーの解放パネルUIに色々表示させるクラス
    /// </summary>
    public class NodeSelectPanelView : MonoBehaviour
    {
        /// <summary>
        ///     初期化メソッド
        /// </summary>
        /// <param name="nodeUnlockController"></param>
        /// <param name="nodeCanUnlockController"></param>
        /// <param name="skillNodePresenter"></param>
        /// <param name="nodeVisibleController"></param>
        /// <param name="skillTreeRepository"></param>
        public void Initialize(
            NodeUnlockController nodeUnlockController,
            NodeCanUnlockController nodeCanUnlockController,
            SkillNodePresenter skillNodePresenter,
            NodeVisibleController nodeVisibleController,
            ISkillTreeRepository skillTreeRepository)
        {
            _nodeUnlockController = nodeUnlockController;
            _nodeCanUnlockController = nodeCanUnlockController;
            _skillNodePresenter = skillNodePresenter;
            _nodeVisibleController = nodeVisibleController;
            _skillTreeRepository = skillTreeRepository;

            // 流れとしては、最初に全てのノードをロック状態にしてから、アンロックされているノードをアンロック状態にする
            for (int i = 1; i <= _skillTreeRepository.PhaseCount; i++)
            {
                var phaseNodes = _skillTreeRepository.GetNodesByPhase(i);
                foreach (var node in phaseNodes)
                {
                    _nodeVisibleController.InVisibleNode(node.SkillNodeIdVO.Id);
                }
            }
            var unlockedNodes = _skillTreeRepository.AllSkillNodes.Where(n => n.IsUnlocked);
            foreach (var node in unlockedNodes)
            {
                _skillNodePresenter.Unlock(node.SkillNodeIdVO.Id, true);
            }
            CanUnlockNodes();
        }
        /// <summary>
        ///     ノードの状態を代入する
        /// </summary>
        /// <param name="nodeView"></param>
        public void SetNode(NodeView nodeView)
        {
            if (nodeView.IsUnlocked) return;
            _currentNodeCost = _skillNodePresenter.GetTotalCost(nodeView.Id);
            _nodeNameText.text = $"必要コスト {_currentNodeCost}";
            _currentNodeId = nodeView.Id;
        }
        [SerializeField] private TMP_Text _nodeNameText;
        [SerializeField] private Button _unlockButton;

        private int _currentNodeId = -1;
        private int _currentNodeCost = 0;
        private int _currentPhaseIndex = 0;

        private NodeUnlockController _nodeUnlockController;
        private NodeCanUnlockController _nodeCanUnlockController;
        private SkillNodePresenter _skillNodePresenter;
        private NodeVisibleController _nodeVisibleController;
        private ISkillTreeRepository _skillTreeRepository;

        /// <summary>
        ///     ボタンがクリックされた時に呼び出すメソッド
        /// </summary>
        private void OnUnlockButtonClicked()
        {
            var result = _nodeUnlockController.UnlockNode(_currentNodeId);
            if (result)
            {
                _nodeNameText.text = "Complete!";
            }
            var isAllUnlocked = TryVisibleNodes();
            if (isAllUnlocked)
            {
                VisibleNodes();
            }
            CanUnlockNodes();
        }
        /// <summary>
        ///     次のフェーズのノードを表示する
        /// </summary>
        private void VisibleNodes()
        {
            var phaseNodes = _skillTreeRepository.GetNodesByPhase(_currentPhaseIndex + 1);
            foreach (var phaseNode in phaseNodes)
            {
                _nodeVisibleController.VisibleNode(phaseNode.SkillNodeIdVO.Id);
            }
            _currentPhaseIndex++;
        }
        /// <summary>
        ///     現在のフェーズのノードが全てアンロックされているかを確認する
        /// </summary>
        /// <returns></returns>
        private bool TryVisibleNodes()
        {
            var isAllUnlocked = true;
            var phaseNodes = _skillTreeRepository.GetNodesByPhase(_currentPhaseIndex);
            foreach (var phaseNode in phaseNodes)
            {
                if (!phaseNode.IsUnlocked)
                {
                    isAllUnlocked = false;
                    break;
                }
            }
            return isAllUnlocked;
        }
        /// <summary>
        ///     ノードが表示可能なノードを可視化させる
        /// </summary>
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
        private void OnDestroy()
        {
            _unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
        }
    }
}
