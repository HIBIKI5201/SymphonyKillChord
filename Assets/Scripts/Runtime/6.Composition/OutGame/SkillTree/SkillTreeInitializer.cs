using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.SkillTree;
using KillChord.Runtime.Utility.OutGame;
using KillChord.Runtime.Utility.OutGame.Savedata;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillTree;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        [SerializeField]
        [Tooltip("スキルツリー画面のUIDocumentです。")]
        private UIDocument _uiDocument;

        [SerializeField]
        [Tooltip("スキルノード定義リポジトリの Addressables キーです。")]
        private string _skillNodeDataRepoKey;

        [SerializeField]
        [Tooltip("スキルノード接続定義リポジトリの Addressables キーです。")]
        private string _skillNodeBindRepoKey;

        [SerializeField]
        [Tooltip("スキルツリー段階定義リポジトリの Addressables キーです。")]
        private string _skillNodePhaseBindRepoKey;

        [SerializeField]
        [Tooltip("スキルプレビュー動画を再生する VideoPlayer です。")]
        private VideoPlayer _videoPlayer;

        [Space]
        [Header("デバッグ用")]
        [SerializeField]
        [Tooltip("デバッグ入力データの Addressables キーです。")]
        private string _inputDataKey;

        [SerializeField]
        [Tooltip("デバッグモードを使用するかどうかです。")]
        private bool _isDebugMode = false;

        private VisualElement _rootElement;
        private VisualElement _skillDetailRoot;
        private VisualElement _playerStatusRoot;
        private VisualElement _previewVideoContainerRoot;
        private VisualElement _previewVideoRoot;
        private Label _currentPointsLabel;
        private SkillDetailScreenView _skillDetailScreenView;
        private PlayerStatusScreenView _playerStatusScreenView;
        private PreviewVideoScreenView _previewVideoScreenView;
        private SkillTreeController _skillTreeController;
        private SkillDetailPresenter _skillDetailPresenter;
        private PlayerStatusPresenter _playerStatusPresenter;
        private SkillUnlockData _skillUnlockData;
        private OutGameUIEvent _outGameUIEvent;
        private CancellationTokenSource _cts;
        private RenderTexture _renderTexture;
        private Dictionary<int, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private Dictionary<string, ISkillNodeConnViewModel> _skillNodeConnViews;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, VideoClip> _skillPreviewVideos;
        private SkillNodeDataRepo _loadedSkillNodeDataRepo;
        private SkillNodeBindRepo _loadedSkillNodeBindRepo;
        private SkillNodePhaseBindDataRepo _loadedSkillNodePhaseBindRepo;
        private SkillTreeTestInputData _loadedInputData;
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

            if (_isDebugMode)
            {
                _loadedInputData = await _inputDataKey.LoadAssetAsync<SkillTreeTestInputData>(this, destroyCancellationToken);
            }

            if (_loadedSkillNodeDataRepo == null
                || _loadedSkillNodeBindRepo == null
                || _loadedSkillNodePhaseBindRepo == null)
            {
                return false;
            }

            if (_isDebugMode && _loadedInputData == null)
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] SavedataSystem が取得できませんでした。", this);
                return false;
            }

            SaveData saveData = await savedataSystem.LoadAsync<SaveData>();
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
            _inputDataKey.ReleaseLoadedAsset(this);
            _loadedSkillNodeDataRepo = null;
            _loadedSkillNodeBindRepo = null;
            _loadedSkillNodePhaseBindRepo = null;
            _loadedInputData = null;
            _skillUnlockData = null;
            _outGameUIEvent = null;
            _isInitialized = false;
            _isSubscribed = false;
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

            ApplyDebugInputIfNeeded();

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

            SkillTreeStatusEntity skillTreeEntity = new(
                _skillUnlockData.ResearchPoint,
                _skillUnlockData.UnlockedSkillNodeIds,
                _skillUnlockData.UnlockedSkillIds);
            SkillTreeService skillTreeService = new(_skillNodeEntities);

            _skillDetailPresenter = new SkillDetailPresenter(_skillDetailScreenView);
            _playerStatusPresenter = new PlayerStatusPresenter();
            _skillTreeController = new SkillTreeController(
                _skillDetailScreenView,
                _skillDetailPresenter,
                _currentPointsLabel,
                skillTreeService,
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
        ///     スキルノードと接続線の紐づきを作成します。
        /// </summary>
        private void BuildConnBinds()
        {
            _skillNodeConnBinds = new();
            foreach (SkillNodeBindData bind in _loadedSkillNodeBindRepo.SkillNodeBinds)
            {
                _skillNodeConnBinds.Add(bind.SkillNodeData.NodeId, bind.FromConnNames);
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

            for (int i = 0; i < nodes.Count; i++)
            {
                string nodeName = nodes[i].name;
                SkillNodeData nodeData = _loadedSkillNodeBindRepo.FindByName(nodeName)?.SkillNodeData;
                if (nodeData == null)
                {
                    throw new KeyNotFoundException($"SkillNodeBindDataが見つかりません：{nodeName}");
                }

                SkillNodeEntity nodeEntity = nodeData.ToDomain();
                SkillNodeView nodeView = new SkillNodeView(nodes[i], nodeData.NodeId, _outGameUIEvent);
                SetNodeUnlockState(nodeView, nodeEntity);

                _skillNodeEntities.Add(nodeData.NodeId, nodeEntity);
                _skillNodeViews.Add(nodeData.NodeId, nodeView);
            }

            foreach (SkillNodeEntity entity in _skillNodeEntities.Values)
            {
                SkillNodeData data = _loadedSkillNodeDataRepo.FindNodeData(entity.SkillNodeIdVO.Id);
                SkillNodeEntity[] parents = new SkillNodeEntity[data.ParentNodeIds.Length];
                for (int i = 0; i < data.ParentNodeIds.Length; i++)
                {
                    if (!_skillNodeEntities.TryGetValue(data.ParentNodeIds[i], out SkillNodeEntity parent))
                    {
                        throw new KeyNotFoundException(
                            $"親ノードID {data.ParentNodeIds[i]} が UI/Bind 構築結果に存在しません。子ノードID: {entity.SkillNodeIdVO.Id}");
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
                _unlockPhases.Add(phaseBindData.RequiredSkillNodeId, phaseRoot);
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
            if (_loadedSkillNodePhaseBindRepo.TryGetUnlockPhaseName(nodeId, out string phaseName))
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
                    _skillPreviewVideos.Add(node.NodeId, node.PreviewVideoClip);
                }
            }
        }

        /// <summary>
        ///     デバッグ入力が有効な場合にスキル解放状態を上書きします。
        /// </summary>
        private void ApplyDebugInputIfNeeded()
        {
            if (!_isDebugMode || _loadedInputData == null)
            {
                return;
            }

            _skillUnlockData.SetResearchPoint(
                _skillUnlockData.ResearchPoint == 0
                    ? _loadedInputData.currentPoints
                    : _skillUnlockData.ResearchPoint);
            _skillUnlockData.SetUnlockedSkillNodeIds(
                _loadedInputData.UnlockedSkillNodeIds.Length == 0
                    ? _skillUnlockData.UnlockedSkillNodeIds
                    : _loadedInputData.UnlockedSkillNodeIds);
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
            _outGameUIEvent.OnSkillPreviewButtonClicked += HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked += HandlePreviewClosed;
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
            _outGameUIEvent.OnSkillPreviewButtonClicked -= HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked -= HandlePreviewClosed;
            _isSubscribed = false;
        }

        /// <summary>
        ///     生成したコンポーネントを解放します。
        /// </summary>
        private void DisposeComponents()
        {
            _previewVideoScreenView?.Dispose();
            _previewVideoScreenView = null;
            _skillDetailScreenView?.Dispose();
            _skillDetailScreenView = null;
            _playerStatusScreenView = null;
            _skillTreeController = null;
            _skillDetailPresenter = null;
            _playerStatusPresenter = null;

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
            SkillNodeData nodeData = _loadedSkillNodeBindRepo.FindByName(nodeName).SkillNodeData;
            _skillTreeController.OnSkillNodeSelected(nodeData.NodeId, _cts.Token);
        }

        /// <summary>
        ///     スキル詳細画面を閉じる時の処理です。
        /// </summary>
        /// <param name="nodeId"> 対象ノードIDです。 </param>
        private void HandleSkillDetailClosed(int nodeId)
        {
            _skillTreeController.OnSkillDetailClosed();
            _skillDetailScreenView.HideImmediately();
        }

        /// <summary>
        ///     スキルを解放する時の処理です。
        /// </summary>
        private void HandleSkillUnlocked()
        {
            _skillTreeController.OnSkillUnlocked();
        }

        /// <summary>
        ///     プレビュー動画再生ボタン押下時の処理です。
        /// </summary>
        private void HandlePreviewButtonClicked()
        {
            _skillTreeController.OnPreviewButtonClicked(_cts.Token);
        }

        /// <summary>
        ///     プレビュー動画画面を閉じるボタン押下時の処理です。
        /// </summary>
        private void HandlePreviewClosed()
        {
            _previewVideoScreenView.HideImmediately();
        }
    }
}
