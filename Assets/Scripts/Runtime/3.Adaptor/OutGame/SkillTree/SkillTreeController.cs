using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Player;
using System;
using System.Collections.Generic;
using System.Text;
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
            Action ownedSkillChanged,
            ISkillRepository skillRepository,
            SkillDisplayTextFormatter skillDisplayTextFormatter,
            IReadOnlyDictionary<SkillType, Sprite> skillGenreIcons)
        {
            _skillRepository = skillRepository;
            _skillDisplayTextFormatter = skillDisplayTextFormatter;
            _skillGenreIcons = skillGenreIcons;
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
        public void OnSkillNodeSelected(int nodeId)
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
                HasUnlockSkill(entity.UnlockSkillIds),
                ResolveSkillName(entity.UnlockSkillIds),
                ResolveSkillCommand(entity.UnlockSkillIds),
                ResolveSkillGenre(entity.UnlockSkillIds),
                ResolveSkillGenreIcon(entity.UnlockSkillIds),
                entity.SkillDetail,
                _costToUnlock,
                canUnlock,
                entity.IsUnlocked,
                hasVideo);
            _skillDetailPresenter.Push(dto);
            _skillDetailView.Show();
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
            SkillDetailDTO dto = new SkillDetailDTO(
                selectedNode.SkillNodeIdVO.Id,
                HasUnlockSkill(selectedNode.UnlockSkillIds),
                ResolveSkillName(selectedNode.UnlockSkillIds),
                ResolveSkillCommand(selectedNode.UnlockSkillIds),
                ResolveSkillGenre(selectedNode.UnlockSkillIds),
                ResolveSkillGenreIcon(selectedNode.UnlockSkillIds),
                selectedNode.SkillDetail,
                -1,
                false,
                selectedNode.IsUnlocked,
                hasVideo);
            _skillDetailPresenter.Push(dto);
            _currentPointsLabel.text = CURRENT_POINTS_LABEL_TEXT + _skillTreeStatusEntity.CurrentPoints.ToString();
            _playerStatusPresenter.Push();
            _ownedSkillChanged?.Invoke();
        }

        /// <summary>
        ///     スキルツリーをリセットした場合に返却される研究ポイントを取得する。
        /// </summary>
        /// <returns> 返却予定の研究ポイント。 </returns>
        public int GetResetRefundPoints()
        {
            return _skillTreeService.CalculateResetRefundPoints(_skillTreeStatusEntity.UnlockedNodes);
        }

        /// <summary>
        ///     スキルツリーをリセットして保存し、画面表示を更新する。
        /// </summary>
        /// <returns> リセットに成功した場合はtrue。 </returns>
        public async Task<bool> ResetSkillTreeAsync(CancellationToken cancellationToken)
        {
            if (_isResetting || GetResetRefundPoints() <= 0)
            {
                return false;
            }

            _isResetting = true;
            try
            {
                SkillTreeResetResult result = await _skillTreeService.ResetSkillTreeAsync(
                    _skillTreeStatusEntity.UnlockedNodes,
                    _skillTreeStatusEntity.UnlockedSkillIds,
                    _skillTreeStatusEntity.CurrentPoints);
                cancellationToken.ThrowIfCancellationRequested();
                ApplyResetResult(result);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(SkillTreeController)}] スキルツリーのリセットに失敗しました: {exception}");
                return false;
            }
            finally
            {
                _isResetting = false;
            }
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
        public void OnPreviewButtonClicked()
        {
            if (_selectedNodeId == -1) return;
            _previewVideoScreenViewShowable.Show();
            _previewVideoScreenView.PlayPreviewVideo(_selectedNodeId);
        }

        private Dictionary<SkillNodeId, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private HashSet<SkillNodeEntity> _nodesOnPath;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<string, ISkillNodeConnViewModel> _nodeConns;
        private Dictionary<int, VideoClip> _videoClipBinds;
        private ISkillRepository _skillRepository;
        private SkillDisplayTextFormatter _skillDisplayTextFormatter;
        private IReadOnlyDictionary<SkillType, Sprite> _skillGenreIcons;
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
        private bool _isResetting;

        private const string CURRENT_POINTS_LABEL_TEXT = "所持ポイント：";
        private const string SKILL_NAME_SEPARATOR = "、";
        private const string COMMAND_SEPARATOR = " → ";

        /// <summary>
        ///     ノードがスキルを解放するかどうかを判定する。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> 1件以上のスキルを解放する場合は true。 </returns>
        private static bool HasUnlockSkill(SkillId[] skillIds)
        {
            return skillIds != null && skillIds.Length > 0;
        }

        /// <summary>
        ///     ノードが解放するスキルの名前を解決する。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> スキル名を「、」で連結した文字列。解決できない場合は空文字列。 </returns>
        private string ResolveSkillName(SkillId[] skillIds)
        {
            if (_skillRepository == null || skillIds == null || skillIds.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < skillIds.Length; i++)
            {
                if (!_skillRepository.TryGetSkill(skillIds[i], out SkillTemplate skillData))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(SKILL_NAME_SEPARATOR);
                }
                builder.Append(skillData.DisplayName);
            }

            return builder.ToString();
        }

        /// <summary>
        ///     ノードが解放するスキルの発動コマンドを解決する(値のみ、キャプション無し)。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> 発動コマンド表示を「、」で連結した文字列。解決できない場合は空文字列。 </returns>
        private string ResolveSkillCommand(SkillId[] skillIds)
        {
            if (_skillRepository == null || skillIds == null || skillIds.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < skillIds.Length; i++)
            {
                if (!_skillRepository.TryGetSkill(skillIds[i], out SkillTemplate skillData))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(SKILL_NAME_SEPARATOR);
                }
                builder.Append(BuildCommandLabel(skillData.Pattern));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     入力パターンから「2 → 4 → 1」形式のコマンド表示を構築する。
        /// </summary>
        /// <param name="pattern"> 入力パターン。 </param>
        /// <returns> コマンド表示文字列。 </returns>
        private static string BuildCommandLabel(BeatType[] pattern)
        {
            if (pattern == null || pattern.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < pattern.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(COMMAND_SEPARATOR);
                }
                builder.Append((int)pattern[i]);
            }

            return builder.ToString();
        }

        /// <summary>
        ///     ノードが解放するスキルのジャンルを解決する(値のみ、キャプション無し)。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> ジャンル表示を「、」で連結した文字列。解決できない場合は空文字列。 </returns>
        private string ResolveSkillGenre(SkillId[] skillIds)
        {
            if (_skillRepository == null || _skillDisplayTextFormatter == null || skillIds == null || skillIds.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < skillIds.Length; i++)
            {
                if (!_skillRepository.TryGetSkill(skillIds[i], out SkillTemplate skillData))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(SKILL_NAME_SEPARATOR);
                }
                builder.Append(_skillDisplayTextFormatter.Format(skillData).SkillTypeLabel);
            }

            return builder.ToString();
        }

        /// <summary>
        ///     ノードが解放するスキルのジャンルアイコンを解決する。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> 最初に解決できたスキルの最初のジャンルのアイコン。解決できない場合は null。 </returns>
        private Sprite ResolveSkillGenreIcon(SkillId[] skillIds)
        {
            if (_skillRepository == null || _skillGenreIcons == null || skillIds == null || skillIds.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < skillIds.Length; i++)
            {
                if (!_skillRepository.TryGetSkill(skillIds[i], out SkillTemplate skillData))
                {
                    continue;
                }

                if (skillData.Type == null || skillData.Type.Length == 0)
                {
                    continue;
                }

                return _skillGenreIcons.TryGetValue(skillData.Type[0], out Sprite icon) ? icon : null;
            }

            return null;
        }

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

        /// <summary>
        ///     保存済みのリセット結果をDomainとViewへ反映する。
        /// </summary>
        /// <param name="result"> 保存済みのリセット結果。 </param>
        private void ApplyResetResult(SkillTreeResetResult result)
        {
            foreach (SkillNodeEntity node in _skillNodeEntities.Values)
            {
                node.Lock();
                _skillNodeViews[node.SkillNodeIdVO.Id].SetLocked();
            }

            foreach (ISkillNodeConnViewModel connectionView in _nodeConns.Values)
            {
                connectionView.SetNotPassed();
            }

            foreach (VisualElement unlockPhase in _unlockPhases.Values)
            {
                if (unlockPhase != null)
                {
                    unlockPhase.visible = false;
                }
            }

            ReadOnlySpan<SkillNodeId> unlockedNodeIds = result.UnlockedNodeIds.Span;
            for (int i = 0; i < unlockedNodeIds.Length; i++)
            {
                SkillNodeId nodeId = unlockedNodeIds[i];
                if (!_skillNodeEntities.TryGetValue(nodeId, out SkillNodeEntity node))
                {
                    continue;
                }

                node.Unlock();
                _skillNodeViews[nodeId.Id].SetUnlocked();
                UpdateConns(nodeId.Id);
                UpdateUnlockPhase(nodeId.Id);
            }

            _skillTreeStatusEntity.Reset(
                result.CurrentPoints,
                result.UnlockedNodeIds,
                result.UnlockedSkillIds);
            _nodesOnPath.Clear();
            _selectedNodeId = -1;
            _costToUnlock = -1;
            _currentPointsLabel.text = CURRENT_POINTS_LABEL_TEXT + result.CurrentPoints.ToString();
            _playerStatusPresenter.Push();
            _ownedSkillChanged?.Invoke();
        }

    }
}
