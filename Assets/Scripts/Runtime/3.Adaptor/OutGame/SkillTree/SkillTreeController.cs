using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリー画面のコントローラークラス。
    /// </summary>
    public class SkillTreeController
    {
        public SkillTreeController(ISkillDetailShowable skillDetailView,
            SkillDetailPresenter presenter,
            Label currentPointsLabel,
            SkillTreeService skillTreeService,
            PlayerStatusPresenter playerStatusPresenter,
            IPreviewVideoScreenViewModel previewVideoScreenViewModel,
            IPreviewVideoScreenViewShowable previewVideoScreenViewShowable,
            Dictionary<SkillNodeId, SkillNodeEntity> skillNodeEntities,
            Dictionary<int, ISkillNodeViewModel> skillNodeViews,
            Dictionary<int, string[]> skillNodeConnBinds,
            Dictionary<string, ISkillNodeConnViewModel> nodeConns,
            Dictionary<int, VisualElement> unlockPhases,
            Dictionary<int, VideoClip> videoClipBinds,
            SkillTreeStatusEntity skillTreeStatusEntity,
            Action ownedSkillChanged)
        {
            _skillDetailPresenter = presenter;
            _currentPointsLabel = currentPointsLabel;
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
            _videoClipBinds = videoClipBinds;
            _skillTreeStatusEntity = skillTreeStatusEntity;
            _ownedSkillChanged = ownedSkillChanged;
            _nodesOnPath = new();

            _currentPointsLabel.text = CURRENT_POINTS_LABEL_TEXT + _skillTreeStatusEntity.CurrentPoints.ToString();
        }

        /// <summary>
        ///     スキルノードが選択された時の処理。
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="token"></param>
        public void OnSkillNodeSelected(int nodeId, CancellationToken token)
        {
            if (_selectedNodeId != -1)
            {
                _skillNodeViews[_selectedNodeId].SetUnSelected();
            }
            _selectedNodeId = nodeId;
            SkillNodeId selectedNodeId = new SkillNodeId(nodeId);
            SkillNodeEntity entity = _skillNodeEntities[selectedNodeId];
            ISkillNodeViewModel view = _skillNodeViews[nodeId];
            int currentPoints = _skillTreeStatusEntity.CurrentPoints;
            _nodesOnPath.Clear();
            _costToUnlock = _skillTreeService.TryGetTotalCost(selectedNodeId, _nodesOnPath);

            bool canUnlock = _costToUnlock >= 0 && currentPoints >= _costToUnlock && !entity.IsUnlocked;
            bool hasVideo = _videoClipBinds != null && _videoClipBinds.ContainsKey(nodeId);
            SkillDetailDTO dto = new SkillDetailDTO(
                entity.SkillNodeIdVO.Id,
                entity.SkillDetail,
                _costToUnlock,
                canUnlock,
                entity.IsUnlocked,
                hasVideo);
            _skillDetailPresenter.Push(dto);
            _skillDetailView.Show(token);
            view.SetSelected();
        }

        /// <summary>
        ///     スキルを解放した時の処理。
        /// </summary>
        public void OnSkillUnlocked()
        {
            if (_selectedNodeId == -1 || _costToUnlock < 0) return;
            if (_skillTreeStatusEntity.CurrentPoints < _costToUnlock) return;
            if (_nodesOnPath == null || _nodesOnPath.Count == 0)
            {
                Debug.LogError($"[SkillTreeController] 解放対象ノードの取得に失敗しました。");
            }
            foreach (SkillNodeEntity entity in _nodesOnPath)
            {
                int nodeId = entity.SkillNodeIdVO.Id;
                // TODO 実装待ち：スキル効果をプレイヤーに反映する処理
                entity.Unlock();
                if (!_skillTreeStatusEntity.UnlockedNodes.Contains(entity.SkillNodeIdVO))
                {
                    _skillTreeStatusEntity.AddUnlockedNode(entity.SkillNodeIdVO);
                }
                _skillTreeStatusEntity.AddUnlockedSkillIds(entity.UnlockSkillIds);

                _skillNodeViews[entity.SkillNodeIdVO.Id].SetUnlocked();
                UpdateConns(nodeId);
                UpdateUnlockPhase(nodeId);
            }
            _skillTreeStatusEntity.ModifyPoint(-_costToUnlock);

            _skillTreeService
                .SaveSkillUnlockData(_skillTreeStatusEntity.UnlockedNodes, _skillTreeStatusEntity.UnlockedSkillIds, _skillTreeStatusEntity.CurrentPoints)
                .ContinueWith(
                    t => Debug.LogError($"[SkillTreeController] スキル解放データ保存失敗: {t.Exception}"),
                    TaskContinuationOptions.OnlyOnFaulted);

            SkillNodeEntity selectedNode = _skillNodeEntities[new SkillNodeId(_selectedNodeId)];
            bool hasVideo = _videoClipBinds != null && _videoClipBinds.ContainsKey(_selectedNodeId);
            SkillDetailDTO dto = new SkillDetailDTO(selectedNode.SkillNodeIdVO.Id, selectedNode.SkillDetail, -1, false, selectedNode.IsUnlocked, hasVideo);
            _skillDetailPresenter.Push(dto);
            _currentPointsLabel.text = CURRENT_POINTS_LABEL_TEXT + _skillTreeStatusEntity.CurrentPoints.ToString();
            _playerStatusPresenter.Push();
            _ownedSkillChanged?.Invoke();
        }

        /// <summary>
        ///     スキル詳細画面を閉じたときの処理。
        /// </summary>
        public void OnSkillDetailClosed()
        {
            if (_selectedNodeId == -1) return;
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
            if (_selectedNodeId == -1) return;
            _previewVideoScreenViewShowable.Show(token);
            _previewVideoScreenView.PlayPreviewVideo(_selectedNodeId);
        }

        private Dictionary<SkillNodeId, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private HashSet<SkillNodeEntity> _nodesOnPath;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<string, ISkillNodeConnViewModel> _nodeConns;
        private Dictionary<int, VideoClip> _videoClipBinds;
        private ISkillDetailShowable _skillDetailView;
        private SkillDetailPresenter _skillDetailPresenter;
        private SkillTreeService _skillTreeService;
        private PlayerStatusPresenter _playerStatusPresenter;
        private Label _currentPointsLabel;
        private SkillTreeStatusEntity _skillTreeStatusEntity;
        private IPreviewVideoScreenViewModel _previewVideoScreenView;
        private IPreviewVideoScreenViewShowable _previewVideoScreenViewShowable;
        private Action _ownedSkillChanged;
        private int _costToUnlock = -1;
        private int _selectedNodeId = -1;

        private const string CURRENT_POINTS_LABEL_TEXT = "所持ポイント：";

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
            if (!_skillNodeConnBinds.TryGetValue(nodeId, out string[] connNames) ||
                    connNames == null || connNames.Length == 0) return;
            for (int i = 0; i < connNames.Length; i++)
            {
                if (_nodeConns.TryGetValue(connNames[i], out ISkillNodeConnViewModel conn))
                {
                    conn.SetPassed();
                }
            }
        }
    }
}
