using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.OutGame.SkillTree;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.Utility.OutGame;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillTree;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace KillChord.Runtime.Composition.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリーを初期化するクラス。
    /// </summary>
    public sealed class SkillTreeInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SkillTreeInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 120;

        private const string E_NAME_SKILL_DETAIL = "SkillDetail";
        private const string E_NAME_PLAYER_STATUS = "PlayerStatus";
        private const string E_NAME_PREVIEW_VIDEO_CONTAINER = "PreviewVideoContainer";
        private const string E_NAME_PREVIEW_VIDEO = "PreviewVideo";
        private const string E_NAME_CURRENT_POINTS_LABEL = "Points";
        private const float DEFAULT_CRITICAL_DAMAGE_MULTIPLIER = 1f;

        [SerializeField]
        [Tooltip("スキルツリー画面のUIDocumentです。")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("表示するプレイヤーの基礎ステータス定義です。")]
        private CharacterDefinitionAsset _playerData;

        [SerializeField, SourceDataAddress]
        [Tooltip("スキルノード定義リポジトリの Addressables キーです。")]
        private string _skillNodeDataRepoKey;

        [SerializeField, SourceDataAddress]
        [Tooltip("スキルノード接続定義リポジトリの Addressables キーです。")]
        private string _skillNodeBindRepoKey;

        [SerializeField, SourceDataAddress]
        [Tooltip("スキルツリー段階定義リポジトリの Addressables キーです。")]
        private string _skillNodePhaseBindRepoKey;

        [SerializeField]
        [Tooltip("スキルプレビュー動画を再生する VideoPlayer です。")]
        private VideoPlayer _videoPlayer;

        private VisualElement _rootElement;
        private VisualElement _skillDetailRoot;
        private VisualElement _playerStatusRoot;
        private VisualElement _previewVideoContainerRoot;
        private VisualElement _previewVideoRoot;
        private Label _currentPointsLabel;
        private SkillDetailScreenView _skillDetailScreenView;
        private PlayerStatusScreenView _playerStatusScreenView;
        private PreviewVideoScreenView _previewVideoScreenView;
        private SkillTreeResetDialogView _skillTreeResetDialogView;
        private SkillTreeViewportView _skillTreeViewportView;
        private SkillTreeController _skillTreeController;
        private SkillTreeService _skillTreeService;
        private SkillDetailPresenter _skillDetailPresenter;
        private PlayerStatusPresenter _playerStatusPresenter;
        private SkillTreeFocusPresenter _skillTreeFocusPresenter;
        private SkillUnlockData _skillUnlockData;
        private OutGameUIEvent _outGameUIEvent;
        private CancellationTokenSource _cts;
        private RenderTexture _renderTexture;
        private Dictionary<SkillNodeId, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private Dictionary<int, VisualElement> _skillNodeElements;
        private Dictionary<string, ISkillNodeConnViewModel> _skillNodeConnViews;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, VideoClip> _skillPreviewVideos;
        private SkillNodeDataRepo _loadedSkillNodeDataRepo;
        private SkillNodeBindRepo _loadedSkillNodeBindRepo;
        private SkillNodePhaseBindDataRepo _loadedSkillNodePhaseBindRepo;
        private bool _isInitialized;
        private bool _isSubscribed;

        /// <summary>
        ///     非同期のリソースロードを行います。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _loadedSkillNodeDataRepo = await _skillNodeDataRepoKey.LoadAssetAsync<SkillNodeDataRepo>(this, destroyCancellationToken);
            _loadedSkillNodeBindRepo = await _skillNodeBindRepoKey.LoadAssetAsync<SkillNodeBindRepo>(this, destroyCancellationToken);
            _loadedSkillNodePhaseBindRepo =
                await _skillNodePhaseBindRepoKey.LoadAssetAsync<SkillNodePhaseBindDataRepo>(this, destroyCancellationToken);

            if (_loadedSkillNodeDataRepo == null
                || _loadedSkillNodeBindRepo == null
                || _loadedSkillNodePhaseBindRepo == null)
            {
                return false;
            }

            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            if (saveData == null)
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] SaveData が取得できませんでした。", this);
                return false;
            }

            _skillUnlockData = saveData.SkillUnlock;
            return _skillUnlockData != null;
        }

        /// <summary>
        ///     システムを構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            return Initialize();
        }

        /// <summary>
        ///     他モジュールとの結合を行います。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isInitialized)
            {
                return false;
            }

            _cts = new CancellationTokenSource();
            Subscribe();
            return true;
        }

        /// <summary>
        ///     登録済みサービスやイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            Unsubscribe();
            DisposeComponents();
            CancelAndDisposeCts();

            _skillNodeDataRepoKey.ReleaseLoadedAsset(this);
            _skillNodeBindRepoKey.ReleaseLoadedAsset(this);
            _skillNodePhaseBindRepoKey.ReleaseLoadedAsset(this);
            _loadedSkillNodeDataRepo = null;
            _loadedSkillNodeBindRepo = null;
            _loadedSkillNodePhaseBindRepo = null;
            _skillUnlockData = null;
            _outGameUIEvent = null;
            _isInitialized = false;
            _isSubscribed = false;
        }

        /// <summary>
        ///     永続化された数値IDをスキルノードIDへ変換します。
        /// </summary>
        /// <param name="values"> 永続化された数値ID。 </param>
        /// <returns> スキルノードID配列。 </returns>
        private static SkillNodeId[] CreateSkillNodeIds(IReadOnlyList<int> values)
        {
            SkillNodeId[] ids = new SkillNodeId[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                ids[i] = new SkillNodeId(values[i]);
            }

            return ids;
        }

        /// <summary>
        ///     永続化された数値IDをスキルIDへ変換します。
        /// </summary>
        /// <param name="values"> 永続化された数値ID。 </param>
        /// <returns> スキルID配列。 </returns>
        private static SkillId[] CreateSkillIds(IReadOnlyList<int> values)
        {
            SkillId[] ids = new SkillId[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                ids[i] = new SkillId(values[i]);
            }

            return ids;
        }

        /// <summary>
        ///     スキルツリーを構築します。
        /// </summary>
        /// <returns> 初期化に成功した場合はtrue。 </returns>
        private bool Initialize()
        {
            if (_uiDocument == null)
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] UIDocument が設定されていません。", this);
                return false;
            }

            if (_videoPlayer == null)
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] VideoPlayer が設定されていません。", this);
                return false;
            }

            if (_playerData == null)
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] プレイヤー定義アセットが設定されていません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] OutGameUIEvent が取得できませんでした。", this);
                return false;
            }

            _rootElement = _uiDocument.rootVisualElement;
            _skillDetailRoot = _rootElement.Q<VisualElement>(E_NAME_SKILL_DETAIL);
            _playerStatusRoot = _rootElement.Q<VisualElement>(E_NAME_PLAYER_STATUS);
            _previewVideoContainerRoot = _rootElement.Q<VisualElement>(E_NAME_PREVIEW_VIDEO_CONTAINER);
            _previewVideoRoot = _rootElement.Q<VisualElement>(E_NAME_PREVIEW_VIDEO);
            _currentPointsLabel = _rootElement.Q<Label>(E_NAME_CURRENT_POINTS_LABEL);

            if (_skillDetailRoot == null
                || _playerStatusRoot == null
                || _previewVideoContainerRoot == null
                || _previewVideoRoot == null
                || _currentPointsLabel == null)
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] スキルツリー用のUI要素が不足しています。", this);
                return false;
            }

            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.isLooping = true;
            _renderTexture = _videoPlayer.targetTexture;
            _previewVideoRoot.style.backgroundImage = Background.FromRenderTexture(_renderTexture);

            BuildSkillNodes();
            BuildNodeConns();
            BuildConnBinds();
            InitializePhaseState();
            BuildVideoClipDict();

            _skillDetailScreenView = new SkillDetailScreenView(_skillDetailRoot, _outGameUIEvent);
            _skillDetailScreenView.HideImmediately();
            _playerStatusScreenView = new PlayerStatusScreenView(_playerStatusRoot, _outGameUIEvent);
            _previewVideoScreenView = new PreviewVideoScreenView(_previewVideoContainerRoot, _outGameUIEvent, _videoPlayer, _skillPreviewVideos);
            _previewVideoScreenView.HideImmediately();
            _skillTreeResetDialogView = new SkillTreeResetDialogView(_rootElement, _outGameUIEvent);
            _skillTreeViewportView = new SkillTreeViewportView(_rootElement, _skillNodeElements);

            SkillTreeStatusEntity skillTreeEntity = new(
                _skillUnlockData.ResearchPoint,
                CreateSkillNodeIds(_skillUnlockData.UnlockedSkillNodeIds),
                CreateSkillIds(_skillUnlockData.UnlockedSkillIds));
            _skillTreeService = new SkillTreeService(_skillNodeEntities);
            PlayerStatusBonusCalculator playerStatusBonusCalculator =
                new PlayerStatusBonusCalculator(_loadedSkillNodeDataRepo.GetAll());

            _skillDetailPresenter = new SkillDetailPresenter(_skillDetailScreenView);
            _skillTreeFocusPresenter = new SkillTreeFocusPresenter(
                _skillTreeService,
                _skillTreeViewportView);
            _playerStatusPresenter = new PlayerStatusPresenter(
                _playerStatusScreenView,
                playerStatusBonusCalculator,
                skillTreeEntity,
                _playerData.MaxHealth,
                _playerData.BaseDamage,
                GetBaseCriticalChance(),
                GetBaseCriticalDamageMultiplier());
            _playerStatusPresenter.Push();
            _skillTreeController = new SkillTreeController(
                _skillDetailScreenView,
                _skillDetailPresenter,
                _currentPointsLabel,
                _skillTreeService,
                _playerStatusPresenter,
                _previewVideoScreenView,
                _previewVideoScreenView,
                _skillNodeEntities,
                _skillNodeViews,
                _skillNodeConnBinds,
                _skillNodeConnViews,
                _unlockPhases,
                _skillPreviewVideos,
                skillTreeEntity,
                () => _outGameUIEvent.OnOwnedSkillChanged?.Invoke());

            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     プレイヤーの基礎会心率を取得します。
        /// </summary>
        /// <returns> 基礎会心率。 </returns>
        private float GetBaseCriticalChance()
        {
            return _playerData == null ? 0f : _playerData.CriticalChance;
        }

        /// <summary>
        ///     プレイヤーの先頭攻撃定義から基礎会心ダメージ倍率を取得します。
        /// </summary>
        /// <returns> 基礎会心ダメージ倍率。攻撃定義がない場合は1。 </returns>
        private float GetBaseCriticalDamageMultiplier()
        {
            AttackDefinitionAsset[] attackDefinitions = _playerData.AttackDefinitionAssets;
            if (attackDefinitions == null)
            {
                return DEFAULT_CRITICAL_DAMAGE_MULTIPLIER;
            }

            for (int i = 0; i < attackDefinitions.Length; i++)
            {
                if (attackDefinitions[i] != null)
                {
                    return attackDefinitions[i].CriticalDamageMultiplier;
                }
            }

            return DEFAULT_CRITICAL_DAMAGE_MULTIPLIER;
        }

        /// <summary>
        ///     スキルノードと接続線の紐づきを作成します。
        /// </summary>
        private void BuildConnBinds()
        {
            _skillNodeConnBinds = new();
            foreach (SkillNodeBindData bind in _loadedSkillNodeBindRepo.SkillNodeBinds)
            {
                _skillNodeConnBinds.Add(bind.SkillNodeId.Id, bind.FromConnNames);
            }
        }

        /// <summary>
        ///     スキルノードのEntityとViewを作成し、ノードIDとの紐づきを作成します。
        /// </summary>
        private void BuildSkillNodes()
        {
            List<Button> nodes = _rootElement.Query<Button>(className: UssClassNameConstants.USS_CLASS_SKILL_NODE).ToList();
            _skillNodeEntities = new();
            _skillNodeViews = new();
            _skillNodeElements = new();

            for (int i = 0; i < nodes.Count; i++)
            {
                string nodeName = nodes[i].name;
                SkillNodeBindData bind = _loadedSkillNodeBindRepo.FindByName(nodeName);
                SkillNodeData nodeData = bind != null ? _loadedSkillNodeDataRepo.FindNodeData(bind.SkillNodeId) : null;
                if (nodeData == null)
                {
                    throw new KeyNotFoundException($"SkillNodeBindDataが見つかりません：{nodeName}");
                }

                SkillNodeEntity nodeEntity = nodeData.ToDomain();
                SkillNodeView nodeView = new SkillNodeView(nodes[i], nodeData.NodeId.Id, _outGameUIEvent);
                SetNodeUnlockState(nodeView, nodeEntity);

                _skillNodeEntities.Add(nodeData.NodeId, nodeEntity);
                _skillNodeViews.Add(nodeData.NodeId.Id, nodeView);
                _skillNodeElements.Add(nodeData.NodeId.Id, nodes[i]);
            }

            foreach (SkillNodeEntity entity in _skillNodeEntities.Values)
            {
                SkillNodeData data = _loadedSkillNodeDataRepo.FindNodeData(entity.SkillNodeIdVO);
                SkillNodeEntity[] parents = new SkillNodeEntity[data.ParentNodeCount];
                for (int i = 0; i < data.ParentNodeCount; i++)
                {
                    SkillNodeId parentNodeId = data.GetParentNodeId(i);
                    if (!_skillNodeEntities.TryGetValue(parentNodeId, out SkillNodeEntity parent))
                    {
                        throw new KeyNotFoundException(
                            $"親ノードID {parentNodeId.Id} が UI/Bind 構築結果に存在しません。子ノードID: {entity.SkillNodeIdVO.Id}");
                    }

                    parents[i] = parent;
                }

                entity.SetParent(parents);
            }
        }

        /// <summary>
        ///     ノード接続線のViewを作成します。
        /// </summary>
        private void BuildNodeConns()
        {
            List<VisualElement> conns = _rootElement.Query(className: UssClassNameConstants.USS_CLASS_SKILL_NODE_CONN).ToList();
            _skillNodeConnViews = new();
            for (int i = 0; i < conns.Count; i++)
            {
                string name = conns[i].name;
                SkillNodeConnView view = new SkillNodeConnView(conns[i]);
                _skillNodeConnViews.Add(name, view);
            }
        }

        /// <summary>
        ///     スキルノードの初期解放状態を設定します。
        /// </summary>
        /// <param name="view"> 対象ノードViewです。 </param>
        /// <param name="entity"> 対象ノードEntityです。 </param>
        private void SetNodeUnlockState(SkillNodeView view, SkillNodeEntity entity)
        {
            if (_skillUnlockData.UnlockedSkillNodeIds.Contains(entity.SkillNodeIdVO.Id))
            {
                view.SetUnlocked();
                entity.Unlock();
            }
        }

        /// <summary>
        ///     解放段階の初期状態を設定します。
        /// </summary>
        private void InitializePhaseState()
        {
            _unlockPhases = new();
            for (int i = 0; i < _loadedSkillNodePhaseBindRepo.PhaseBindData.Length; i++)
            {
                SkillNodePhaseBindData phaseBindData = _loadedSkillNodePhaseBindRepo.PhaseBindData[i];
                string phaseName = phaseBindData.PhaseName;
                VisualElement phaseRoot = _rootElement.Q(phaseName);
                _unlockPhases.Add(phaseBindData.RequiredSkillNodeId.Id, phaseRoot);
                phaseRoot.visible = false;
            }

            for (int i = 0; i < _skillUnlockData.UnlockedSkillNodeIds.Length; i++)
            {
                SetUnlockPhaseState(_skillUnlockData.UnlockedSkillNodeIds[i]);
            }
        }

        /// <summary>
        ///     解放段階を設定します。
        /// </summary>
        /// <param name="nodeId"> 解放済みノードIDです。 </param>
        private void SetUnlockPhaseState(int nodeId)
        {
            if (_loadedSkillNodePhaseBindRepo.TryGetUnlockPhaseName(new SkillNodeId(nodeId), out string phaseName))
            {
                _rootElement.Q(phaseName).visible = true;
            }
        }

        /// <summary>
        ///     スキルノードのIDとプレビュー動画の紐づきを作成します。
        /// </summary>
        private void BuildVideoClipDict()
        {
            _skillPreviewVideos = new();
            foreach (SkillNodeData node in _loadedSkillNodeDataRepo.SkillNodes)
            {
                if (node.PreviewVideoClip != null)
                {
                    _skillPreviewVideos.Add(node.NodeId.Id, node.PreviewVideoClip);
                }
            }
        }

        /// <summary>
        ///     イベントを購読します。
        /// </summary>
        private void Subscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || _isSubscribed)
            {
                return;
            }

            _outGameUIEvent.OnSkillNodeSelected += HandleSkillNodeSelected;
            _outGameUIEvent.OnSkillDetailClosed += HandleSkillDetailClosed;
            _outGameUIEvent.OnSkillUnlocked += HandleSkillUnlocked;
            _outGameUIEvent.OnSkillTreeResetRequested += HandleSkillTreeResetRequested;
            _outGameUIEvent.OnSkillTreeResetConfirmed += HandleSkillTreeResetConfirmed;
            _outGameUIEvent.OnSkillTreeResetCancelled += HandleSkillTreeResetCancelled;
            _outGameUIEvent.OnSkillPreviewButtonClicked += HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked += HandlePreviewClosed;
            _outGameUIEvent.OnShownSkillTreeScreen += HandleSkillTreeScreenShownHandler;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosedHandler;
            _skillTreeViewportView.OnFocusTargetsRequested += HandleFocusTargetsRequestedHandler;
            _isSubscribed = true;
        }

        /// <summary>
        ///     イベント購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isInitialized || _outGameUIEvent == null || !_isSubscribed)
            {
                return;
            }

            _outGameUIEvent.OnSkillNodeSelected -= HandleSkillNodeSelected;
            _outGameUIEvent.OnSkillDetailClosed -= HandleSkillDetailClosed;
            _outGameUIEvent.OnSkillUnlocked -= HandleSkillUnlocked;
            _outGameUIEvent.OnSkillTreeResetRequested -= HandleSkillTreeResetRequested;
            _outGameUIEvent.OnSkillTreeResetConfirmed -= HandleSkillTreeResetConfirmed;
            _outGameUIEvent.OnSkillTreeResetCancelled -= HandleSkillTreeResetCancelled;
            _outGameUIEvent.OnSkillPreviewButtonClicked -= HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked -= HandlePreviewClosed;
            _outGameUIEvent.OnShownSkillTreeScreen -= HandleSkillTreeScreenShownHandler;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosedHandler;
            _skillTreeViewportView.OnFocusTargetsRequested -= HandleFocusTargetsRequestedHandler;
            _isSubscribed = false;
        }

        /// <summary>
        ///     生成したコンポーネントを解放します。
        /// </summary>
        private void DisposeComponents()
        {
            _previewVideoScreenView?.Dispose();
            _previewVideoScreenView = null;
            _skillTreeResetDialogView?.Dispose();
            _skillTreeResetDialogView = null;
            _skillDetailScreenView?.Dispose();
            _skillDetailScreenView = null;
            _playerStatusScreenView = null;
            _skillTreeViewportView?.Dispose();
            _skillTreeViewportView = null;
            _skillTreeController = null;
            _skillTreeService = null;
            _skillDetailPresenter = null;
            _playerStatusPresenter = null;
            _skillTreeFocusPresenter = null;

            if (_skillNodeViews != null)
            {
                foreach (ISkillNodeViewModel skillNodeViewModel in _skillNodeViews.Values)
                {
                    if (skillNodeViewModel is SkillNodeView skillNodeView)
                    {
                        skillNodeView.Dispose();
                    }
                }
            }

            _skillNodeEntities = null;
            _skillNodeViews = null;
            _skillNodeElements = null;
            _skillNodeConnViews = null;
            _skillNodeConnBinds = null;
            _unlockPhases = null;
            _skillPreviewVideos = null;
        }

        /// <summary>
        ///     キャンセルトークンを解放します。
        /// </summary>
        private void CancelAndDisposeCts()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        /// <summary>
        ///     スキルノードを選択した時の処理です。
        /// </summary>
        /// <param name="nodeName"> 選択されたノード名です。 </param>
        private void HandleSkillNodeSelected(string nodeName)
        {
            SkillNodeId nodeId = _loadedSkillNodeBindRepo.FindByName(nodeName).SkillNodeId;
            _skillTreeController.OnSkillNodeSelected(nodeId.Id);
        }

        /// <summary>
        ///     スキル詳細画面を閉じる時の処理です。
        /// </summary>
        /// <param name="nodeId"> 対象ノードIDです。 </param>
        private void HandleSkillDetailClosed(int nodeId)
        {
            _skillTreeController.OnSkillDetailClosed();
            _skillDetailScreenView.Hide();
        }

        /// <summary>
        ///     スキルを解放する時の処理です。
        /// </summary>
        private void HandleSkillUnlocked()
        {
            _skillTreeController.OnSkillUnlocked();
        }

        /// <summary>
        ///     研究画面が表示された時に初期フォーカス対象を反映する。
        /// </summary>
        private void HandleSkillTreeScreenShownHandler()
        {
            if (_skillTreeViewportView == null)
            {
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] スキルツリーの初期フォーカス表示を要求できませんでした。",
                    this);
                return;
            }

            _skillTreeViewportView.RequestFocus();
        }

        /// <summary>
        ///     画面を閉じる時に保留中の初期フォーカス処理を中止する。
        /// </summary>
        private void HandleScreenClosedHandler()
        {
            _skillTreeViewportView?.CancelFocus();
        }

        /// <summary>
        ///     表示中の候補から初期フォーカス対象を再計算する。
        /// </summary>
        /// <param name="visibleCandidateNodeIds"> 表示中の初期フォーカス候補ノードID。 </param>
        private void HandleFocusTargetsRequestedHandler(
            IReadOnlyList<int> visibleCandidateNodeIds)
        {
            if (_skillTreeFocusPresenter == null
                || !_skillTreeFocusPresenter.Push(visibleCandidateNodeIds))
            {
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] スキルツリーの初期フォーカス対象を取得できませんでした。",
                    this);
            }
        }

        /// <summary>
        ///     スキルツリーリセットの確認ダイアログを表示する。
        /// </summary>
        private void HandleSkillTreeResetRequested()
        {
            int refundPoints = _skillTreeController.GetResetRefundPoints();
            _skillTreeResetDialogView.Show(refundPoints);
        }

        /// <summary>
        ///     スキルツリーリセットを確定して保存する。
        /// </summary>
        private async void HandleSkillTreeResetConfirmed()
        {
            SkillTreeResetDialogView dialogView = _skillTreeResetDialogView;
            SkillTreeController controller = _skillTreeController;
            if (dialogView == null || controller == null || _cts == null)
            {
                return;
            }

            dialogView.SetInteractionEnabled(false);
            bool isSucceeded;
            try
            {
                isSucceeded = await controller.ResetSkillTreeAsync(_cts.Token);
            }
            finally
            {
                if (_isInitialized && ReferenceEquals(dialogView, _skillTreeResetDialogView))
                {
                    dialogView.SetInteractionEnabled(true);
                }
            }

            if (!_isInitialized || !isSucceeded || !ReferenceEquals(dialogView, _skillTreeResetDialogView))
            {
                return;
            }

            dialogView.Hide();
            _skillDetailScreenView?.HideImmediately();
        }

        /// <summary>
        ///     スキルツリーリセットをキャンセルする。
        /// </summary>
        private void HandleSkillTreeResetCancelled()
        {
            _skillTreeResetDialogView.Hide();
        }

        /// <summary>
        ///     プレビュー動画再生ボタン押下時の処理です。
        /// </summary>
        private void HandlePreviewButtonClicked()
        {
            _skillTreeController.OnPreviewButtonClicked();
        }

        /// <summary>
        ///     プレビュー動画画面を閉じるボタン押下時の処理です。
        /// </summary>
        private void HandlePreviewClosed()
        {
            _previewVideoScreenView.Hide();
        }
    }
}
