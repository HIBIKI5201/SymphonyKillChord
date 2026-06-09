using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.OutGame.SkillTree;
using KillChord.Runtime.Utility.OutGame;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.SkillTree;
using SymphonyFrameWork.System.ServiceLocate;
using System;
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
    public class SkillTreeInitializer : MonoBehaviour
    {

        [SerializeField]
        private UIDocument _uiDocument;
        [SerializeField]
        private SkillNodeDataRepo _skillNodeDataRepo;
        [SerializeField]
        private SkillNodeBindRepo _skillNodeBindRepo;
        [SerializeField]
        private SkillNodePhaseBindDataRepo _skillNodePhaseBindRepo;
        [SerializeField]
        private VideoPlayer _videoPlayer;
        [Space]
        [Header("デバッグ用")]
        [SerializeField]
        private SkillTreeTestInputData _inputData;

        private VisualElement _rootElement;
        private VisualElement _skillDetailRoot;
        private VisualElement _playerStatusRoot;
        private VisualElement _previewVideoContainerRoot;
        private VisualElement _previewVideoRoot;

        private SkillDetailScreenView _skillDetailScreenView;
        private PlayerStatusScreenView _playerStatusScreenView;
        private PreviewVideoScreenView _previewVideoScreenView;

        private SkillTreeController _skillTreeController;
        private SkillDetailPresenter _skillDetailPresenter;
        private PlayerStatusPresenter _playerStatusPresenter;

        private OutGameUIEvent _outGameUIEvent;
        private CancellationTokenSource _cts;
        private RenderTexture _renderTexture;

        private Dictionary<int, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private Dictionary<string, ISkillNodeConnViewModel> _skillNodeConnViews;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, VideoClip> _skillPreviewVideos;

        private const string E_NAME_SKILL_DETAIL = "SkillDetail";
        private const string E_NAME_PLAYER_STATUS = "PlayerStatus";
        private const string E_NAME_PREVIEW_VIDEO_CONTAINER = "PreviewVideoContainer";
        private const string E_NAME_PREVIEW_VIDEO = "PreviewVideo";

        /// <summary>
        ///     初期化処理。
        /// </summary>
        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        ///     イベントを登録する。
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        ///     イベント登録を解除する。
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
            DisposeComponents();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void Initialize()
        {
            _rootElement = _uiDocument.rootVisualElement;
            _outGameUIEvent = ServiceLocator.GetInstance<OutGameUIEvent>();
            _skillDetailRoot = _rootElement.Q<VisualElement>(E_NAME_SKILL_DETAIL);
            _playerStatusRoot = _rootElement.Q<VisualElement>(E_NAME_PLAYER_STATUS);
            _previewVideoContainerRoot = _rootElement.Q<VisualElement>(E_NAME_PREVIEW_VIDEO_CONTAINER);
            _previewVideoRoot = _previewVideoContainerRoot.Q<VisualElement>(name: E_NAME_PREVIEW_VIDEO);

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

            // TODO 今後、所持ポイント、解放済みスキルなどはセーブデータから取得するようにする
            SkillTreeStatusEntity skillTreeEntity = new SkillTreeStatusEntity(_inputData.currentPoints, _inputData.UnlockedSkillIds);

            SkillTreeService skillTreeService = new SkillTreeService(_skillNodeEntities);

            _skillDetailPresenter = new SkillDetailPresenter(_skillDetailScreenView);
            _playerStatusPresenter = new PlayerStatusPresenter();
            _skillTreeController = new SkillTreeController(_skillDetailScreenView,
                _skillDetailPresenter, 
                skillTreeService, 
                _playerStatusPresenter,
                _previewVideoScreenView,
                _previewVideoScreenView,
                _skillNodeEntities,
                _skillNodeViews,
                _skillNodeConnBinds,
                _skillNodeConnViews,
                _unlockPhases,
                skillTreeEntity);

            _cts = new CancellationTokenSource();
        }

        /// <summary>
        ///     スキルノードと接続線の紐づきを作成する。
        /// </summary>
        private void BuildConnBinds()
        {
            _skillNodeConnBinds = new();
            foreach (SkillNodeBindData bind in _skillNodeBindRepo.SkillNodeBinds)
            {
                _skillNodeConnBinds.Add(bind.SkillNodeData.NodeId, bind.FromConnNames);
            }
        }

        /// <summary>
        ///     スキルノードのEntityとViewを作成し、ノードのIDとの紐づけを作成する。
        /// </summary>
        private void BuildSkillNodes()
        {
            List<Button> nodes = _rootElement.Query<Button>(className: UssClassNameConstants.USS_CLASS_SKILL_NODE).ToList();
            _skillNodeEntities = new();
            _skillNodeViews = new();

            for (int i = 0; i < nodes.Count; i++)
            {
                string nodeName = nodes[i].name;
                SkillNodeData nodeData = _skillNodeBindRepo.FindByName(nodeName)?.SkillNodeData;
                if (nodeData == null)
                {
                    continue;
                    // TODO スキルツリーの設計が固まったら正式的なエラー処理を行う。
                    //throw new KeyNotFoundException($"VisualElementの要素名に対応するスキルノードデータが見つかりません：{nodeName}");
                }

                SkillNodeEntity nodeEntity = nodeData.ToDomain();
                SkillNodeView nodeView = new SkillNodeView(nodes[i], nodeData.NodeId, _outGameUIEvent);
                SetNodeUnlockState(nodeView, nodeEntity);

                _skillNodeEntities.Add(nodeData.NodeId, nodeEntity);
                _skillNodeViews.Add(nodeData.NodeId, nodeView);
            }

            foreach (SkillNodeEntity entity in _skillNodeEntities.Values)
            {
                SkillNodeData data = _skillNodeDataRepo.FindNodeData(entity.SkillNodeIdVO.Id);
                SkillNodeEntity[] parents = new SkillNodeEntity[data.ParentNodeIds.Length];
                for (int i = 0; i < data.ParentNodeIds.Length; i++)
                {
                    parents[i] = _skillNodeEntities[data.ParentNodeIds[i]];
                }
                entity.SetParent(parents);
            }
        }

        /// <summary>
        ///     ノード接続線のViewを作成する。
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
        ///     スキルノードの初期解放状態を設定する。
        /// </summary>
        /// <param name="view"></param>
        /// <param name="entity"></param>
        private void SetNodeUnlockState(SkillNodeView view, SkillNodeEntity entity)
        {
            if (_inputData.UnlockedSkillIds.Contains(entity.SkillNodeIdVO.Id)) {
                view.SetUnlocked();
                entity.Unlock();
            }
        }

        /// <summary>
        ///     解放段階の初期状態を設定する。
        /// </summary>
        private void InitializePhaseState()
        {
            _unlockPhases = new();
            for (int i = 0; i < _skillNodePhaseBindRepo.PhaseBindData.Length; i++)
            {
                SkillNodePhaseBindData phaseBindData = _skillNodePhaseBindRepo.PhaseBindData[i];
                string phaseName = phaseBindData.PhaseName;
                VisualElement phaseRoot = _rootElement.Q(name: phaseName);
                _unlockPhases.Add(phaseBindData.RequiredSkillNodeId, phaseRoot);
                phaseRoot.visible = false;
            }
            for(int i = 0; i < _inputData.UnlockedSkillIds.Length; i++)
            {
                SetUnlockPhaseState(_inputData.UnlockedSkillIds[i]);
            }
        }

        /// <summary>
        ///     解放段階を設定する。
        /// </summary>
        /// <param name="nodeId"></param>
        private void SetUnlockPhaseState(int nodeId)
        {
            string phaseName;
            if(_skillNodePhaseBindRepo.TryGetUnlockPhaseName(nodeId, out phaseName))
            {
                _rootElement.Q(name: phaseName).visible = true;
            }
        }

        /// <summary>
        ///     スキルノードのIDとプレビュー動画の紐づきを作成する。
        /// </summary>
        private void BuildVideoClipDict()
        {
            _skillPreviewVideos = new();
            foreach (SkillNodeData node in _skillNodeDataRepo.SkillNodes)
            {
                if (node.PreviewVideoClip != null)
                {
                    _skillPreviewVideos.Add(node.NodeId, node.PreviewVideoClip);
                }
            }
        }

        private void Subscribe()
        {
            _outGameUIEvent.OnSkillNodeSelected += HandleSkillNodeSelected;
            _outGameUIEvent.OnSkillDetailClosed += HandleSkillDetailClosed;
            _outGameUIEvent.OnSkillUnlocked += HandleSkillUnlocked;
            _outGameUIEvent.OnSkillPreviewButtonClicked += HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked += HandlePreviewClosed;
        }

        private void Unsubscribe()
        {
            _outGameUIEvent.OnSkillNodeSelected -= HandleSkillNodeSelected;
            _outGameUIEvent.OnSkillDetailClosed -= HandleSkillDetailClosed;
            _outGameUIEvent.OnSkillUnlocked -= HandleSkillUnlocked;
            _outGameUIEvent.OnSkillPreviewButtonClicked -= HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked -= HandlePreviewClosed;
        }

        private void DisposeComponents()
        {
            _previewVideoScreenView.Dispose();
            _skillDetailScreenView.Dispose();
            foreach(int key in _skillNodeViews.Keys)
            {
                ((SkillNodeView)_skillNodeViews[key]).Dispose();
            }
        }

        /// <summary>
        ///     スキルノードを選択した時の処理。
        /// </summary>
        /// <param name="nodeName"></param>
        private void HandleSkillNodeSelected(string nodeName)
        {
            SkillNodeData nodeData = _skillNodeBindRepo.FindByName(nodeName).SkillNodeData;
            _skillTreeController.OnSkillNodeSelected(nodeData.NodeId, _cts.Token);
        }

        /// <summary>
        ///     スキル詳細画面を閉じる時の処理。
        /// </summary>
        /// <param name="nodeId"></param>
        private void HandleSkillDetailClosed(int nodeId)
        {
            _skillTreeController.OnSkillDetailClosed();
            _skillDetailScreenView.HideImmediately();
        }

        /// <summary>
        ///     スキルを解放する時の処理。
        /// </summary>
        private void HandleSkillUnlocked()
        {
            _skillTreeController.OnSkillUnlocked();
        }

        /// <summary>
        ///     プレビュー動画再生ボタンを押下時の処理。
        /// </summary>
        private void HandlePreviewButtonClicked()
        {
            _skillTreeController.OnPreviewButtonClicked(_cts.Token);
        }

        /// <summary>
        ///     プレイビュー動画画面を閉じるボタンを押下時の処理。
        /// </summary>
        private void HandlePreviewClosed()
        {
            _previewVideoScreenView.HideImmediately();
        }
    }
}
