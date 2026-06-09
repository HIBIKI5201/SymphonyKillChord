using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリー画面のコントローラークラス。
    /// </summary>
    public class SkillTreeController
    {
        public SkillTreeController(ISkillDetailShowable skillDetailView,
            SkillDetailPresenter presenter,
            SkillTreeService skillTreeService,
            PlayerStatusPresenter playerStatusPresenter,
            IPreviewVideoScreenViewModel previewVideoScreenViewModel,
            IPreviewVideoScreenViewShowable previewVideoScreenViewShowable,
            Dictionary<int, SkillNodeEntity> skillNodeEntities,
            Dictionary<int, ISkillNodeViewModel> skillNodeViews,
            Dictionary<int, string[]> skillNodeConnBinds,
            Dictionary<string, ISkillNodeConnViewModel> nodeConns,
            Dictionary<int, VisualElement> unlockPhases,
            SkillTreeStatusEntity skillTreeStatusEntity)
        {
            _skillDetailPresenter = presenter;
            _skillDetailView = skillDetailView;
            _skillTreeService = skillTreeService;
            _playerStatusPresenter = playerStatusPresenter;
            _skillNodeEntities = skillNodeEntities;
            _skillNodeViews = skillNodeViews;
            _skillNodeConnBinds = skillNodeConnBinds;
            _previewVideoScreenView = previewVideoScreenViewModel;
            _previewVideoScreenViewShowable = previewVideoScreenViewShowable;
            _nodeConns = nodeConns;
            _unlockPhases = unlockPhases;
            _skillTreeStatusEntity = skillTreeStatusEntity;
            _nodesOnPath = new();
        }

        /// <summary>
        ///     スキルノードが選択された時の処理。
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="token"></param>
        public void OnSkillNodeSelected(int nodeId, CancellationToken token)
        {
            if(_selectedNodeId != -1)
            {
                _skillNodeViews[_selectedNodeId].SetUnSelected();
            }
            _selectedNodeId = nodeId;
            SkillNodeEntity entity = _skillNodeEntities[nodeId];
            ISkillNodeViewModel view = _skillNodeViews[nodeId];
            int currentPoints = _skillTreeStatusEntity.CurrentPoints;
            _nodesOnPath.Clear();
            _costToUnlock = _skillTreeService.TryGetTotalCost(nodeId, _nodesOnPath);

            SkillDetailDTO dto = new SkillDetailDTO(entity.SkillNodeIdVO.Id, entity.SkillDetail, _costToUnlock, _costToUnlock != -1, entity.IsUnlocked);
            _skillDetailPresenter.Push(dto);
            _skillDetailView.Show(token);
            view.SetSelected();

            // TODO プレイヤーステータスの反映
            _playerStatusPresenter.Push();
        }

        /// <summary>
        ///     スキルを解放した時の処理。
        /// </summary>
        public void OnSkillUnlocked()
        {
            if(_nodesOnPath == null || _nodesOnPath.Count == 0)
            {
                Debug.LogError($"[SkillTreeController] 解放対象ノードの取得に失敗しました。");
            }
            foreach (SkillNodeEntity entity in _nodesOnPath)
            {
                int nodeId = entity.SkillNodeIdVO.Id;
                // TODO 実装待ち：スキル効果をプレイヤーに反映する処理
                entity.Unlock();
                _skillNodeViews[entity.SkillNodeIdVO.Id].SetUnlocked();
                UpdateConns(nodeId);
                UpdateUnlockPhase(nodeId);
            }
            SkillNodeEntity selectedNode = _skillNodeEntities[_selectedNodeId];
            SkillDetailDTO dto = new SkillDetailDTO(selectedNode.SkillNodeIdVO.Id, selectedNode.SkillDetail, -1, false, selectedNode.IsUnlocked);
            _skillDetailPresenter.Push(dto);
        }

        /// <summary>
        ///     スキル詳細画面を閉じたときの処理。
        /// </summary>
        public void OnSkillDetailClosed()
        {
            _skillNodeViews[_selectedNodeId].SetUnSelected();
            _nodesOnPath.Clear();
            _selectedNodeId = -1;
        }

        /// <summary>
        ///     スキルプレビューボタンを押した時の処理。
        /// </summary>
        /// <param name="token"></param>
        public void OnPreviewButtonClicked(CancellationToken token)
        {
            _previewVideoScreenViewShowable.Show(token);
            _previewVideoScreenView.PlayPreviewVideo(_selectedNodeId);
        }

        private Dictionary<int, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private HashSet<SkillNodeEntity> _nodesOnPath;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<string, ISkillNodeConnViewModel> _nodeConns;
        private ISkillDetailShowable _skillDetailView;
        private SkillDetailPresenter _skillDetailPresenter;
        private SkillTreeService _skillTreeService;
        PlayerStatusPresenter _playerStatusPresenter;
        private SkillTreeStatusEntity _skillTreeStatusEntity;
        private IPreviewVideoScreenViewModel _previewVideoScreenView;
        private IPreviewVideoScreenViewShowable _previewVideoScreenViewShowable;
        private int _costToUnlock = -1;
        private int _selectedNodeId = -1;

        /// <summary>
        ///     スキル解放時、解放段階が進むかチェックして、実行する。
        /// </summary>
        /// <param name="nodeId"></param>
        private void UpdateUnlockPhase(int nodeId)
        {
            if (_unlockPhases.ContainsKey(nodeId))
            {
                _unlockPhases[nodeId].visible = true;
            }
        }

        /// <summary>
        ///     スキル解放時、スキルと繋がる来ている方向の接続線を通過状態にする。
        /// </summary>
        /// <param name="nodeId"></param>
        private void UpdateConns(int nodeId)
        {
            string[] connNames = _skillNodeConnBinds[nodeId];
            if (connNames == null || connNames.Length <= 0) return;
            for(int i = 0;i < connNames.Length; i++)
            {
                _nodeConns[connNames[i]]?.SetPassed();
            }
        }
    }
}
