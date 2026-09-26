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
        /// <summary>
        ///     スキルツリー画面の表示・サービス・プレゼンターを指定して生成する。
        /// </summary>
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
            IReadOnlyDictionary<SkillType, Sprite> skillGenreIcons,
            IReadOnlyDictionary<int, Color> skillBeatColors,
            Action<int> showCurrentPoints,
            Func<string> getListSeparator)
        {
            // 受け取った表示・サービス・データを保持する。
            _skillRepository = skillRepository;
            _skillDisplayTextFormatter = skillDisplayTextFormatter;
            _skillGenreIcons = skillGenreIcons;
            _skillBeatColors = skillBeatColors;
            _skillDetailPresenter = presenter;
            _currentPointsLabel = currentPointsLabel;
            _showCurrentPoints = showCurrentPoints;
            _getListSeparator = getListSeparator;
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

            // 現在のポイントを表示する。
            _showCurrentPoints(_skillTreeStatusEntity.CurrentPoints);
        }

        /// <summary> 現在選択中のノードIDを取得する。未選択の場合は<see cref="NO_SELECTION"/>。 </summary>
        public int SelectedNodeId => _selectedNodeId;

        /// <summary>
        ///     ノード未選択を表すセンチネル値。
        ///     ノードIDは<see cref="KillChord.Runtime.Utility.Identity.DataID"/>のハッシュ値であり負値も取り得るため、
        ///     未選択判定は必ずこの値との一致で行うこと。範囲比較(&lt; 0)にすると負IDのノードを未選択と誤判定する。
        /// </summary>
        private const int NO_SELECTION = -1;

        /// <summary> 連続解放演出において、ノード1つあたりの演出開始をずらす間隔(ミリ秒)。Composition層のカメラ演出時間算出にも使用する。 </summary>
        public const long UNLOCK_STAGGER_INTERVAL_MILLISECONDS = 90L;

        /// <summary>
        ///     スキルノードが選択された時の処理。
        /// </summary>
        /// <param name="nodeId"></param>
        public void OnSkillNodeSelected(int nodeId)
        {
            if (_selectedNodeId != NO_SELECTION)
            {
                _skillNodeViews[_selectedNodeId].SetUnSelected();
            }
            _selectedNodeId = nodeId;
            SkillNodeId selectedNodeId = new SkillNodeId(nodeId);
            ISkillNodeViewModel view = _skillNodeViews[nodeId];
            _nodesOnPath.Clear();
            _costToUnlock = _skillTreeService.TryGetTotalCost(selectedNodeId, _nodesOnPath);

            RefreshSelectedText();
            _skillDetailView.Show();
            view.SetSelected();
            _playerStatusPresenter.PushPreview(_nodesOnPath);
        }

        /// <summary>
        ///     選択状態や表示画面を変えず、選択中ノードの翻訳に依存する表示を更新する。
        /// </summary>
        public void RefreshSelectedText()
        {
            // ノードが選ばれていなければ何もしない。
            if (_selectedNodeId == NO_SELECTION)
            {
                return;
            }

            // 選択中のノードについて、解放できるかと表示内容を求める。
            int nodeId = _selectedNodeId;
            SkillNodeEntity entity = _skillNodeEntities[new SkillNodeId(nodeId)];
            int currentPoints = _skillTreeStatusEntity.CurrentPoints;
            bool canUnlock = _costToUnlock >= 0 && currentPoints >= _costToUnlock && !entity.IsUnlocked;
            bool hasVideo = _videoClipBinds != null && _videoClipBinds.ContainsKey(nodeId);
            SkillDetailDTO dto = new SkillDetailDTO(
                entity.SkillNodeIdVO.Id,
                HasUnlockSkill(entity.UnlockSkillIds),
                ResolveSkillName(entity.UnlockSkillIds),
                ResolveSkillCommand(entity.UnlockSkillIds),
                ResolveSkillGenre(entity.UnlockSkillIds),
                ResolveSkillGenreIcon(entity.UnlockSkillIds),
                ResolveSkillIcon(entity.UnlockSkillIds),
                entity.SkillDetail,
                _costToUnlock,
                canUnlock,
                entity.IsUnlocked,
                hasVideo,
                ResolveComboStepColors(entity.UnlockSkillIds));
            // スキル詳細の表示を更新する。
            _skillDetailPresenter.Push(dto);
        }

        /// <summary>
        ///     選択中ノードの解放確認ダイアログに表示するデータを組み立てる。
        /// </summary>
        /// <returns> 解放確認ダイアログ用のDTO。 </returns>
        public UnlockConfirmDTO GetUnlockConfirmation()
        {
            PlayerStatusDTO statusPreview = _playerStatusPresenter.BuildPreview(_nodesOnPath);
            return new UnlockConfirmDTO(
                ResolvePathSkillNames(_nodesOnPath),
                _skillTreeStatusEntity.CurrentPoints,
                _costToUnlock,
                statusPreview.PlayerHealth,
                statusPreview.PreviewPlayerHealth,
                statusPreview.PlayerAttack,
                statusPreview.PreviewPlayerAttack,
                statusPreview.CriticalChance,
                statusPreview.PreviewCriticalChance,
                statusPreview.CriticalDamage,
                statusPreview.PreviewCriticalDamage,
                statusPreview.AreaAttackRangeMultiplier,
                statusPreview.PreviewAreaAttackRangeMultiplier);
        }

        /// <summary>
        ///     現在解放待ちとなっているノードのID一覧を取得する。
        ///     連続解放演出のカメラワーク算出などで使用する。
        /// </summary>
        /// <returns> 解放待ちノードのID一覧。解放対象が無い場合は空配列。 </returns>
        public IReadOnlyList<int> GetPendingUnlockNodeIds()
        {
            if (_nodesOnPath == null || _nodesOnPath.Count == 0)
            {
                return Array.Empty<int>();
            }

            int[] nodeIds = new int[_nodesOnPath.Count];
            int index = 0;
            foreach (SkillNodeEntity node in _nodesOnPath)
            {
                nodeIds[index] = node.SkillNodeIdVO.Id;
                index++;
            }

            return nodeIds;
        }

        /// <summary>
        ///     連続解放のカメラワークで画面に収めるべきノードのID一覧を取得する。
        ///     解放待ちノードに加えて、それらへ直接つながる解放済みノード(接続元)も含める。
        /// </summary>
        /// <returns> フレーミング対象のノードID一覧。解放対象が無い場合は空配列。 </returns>
        public IReadOnlyList<int> GetUnlockCameraFramingNodeIds()
        {
            if (_nodesOnPath == null || _nodesOnPath.Count == 0)
            {
                return Array.Empty<int>();
            }

            HashSet<int> framingNodeIds = new HashSet<int>();
            foreach (SkillNodeEntity node in _nodesOnPath)
            {
                framingNodeIds.Add(node.SkillNodeIdVO.Id);
                if (node.Parents == null)
                {
                    continue;
                }

                for (int i = 0; i < node.Parents.Length; i++)
                {
                    SkillNodeEntity parent = node.Parents[i];
                    // 経路上に含まれない親は、既に解放済みの直接の接続元。
                    if (parent != null && !_nodesOnPath.Contains(parent))
                    {
                        framingNodeIds.Add(parent.SkillNodeIdVO.Id);
                    }
                }
            }

            int[] result = new int[framingNodeIds.Count];
            framingNodeIds.CopyTo(result);
            return result;
        }

        /// <summary>
        ///     スキルを解放した時の処理。
        /// </summary>
        public void OnSkillUnlocked()
        {
            if (_selectedNodeId == NO_SELECTION || _costToUnlock < 0) return;
            if (_skillTreeStatusEntity.CurrentPoints < _costToUnlock) return;
            if (_nodesOnPath == null || _nodesOnPath.Count == 0)
            {
                Debug.LogError($"[SkillTreeController] 解放対象ノードの取得に失敗しました。");
                return;
            }

            List<SkillNodeEntity> unlockOrder = BuildUnlockOrder(_nodesOnPath);
            foreach (SkillNodeEntity entity in unlockOrder)
            {
                // TODO 実装待ち：スキル効果をプレイヤーに反映する処理
                entity.Unlock();
                if (!_skillTreeStatusEntity.UnlockedNodes.Contains(entity.SkillNodeIdVO))
                {
                    _skillTreeStatusEntity.AddUnlockedNode(entity.SkillNodeIdVO);
                }
                _skillTreeStatusEntity.AddUnlockedSkillIds(entity.UnlockSkillIds);
            }
            _skillTreeStatusEntity.ModifyPoint(-_costToUnlock);
            _skillTreeStatusEntity.SetSkillSlotBonus(
                _skillTreeService.CalculateSkillSlotBonus(_skillTreeStatusEntity.UnlockedNodes));

            PlayUnlockSequence(unlockOrder);

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
                ResolveSkillIcon(selectedNode.UnlockSkillIds),
                selectedNode.SkillDetail,
                -1,
                false,
                selectedNode.IsUnlocked,
                hasVideo,
                ResolveComboStepColors(selectedNode.UnlockSkillIds));
            _skillDetailPresenter.Push(dto);
            _showCurrentPoints(_skillTreeStatusEntity.CurrentPoints);
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
            if (_selectedNodeId == NO_SELECTION) return;
            _skillNodeViews[_selectedNodeId].SetUnSelected();
            _nodesOnPath.Clear();
            _selectedNodeId = NO_SELECTION;
            _playerStatusPresenter.Push();
        }

        /// <summary>
        ///     スキルプレビューボタンを押した時の処理。
        /// </summary>
        public void OnPreviewButtonClicked()
        {
            if (_selectedNodeId == NO_SELECTION) return;
            _previewVideoScreenViewShowable.Show();
            _previewVideoScreenView.PlayPreviewVideo(_selectedNodeId);
        }

        /// <summary>
        ///     再生中の連続解放演出があれば、すべて即座に完了させる。入力によるスキップで使用する。
        /// </summary>
        /// <returns> スキップする演出が存在した場合はtrue。 </returns>
        public bool SkipUnlockAnimation()
        {
            if (_pendingUnlockAnimations.Count == 0)
            {
                return false;
            }

            List<(IVisualElementScheduledItem Item, int NodeId)> remaining =
                new(_pendingUnlockAnimations);
            _pendingUnlockAnimations.Clear();
            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].Item.Pause();
                ApplyNodeUnlockVisual(remaining[i].NodeId);
            }

            return true;
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
        private IReadOnlyDictionary<int, Color> _skillBeatColors;
        private ISkillDetailShowable _skillDetailView;
        private SkillDetailPresenter _skillDetailPresenter;
        private SkillTreeService _skillTreeService;
        private PlayerStatusPresenter _playerStatusPresenter;
        private Label _currentPointsLabel;
        private readonly Action<int> _showCurrentPoints;
        private readonly Func<string> _getListSeparator;
        private SkillTreeStatusEntity _skillTreeStatusEntity;
        private IPreviewVideoScreenViewModel _previewVideoScreenView;
        private IPreviewVideoScreenViewShowable _previewVideoScreenViewShowable;
        private Action _ownedSkillChanged;
        private int _costToUnlock = -1;
        private int _selectedNodeId = NO_SELECTION;
        private bool _isResetting;
        private readonly List<(IVisualElementScheduledItem Item, int NodeId)> _pendingUnlockAnimations = new();

        private const string COMMAND_SEPARATOR = " → ";
        private static readonly Color DEFAULT_COMBO_STEP_COLOR = Color.gray;

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
                    builder.Append(_getListSeparator());
                }
                builder.Append(skillData.DisplayName);
            }

            return builder.ToString();
        }

        /// <summary>
        ///     解放対象ノード群(選択ノード自身を含む)全体で解放されるスキル名を一覧化する。
        /// </summary>
        /// <param name="nodesOnPath"> 解放対象ノード群。 </param>
        /// <returns> 解放されるスキル名一覧。1件も解放しない場合は空配列。 </returns>
        private string[] ResolvePathSkillNames(HashSet<SkillNodeEntity> nodesOnPath)
        {
            if (_skillRepository == null || nodesOnPath == null || nodesOnPath.Count == 0)
            {
                return Array.Empty<string>();
            }

            List<string> names = new List<string>();
            foreach (SkillNodeEntity node in nodesOnPath)
            {
                if (node.UnlockSkillIds == null)
                {
                    continue;
                }

                for (int i = 0; i < node.UnlockSkillIds.Length; i++)
                {
                    if (_skillRepository.TryGetSkill(node.UnlockSkillIds[i], out SkillTemplate skillData))
                    {
                        names.Add(skillData.DisplayName);
                    }
                }
            }

            return names.ToArray();
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
                    builder.Append(_getListSeparator());
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
                    builder.Append(_getListSeparator());
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
        ///     ノードが解放する最初のスキル固有のアイコンを解決する。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> スキル固有のアイコン。解決できない場合はnull。 </returns>
        private Sprite ResolveSkillIcon(SkillId[] skillIds)
        {
            if (_skillRepository == null || skillIds == null)
            {
                return null;
            }

            foreach (SkillId skillId in skillIds)
            {
                if (_skillRepository.TryGetSkill(skillId, out SkillTemplate skillData))
                {
                    return skillData.Icon;
                }
            }

            return null;
        }

        /// <summary>
        ///     ノードが解放するスキルの発動コマンドに対応する色一覧を解決する。
        ///     複数スキルの場合はコマンド表示と同様に、入力順を保ったまま連結する。
        /// </summary>
        /// <param name="skillIds"> 解放対象のスキルID一覧。 </param>
        /// <returns> 発動コマンドの入力順に並んだ色一覧。解決できない場合は空配列。 </returns>
        private Color[] ResolveComboStepColors(SkillId[] skillIds)
        {
            if (_skillRepository == null || skillIds == null || skillIds.Length == 0)
            {
                return Array.Empty<Color>();
            }

            List<Color> colors = new List<Color>();
            for (int i = 0; i < skillIds.Length; i++)
            {
                if (!_skillRepository.TryGetSkill(skillIds[i], out SkillTemplate skillData)
                    || skillData.Pattern == null)
                {
                    continue;
                }

                for (int j = 0; j < skillData.Pattern.Length; j++)
                {
                    colors.Add(_skillBeatColors != null
                        && _skillBeatColors.TryGetValue((int)skillData.Pattern[j], out Color color)
                        ? color
                        : DEFAULT_COMBO_STEP_COLOR);
                }
            }

            return colors.ToArray();
        }

        /// <summary>
        ///     解放対象ノードを、根本(起点に近い側)から先端へ向かう順序に並び替える。
        ///     連続解放時の演出を根本から順に再生するために使用する。
        /// </summary>
        /// <param name="nodesOnPath"> 解放対象ノード群。 </param>
        /// <returns> 根本側から順に並んだノード一覧。 </returns>
        private static List<SkillNodeEntity> BuildUnlockOrder(HashSet<SkillNodeEntity> nodesOnPath)
        {
            Dictionary<SkillNodeEntity, int> depthByNode = new(nodesOnPath.Count);
            foreach (SkillNodeEntity node in nodesOnPath)
            {
                ComputeUnlockDepth(node, nodesOnPath, depthByNode);
            }

            List<SkillNodeEntity> ordered = new(nodesOnPath);
            ordered.Sort((left, right) =>
            {
                int depthCompare = depthByNode[left].CompareTo(depthByNode[right]);
                return depthCompare != 0
                    ? depthCompare
                    : left.SkillNodeIdVO.Id.CompareTo(right.SkillNodeIdVO.Id);
            });
            return ordered;
        }

        /// <summary>
        ///     解放対象ノード群の中における、根本からの深さを算出する。
        /// </summary>
        /// <param name="node"> 深さを算出するノード。 </param>
        /// <param name="nodesOnPath"> 解放対象ノード群。 </param>
        /// <param name="depthByNode"> 算出済みの深さのキャッシュ。 </param>
        /// <returns> 根本からの深さ(根本は0)。 </returns>
        private static int ComputeUnlockDepth(
            SkillNodeEntity node,
            HashSet<SkillNodeEntity> nodesOnPath,
            Dictionary<SkillNodeEntity, int> depthByNode)
        {
            if (depthByNode.TryGetValue(node, out int cachedDepth))
            {
                return cachedDepth;
            }

            int maxParentDepth = -1;
            if (node.Parents != null)
            {
                for (int i = 0; i < node.Parents.Length; i++)
                {
                    SkillNodeEntity parent = node.Parents[i];
                    if (parent != null && nodesOnPath.Contains(parent))
                    {
                        int parentDepth = ComputeUnlockDepth(parent, nodesOnPath, depthByNode);
                        maxParentDepth = Math.Max(maxParentDepth, parentDepth);
                    }
                }
            }

            int depth = maxParentDepth + 1;
            depthByNode[node] = depth;
            return depth;
        }

        /// <summary>
        ///     解放対象ノードの見た目の演出(ポップ・接続線)を、根本から順に間隔を空けて再生する。
        /// </summary>
        /// <param name="unlockOrder"> 根本側から順に並んだ解放対象ノード一覧。 </param>
        private void PlayUnlockSequence(List<SkillNodeEntity> unlockOrder)
        {
            for (int i = 0; i < unlockOrder.Count; i++)
            {
                int nodeId = unlockOrder[i].SkillNodeIdVO.Id;
                long delayMilliseconds = i * UNLOCK_STAGGER_INTERVAL_MILLISECONDS;
                IVisualElementScheduledItem scheduledItem = null;
                // 通常発火時は自身を保留リストから取り除き、スキップ時の二重適用を防ぐ。
                scheduledItem = _currentPointsLabel.schedule.Execute(() =>
                {
                    ApplyNodeUnlockVisual(nodeId);
                    RemovePendingUnlockAnimation(scheduledItem);
                }).StartingIn(delayMilliseconds);
                _pendingUnlockAnimations.Add((scheduledItem, nodeId));
            }
        }

        /// <summary>
        ///     ノード1件分の解放演出(ポップ・接続線・解放段階)を即時に適用する。
        /// </summary>
        /// <param name="nodeId"> 対象ノードID。 </param>
        private void ApplyNodeUnlockVisual(int nodeId)
        {
            _skillNodeViews[nodeId].SetUnlocked();
            UpdateConns(nodeId);
            UpdateUnlockPhase(nodeId);
        }

        /// <summary>
        ///     保留中の連続解放演出一覧から、発火済みの項目を取り除く。
        /// </summary>
        /// <param name="scheduledItem"> 発火した演出のスケジュール項目。 </param>
        private void RemovePendingUnlockAnimation(IVisualElementScheduledItem scheduledItem)
        {
            for (int i = 0; i < _pendingUnlockAnimations.Count; i++)
            {
                if (_pendingUnlockAnimations[i].Item == scheduledItem)
                {
                    _pendingUnlockAnimations.RemoveAt(i);
                    return;
                }
            }
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
            // すべてのノードを未解放にし、接続線と解放段階の表示も初期状態に戻す。
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

            // リセット後も解放済みのノードだけを解放し直す。
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

            // ポイントと解放状態を反映し、選択を解除して表示を更新する。
            _skillTreeStatusEntity.Reset(
                result.CurrentPoints,
                result.UnlockedNodeIds,
                result.UnlockedSkillIds);
            _skillTreeStatusEntity.SetSkillSlotBonus(
                _skillTreeService.CalculateSkillSlotBonus(_skillTreeStatusEntity.UnlockedNodes));
            _nodesOnPath.Clear();
            _selectedNodeId = NO_SELECTION;
            _costToUnlock = -1;
            _showCurrentPoints(result.CurrentPoints);
            _playerStatusPresenter.Push();
            _ownedSkillChanged?.Invoke();
        }

    }
}
