using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.OutGame.Skill;
using KillChord.Runtime.InfraStructure.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.Utility.OutGame;
using KillChord.Runtime.View.InGame.Skill;
using KillChord.Runtime.View.OutGame.Navigation;
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
        private const string E_NAME_UNLOCK_CONFIRM_BOX = "UnlockConfirmBox";
        private const string E_NAME_PLAYER_STATUS = "PlayerStatus";
        private const string E_NAME_PREVIEW_VIDEO_CONTAINER = "PreviewVideoContainer";
        private const string E_NAME_PREVIEW_VIDEO = "PreviewVideo";
        private const string E_NAME_CURRENT_POINTS_LABEL = "Points";
        private const string E_NAME_TOP_BAR_BACKGROUND = "TopBarBackground";
        private const string E_NAME_BACK_BUTTON = "BackButton";
        private const string E_NAME_SETTING_SHORTCUT_BUTTON = "SettingShortcutButton";
        private const string E_NAME_TITLE = "Title";
        private const float DEFAULT_CRITICAL_DAMAGE_MULTIPLIER = 1f;
        private const float DEFAULT_AREA_ATTACK_RANGE = 1f;

        /// <summary> 連続解放の全体表示演出において、最後のノード演出(ポップ・接続線・不透明度)の再生時間の目安(ミリ秒)。 </summary>
        private const long UNLOCK_LAST_NODE_ANIMATION_MILLISECONDS = 350L;

        /// <summary> 連続解放の全体表示演出を、最後のノード演出終了後も見せ続ける時間(ミリ秒)。 </summary>
        private const long UNLOCK_OVERVIEW_DWELL_MILLISECONDS = 500L;

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

        [SerializeField, SourceDataAddress]
        [Tooltip("ステータスボーナス効果アイコンカタログの Addressables キーです。読み込みに失敗してもアイコンなしで続行します。")]
        private string _statusBonusEffectIconCatalogKey;

        [SerializeField, SourceDataAddress]
        [Tooltip("ノードが解放するスキルの名前解決に使う SkillRepository の Addressables キーです。読み込みに失敗しても名前なしで続行します。")]
        private string _skillRepositoryKey = "OutGameSkillRepository";

        [SerializeField, SourceDataAddress]
        [Tooltip("スキルジャンルアイコンカタログの Addressables キーです。読み込みに失敗してもアイコンなしで続行します。")]
        private string _skillGenreIconCatalogKey;

        [SerializeField, SourceDataAddress]
        [Tooltip("発動コマンドの拍子ごとの色設定(SkillInputProgressUIConfig)の Addressables キーです。読み込みに失敗しても既定色で続行します。")]
        private string _skillInputProgressUIConfigKey;

        [SerializeField]
        [Tooltip("スキルプレビュー動画を再生する VideoPlayer です。")]
        private VideoPlayer _videoPlayer;

        [SerializeField]
        [Tooltip("発動コマンド表示に使う正六角形スプライト（Assets/Arts/UI/UI_hexagon.png）です。")]
        private Sprite _comboHexIcon;

        private VisualElement _rootElement;
        private VisualElement _skillDetailRoot;
        private VisualElement _unlockConfirmBoxRoot;
        private bool _isUnlockConfirmOpen;
        private bool _skipUnlockConfirmation;
        private IVisualElementScheduledItem _unlockCameraRestoreItem;
        private VisualElement _topBarBackgroundRoot;
        private VisualElement _backButtonRoot;
        private VisualElement _settingShortcutButtonRoot;
        private VisualElement _titleRoot;
        private VisualElement _playerStatusRoot;
        private VisualElement _previewVideoContainerRoot;
        private VisualElement _previewVideoRoot;
        private Label _currentPointsLabel;
        private SkillDetailScreenView _skillDetailScreenView;
        private PlayerStatusScreenView _playerStatusScreenView;
        private PreviewVideoScreenView _previewVideoScreenView;
        private SkillTreeResetDialogView _skillTreeResetDialogView;
        private UnlockConfirmDialogView _unlockConfirmDialogView;
        private readonly ModalNavigationScope _dialogNavigationScope = new();
        private readonly ModalNavigationScope _skillDetailNavigationScope = new();
        private SkillTreeViewportView _skillTreeViewportView;
        private SkillTreeController _skillTreeController;
        private SkillTreeService _skillTreeService;
        private SkillDetailPresenter _skillDetailPresenter;
        private PlayerStatusPresenter _playerStatusPresenter;
        private SkillTreeFocusPresenter _skillTreeFocusPresenter;
        private SkillUnlockData _skillUnlockData;
        private int _researchPoint;
        private OutGameUIEvent _outGameUIEvent;
        private CancellationTokenSource _cts;
        private RenderTexture _renderTexture;
        private Dictionary<SkillNodeId, SkillNodeEntity> _skillNodeEntities;
        private Dictionary<int, ISkillNodeViewModel> _skillNodeViews;
        private Dictionary<int, VisualElement> _skillNodeElements;
        private List<VisualElement> _skillNodeElementList;
        private List<VisualElement> _skillTreeNavigationCandidates;
        private Dictionary<VisualElement, List<VisualElement>> _skillNodeAdjacency;
        private Dictionary<string, ISkillNodeConnViewModel> _skillNodeConnViews;
        private Dictionary<int, string[]> _skillNodeConnBinds;
        private Dictionary<int, VisualElement> _unlockPhases;
        private Dictionary<int, VideoClip> _skillPreviewVideos;
        private Dictionary<StatusBonusEffectKind, Sprite> _statusBonusEffectIcons;
        private Dictionary<SkillType, Sprite> _skillGenreIcons;
        private SkillNodeDataRepo _loadedSkillNodeDataRepo;
        private SkillNodeBindRepo _loadedSkillNodeBindRepo;
        private SkillNodePhaseBindDataRepo _loadedSkillNodePhaseBindRepo;
        private StatusBonusEffectIconCatalogAsset _loadedStatusBonusEffectIconCatalog;
        private SkillRepository _loadedSkillRepository;
        private SkillGenreIconCatalogAsset _loadedSkillGenreIconCatalog;
        private SkillInputProgressUIConfig _loadedSkillInputProgressUIConfig;
        private Dictionary<int, Color> _skillBeatColors;
        private bool _isInitialized;
        private bool _isSubscribed;
        private bool _isSkillDetailOpen;

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

            try
            {
                _loadedStatusBonusEffectIconCatalog =
                    await _statusBonusEffectIconCatalogKey.LoadAssetAsync<StatusBonusEffectIconCatalogAsset>(this, destroyCancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                // アイコンは付加情報のため、読み込みに失敗してもスキルツリー画面自体の初期化は継続する。
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] ステータスボーナス効果アイコンカタログの読み込みに失敗しました。アイコンなしで続行します。{ex.Message}",
                    this);
                _loadedStatusBonusEffectIconCatalog = null;
            }

            try
            {
                _loadedSkillRepository =
                    await _skillRepositoryKey.LoadAssetAsync<SkillRepository>(this, destroyCancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                // スキル名は付加情報のため、読み込みに失敗してもスキルツリー画面自体の初期化は継続する。
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] SkillRepositoryの読み込みに失敗しました。スキル名なしで続行します。{ex.Message}",
                    this);
                _loadedSkillRepository = null;
            }

            try
            {
                _loadedSkillGenreIconCatalog =
                    await _skillGenreIconCatalogKey.LoadAssetAsync<SkillGenreIconCatalogAsset>(this, destroyCancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                // アイコンは付加情報のため、読み込みに失敗してもスキルツリー画面自体の初期化は継続する。
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] SkillGenreIconCatalogの読み込みに失敗しました。アイコンなしで続行します。{ex.Message}",
                    this);
                _loadedSkillGenreIconCatalog = null;
            }

            try
            {
                _loadedSkillInputProgressUIConfig =
                    await _skillInputProgressUIConfigKey.LoadAssetAsync<SkillInputProgressUIConfig>(this, destroyCancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                // 発動コマンドの色は付加情報のため、読み込みに失敗してもスキルツリー画面自体の初期化は継続する。
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] SkillInputProgressUIConfigの読み込みに失敗しました。既定色で続行します。{ex.Message}",
                    this);
                _loadedSkillInputProgressUIConfig = null;
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
            _researchPoint = saveData.ResourceInventory.GetAmount(GameResourceIds.ResearchPoint);
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
            if (_rootElement != null)
            {
                _rootElement.UnregisterCallback<PointerDownEvent>(HandleRootPointerDown, TrickleDown.TrickleDown);
                _rootElement.UnregisterCallback<NavigationCancelEvent>(HandleRootNavigationCancelHandler, TrickleDown.TrickleDown);
                _rootElement.UnregisterCallback<NavigationMoveEvent>(HandleSkillNodeNavigationMoveHandler);
            }
            _isSkillDetailOpen = false;
            _isUnlockConfirmOpen = false;
            _skipUnlockConfirmation = false;
            DisposeComponents();
            CancelAndDisposeCts();

            _skillNodeDataRepoKey.ReleaseLoadedAsset(this);
            _skillNodeBindRepoKey.ReleaseLoadedAsset(this);
            _skillNodePhaseBindRepoKey.ReleaseLoadedAsset(this);
            _statusBonusEffectIconCatalogKey.ReleaseLoadedAsset(this);
            _skillRepositoryKey.ReleaseLoadedAsset(this);
            _skillGenreIconCatalogKey.ReleaseLoadedAsset(this);
            _skillInputProgressUIConfigKey.ReleaseLoadedAsset(this);
            _loadedSkillNodeDataRepo = null;
            _loadedSkillNodeBindRepo = null;
            _loadedSkillNodePhaseBindRepo = null;
            _loadedStatusBonusEffectIconCatalog = null;
            _loadedSkillRepository = null;
            _loadedSkillGenreIconCatalog = null;
            _loadedSkillInputProgressUIConfig = null;
            _skillBeatColors = null;
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
            _unlockConfirmBoxRoot = _rootElement.Q<VisualElement>(E_NAME_UNLOCK_CONFIRM_BOX);
            _playerStatusRoot = _rootElement.Q<VisualElement>(E_NAME_PLAYER_STATUS);
            _previewVideoContainerRoot = _rootElement.Q<VisualElement>(E_NAME_PREVIEW_VIDEO_CONTAINER);
            _previewVideoRoot = _rootElement.Q<VisualElement>(E_NAME_PREVIEW_VIDEO);
            _currentPointsLabel = _rootElement.Q<Label>(E_NAME_CURRENT_POINTS_LABEL);
            _topBarBackgroundRoot = _rootElement.Q<VisualElement>(E_NAME_TOP_BAR_BACKGROUND);
            _backButtonRoot = _rootElement.Q<VisualElement>(E_NAME_BACK_BUTTON);
            _settingShortcutButtonRoot = _rootElement.Q<VisualElement>(E_NAME_SETTING_SHORTCUT_BUTTON);
            _titleRoot = _rootElement.Q<VisualElement>(E_NAME_TITLE);

            if (_skillDetailRoot == null
                || _unlockConfirmBoxRoot == null
                || _playerStatusRoot == null
                || _previewVideoContainerRoot == null
                || _previewVideoRoot == null
                || _currentPointsLabel == null
                || _topBarBackgroundRoot == null
                || _backButtonRoot == null
                || _settingShortcutButtonRoot == null
                || _titleRoot == null)
            {
                Debug.LogError($"[{nameof(SkillTreeInitializer)}] スキルツリー用のUI要素が不足しています。", this);
                return false;
            }

            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.isLooping = true;
            _renderTexture = _videoPlayer.targetTexture;
            _previewVideoRoot.style.backgroundImage = Background.FromRenderTexture(_renderTexture);

            BuildStatusBonusEffectIconMap();
            BuildSkillGenreIconMap();
            BuildSkillBeatColorMap();
            BuildSkillNodes();
            BuildNodeConns();
            BuildConnBinds();
            InitializePhaseState();
            BuildVideoClipDict();

            // ノード以外にも、トップバーの設定ショートカットボタンなど画面内で
            // コントローラー操作可能な要素を、ノード間移動と同じ実座標ベースの解決に含める。
            // 木構造のノード群とは離れた位置にあり、標準の自動ナビゲーションでは
            // 往復できないことがあるため。
            _skillTreeNavigationCandidates = new List<VisualElement>(_skillNodeElementList)
            {
                _settingShortcutButtonRoot,
            };

            _skillDetailScreenView = new SkillDetailScreenView(_skillDetailRoot, _outGameUIEvent, _comboHexIcon);
            _skillDetailScreenView.HideImmediately();
            _playerStatusScreenView = new PlayerStatusScreenView(
                _playerStatusRoot,
                _outGameUIEvent,
                GetStatusIcon(StatusBonusEffectKind.MaxHealth),
                GetStatusIcon(StatusBonusEffectKind.AttackPower),
                GetStatusIcon(StatusBonusEffectKind.CriticalChance),
                GetStatusIcon(StatusBonusEffectKind.CriticalDamage),
                GetStatusIcon(StatusBonusEffectKind.AreaAttackRange));
            _previewVideoScreenView = new PreviewVideoScreenView(_previewVideoContainerRoot, _outGameUIEvent, _videoPlayer, _skillPreviewVideos);
            _previewVideoScreenView.HideImmediately();
            _skillTreeResetDialogView = new SkillTreeResetDialogView(_rootElement, _outGameUIEvent);
            _unlockConfirmDialogView = new UnlockConfirmDialogView(_rootElement, _outGameUIEvent);
            _skillTreeViewportView = new SkillTreeViewportView(_rootElement, _skillNodeElements);

            SkillTreeStatusEntity skillTreeEntity = new(
                _researchPoint,
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
                GetBaseCriticalDamageMultiplier(),
                GetBaseAreaAttackRange());
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
                () => _outGameUIEvent.OnOwnedSkillChanged?.Invoke(),
                _loadedSkillRepository,
                new SkillDisplayTextFormatter(new SkillEffectDescriptionFormatter()),
                _skillGenreIcons,
                _skillBeatColors);

            _rootElement.RegisterCallback<PointerDownEvent>(HandleRootPointerDown, TrickleDown.TrickleDown);
            _rootElement.RegisterCallback<NavigationCancelEvent>(HandleRootNavigationCancelHandler, TrickleDown.TrickleDown);
            _rootElement.RegisterCallback<NavigationMoveEvent>(HandleSkillNodeNavigationMoveHandler);

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
        ///     プレイヤーの先頭攻撃定義から基礎射程を取得します。
        /// </summary>
        /// <returns> 基礎射程。攻撃定義がない場合は既定値。 </returns>
        private float GetBaseAreaAttackRange()
        {
            AttackDefinitionAsset[] attackDefinitions = _playerData.AttackDefinitionAssets;
            if (attackDefinitions == null)
            {
                return DEFAULT_AREA_ATTACK_RANGE;
            }

            for (int i = 0; i < attackDefinitions.Length; i++)
            {
                if (attackDefinitions[i] != null)
                {
                    return attackDefinitions[i].Range;
                }
            }

            return DEFAULT_AREA_ATTACK_RANGE;
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
                nodeView.SetIcon(GetNodeIcon(nodeEntity));

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

            _skillNodeElementList = new List<VisualElement>(_skillNodeElements.Values);
            BuildSkillNodeAdjacency();
            MarkInitialFocusNode();
        }

        /// <summary>
        ///     各ノードの親子接続関係(<see cref="SkillNodeEntity.Parents"/>)から、
        ///     コントローラー移動先の候補となる隣接ノード要素の一覧を双方向で構築する。
        ///     要素自身をキーにすることで、移動判定時にノードIDへ逆引きする必要をなくす。
        /// </summary>
        private void BuildSkillNodeAdjacency()
        {
            _skillNodeAdjacency = new Dictionary<VisualElement, List<VisualElement>>();
            foreach (VisualElement element in _skillNodeElements.Values)
            {
                _skillNodeAdjacency[element] = new List<VisualElement>();
            }

            foreach (SkillNodeEntity entity in _skillNodeEntities.Values)
            {
                VisualElement nodeElement = _skillNodeElements[entity.SkillNodeIdVO.Id];
                if (entity.Parents == null)
                {
                    continue;
                }

                for (int i = 0; i < entity.Parents.Length; i++)
                {
                    VisualElement parentElement = _skillNodeElements[entity.Parents[i].SkillNodeIdVO.Id];
                    _skillNodeAdjacency[nodeElement].Add(parentElement);
                    _skillNodeAdjacency[parentElement].Add(nodeElement);
                }
            }
        }

        /// <summary>
        ///     親を持たない起点ノードへ、初期フォーカス用のUSSクラスを付与します。
        /// </summary>
        private void MarkInitialFocusNode()
        {
            SkillNodeEntity rootEntity = null;
            int rootCount = 0;

            foreach (SkillNodeEntity entity in _skillNodeEntities.Values)
            {
                if (entity.Parents != null && entity.Parents.Length > 0)
                {
                    continue;
                }

                rootCount++;

                // 起点が複数ある場合でも結果が実行ごとに変わらないよう、
                // ノードIDが最小のものを採用する。Dictionaryの列挙順は保証されないため。
                if (rootEntity == null || entity.SkillNodeIdVO.Id < rootEntity.SkillNodeIdVO.Id)
                {
                    rootEntity = entity;
                }
            }

            if (rootEntity == null)
            {
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] 親を持たない起点ノードが見つかりません。"
                    + " 初期フォーカスは戻るボタンへフォールバックします。");
                return;
            }

            if (rootCount > 1)
            {
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] 親を持たない起点ノードが{rootCount}件あります。"
                    + $" ノードID {rootEntity.SkillNodeIdVO.Id} を初期フォーカス先に採用しました。");
            }

            // 辞書はViewModelインターフェースで保持しているため、要素を持つ実装型へ絞り込む。
            if (!_skillNodeViews.TryGetValue(rootEntity.SkillNodeIdVO.Id, out ISkillNodeViewModel viewModel)
                || viewModel is not SkillNodeView nodeView)
            {
                Debug.LogWarning(
                    $"[{nameof(SkillTreeInitializer)}] 起点ノードのViewが取得できませんでした。"
                    + $" ノードID: {rootEntity.SkillNodeIdVO.Id}");
                return;
            }

            nodeView.RootElement.AddToClassList(UINavigationExtensions.INITIAL_FOCUS_CLASS_NAME);
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
        ///     ステータスボーナス効果種別とアイコンの紐づきを作成します。
        /// </summary>
        private void BuildStatusBonusEffectIconMap()
        {
            _statusBonusEffectIcons = new();
            if (_loadedStatusBonusEffectIconCatalog == null)
            {
                return;
            }

            foreach (StatusBonusEffectIconCatalogEntry entry in _loadedStatusBonusEffectIconCatalog.Entries)
            {
                if (entry.Icon != null)
                {
                    _statusBonusEffectIcons[entry.Kind] = entry.Icon;
                }
            }
        }

        /// <summary>
        ///     ノードに表示するアイコンを取得します。スキルを解放するノードは対応スキルのアイコンを優先し、
        ///     それ以外はステータスボーナス効果のアイコンにフォールバックします。
        /// </summary>
        /// <param name="nodeEntity"> 対象のノードEntity。 </param>
        /// <returns> 対応するアイコン。取得できない場合はnull。 </returns>
        private Sprite GetNodeIcon(SkillNodeEntity nodeEntity)
        {
            Sprite skillIcon = GetUnlockSkillIcon(nodeEntity);
            if (skillIcon != null)
            {
                return skillIcon;
            }

            if (nodeEntity.StatusBonusEffects.Count == 0 || _statusBonusEffectIcons == null)
            {
                return null;
            }

            StatusBonusEffectKind kind = nodeEntity.StatusBonusEffects[0].Kind;
            return _statusBonusEffectIcons.TryGetValue(kind, out Sprite icon) ? icon : null;
        }

        /// <summary>
        ///     ノードが解放するスキルに設定されたアイコンを取得します。
        /// </summary>
        /// <param name="nodeEntity"> 対象のノードEntity。 </param>
        /// <returns> 最初に解決できたスキルのアイコン。解決できない場合はnull。 </returns>
        private Sprite GetUnlockSkillIcon(SkillNodeEntity nodeEntity)
        {
            if (_loadedSkillRepository == null || nodeEntity.UnlockSkillIds.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < nodeEntity.UnlockSkillIds.Length; i++)
            {
                if (_loadedSkillRepository.TryGetSkill(nodeEntity.UnlockSkillIds[i], out SkillTemplate skillData)
                    && skillData.Icon != null)
                {
                    return skillData.Icon;
                }
            }

            return null;
        }

        /// <summary>
        ///     ステータス種別に対応するアイコンを取得します。
        /// </summary>
        /// <param name="kind"> 対象のステータス種別。 </param>
        /// <returns> 対応するアイコン。見つからない場合はnull。 </returns>
        private Sprite GetStatusIcon(StatusBonusEffectKind kind)
        {
            if (_statusBonusEffectIcons == null)
            {
                return null;
            }

            return _statusBonusEffectIcons.TryGetValue(kind, out Sprite icon) ? icon : null;
        }

        /// <summary>
        ///     スキルジャンルとアイコンの紐づきを作成します。
        /// </summary>
        private void BuildSkillGenreIconMap()
        {
            _skillGenreIcons = new();
            if (_loadedSkillGenreIconCatalog == null)
            {
                return;
            }

            foreach (SkillGenreIconCatalogEntry entry in _loadedSkillGenreIconCatalog.Entries)
            {
                if (entry.Icon != null)
                {
                    _skillGenreIcons[entry.Genre] = entry.Icon;
                }
            }
        }

        /// <summary>
        ///     発動コマンドの拍子(BeatType)と色の対応表を構築します。
        ///     実際のインゲーム入力進行UIと同じ色設定(SkillInputProgressUIConfig)を流用します。
        /// </summary>
        private void BuildSkillBeatColorMap()
        {
            _skillBeatColors = new Dictionary<int, Color>();
            if (_loadedSkillInputProgressUIConfig == null)
            {
                return;
            }

            SkillInputProgressViewSetting viewSetting = _loadedSkillInputProgressUIConfig.Create();
            foreach (BeatType beatType in System.Enum.GetValues(typeof(BeatType)))
            {
                try
                {
                    SkillBeatVisualSetting setting = viewSetting.GetSetting((int)beatType);
                    _skillBeatColors[(int)beatType] = setting.NormalColor;
                }
                catch (System.InvalidOperationException)
                {
                    // 該当する拍子の設定が存在しない場合はスキップする。
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
            _outGameUIEvent.OnSkillUnlockConfirmationRequested += HandleSkillUnlockConfirmationRequested;
            _outGameUIEvent.OnSkillUnlockConfirmed += HandleSkillUnlockConfirmed;
            _outGameUIEvent.OnSkillTreeResetRequested += HandleSkillTreeResetRequested;
            _outGameUIEvent.OnSkillTreeResetConfirmed += HandleSkillTreeResetConfirmed;
            _outGameUIEvent.OnSkillTreeResetCancelled += HandleSkillTreeResetCancelled;
            _outGameUIEvent.OnSkillPreviewButtonClicked += HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked += HandlePreviewClosed;
            _outGameUIEvent.OnShownSkillTreeScreen += HandleSkillTreeScreenShownHandler;
            _outGameUIEvent.OnScreenClosed += HandleScreenClosedHandler;
            _skillTreeViewportView.OnFocusTargetsRequested += HandleFocusTargetsRequestedHandler;
            _unlockConfirmDialogView.OnCancelled += HandleUnlockConfirmDialogCancelledHandler;
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
            _outGameUIEvent.OnSkillUnlockConfirmationRequested -= HandleSkillUnlockConfirmationRequested;
            _outGameUIEvent.OnSkillUnlockConfirmed -= HandleSkillUnlockConfirmed;
            _outGameUIEvent.OnSkillTreeResetRequested -= HandleSkillTreeResetRequested;
            _outGameUIEvent.OnSkillTreeResetConfirmed -= HandleSkillTreeResetConfirmed;
            _outGameUIEvent.OnSkillTreeResetCancelled -= HandleSkillTreeResetCancelled;
            _outGameUIEvent.OnSkillPreviewButtonClicked -= HandlePreviewButtonClicked;
            _outGameUIEvent.OnSkillPreviewCloseButtonClicked -= HandlePreviewClosed;
            _outGameUIEvent.OnShownSkillTreeScreen -= HandleSkillTreeScreenShownHandler;
            _outGameUIEvent.OnScreenClosed -= HandleScreenClosedHandler;
            _skillTreeViewportView.OnFocusTargetsRequested -= HandleFocusTargetsRequestedHandler;
            _unlockConfirmDialogView.OnCancelled -= HandleUnlockConfirmDialogCancelledHandler;
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
            _unlockConfirmDialogView?.Dispose();
            _unlockConfirmDialogView = null;
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
            _skillNodeElementList = null;
            _skillTreeNavigationCandidates = null;
            _skillNodeAdjacency = null;
            _skillNodeConnViews = null;
            _skillNodeConnBinds = null;
            _unlockPhases = null;
            _skillPreviewVideos = null;
            _statusBonusEffectIcons = null;
            _skillGenreIcons = null;
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

            // マウスクリック等でパネルを閉じずに別ノードへ直接切り替わることがあり、
            // その場合前回のノード用に閉じ込めたフォーカスが残ったままになる
            // (Activate() は既に有効な間は何もしないため)。ノード選択のたびに
            // 一旦解除し、新しいノードの状態に応じて改めて判定し直す。
            _skillDetailNavigationScope.Deactivate();

            _skillTreeController.OnSkillNodeSelected(nodeId.Id);
            _isSkillDetailOpen = true;
            _skillTreeResetDialogView.SetResetButtonVisible(false);
            _skillTreeViewportView.FocusOnNode(nodeId.Id);

            // 解放操作ができるノードの場合のみ、パネル側へフォーカスを移して閉じ込める。
            // 解放済み等でパネル内に操作できる要素が無い場合にまで閉じ込めると、
            // コントローラーではフォーカスの行き場が無くなり操作不能になるため、
            // その場合はノード側にフォーカスを残したままにする。
            if (_skillDetailScreenView.IsUnlockAvailable)
            {
                _skillDetailNavigationScope.Activate(_skillDetailRoot);
            }
        }

        /// <summary>
        ///     スキル詳細画面を閉じる時の処理です。
        /// </summary>
        /// <param name="nodeId"> 対象ノードIDです。 </param>
        private void HandleSkillDetailClosed(int nodeId)
        {
            _isSkillDetailOpen = false;
            _skillTreeController.OnSkillDetailClosed();
            _skillDetailScreenView.Hide();
            _skillTreeResetDialogView.SetResetButtonVisible(true);
            _skillTreeViewportView.ClearFocusZoom();
            _skillDetailNavigationScope.Deactivate();
        }

        /// <summary>
        ///     選択中のノードと、その詳細を表示しているウィンドウ以外がクリックされたときに選択を解除する。
        /// </summary>
        /// <param name="evt"> ポインタ押下イベント。 </param>
        private void HandleRootPointerDown(PointerDownEvent evt)
        {
            if (TrySkipUnlockPerformance())
            {
                evt.StopPropagation();
                return;
            }

            if (evt.target is not VisualElement target) { return; }

            if (_isUnlockConfirmOpen)
            {
                if (!IsSameOrDescendant(_unlockConfirmBoxRoot, target))
                {
                    _unlockConfirmDialogView.Hide();
                    _isUnlockConfirmOpen = false;
                    _dialogNavigationScope.Deactivate();
                }

                return;
            }

            if (!_isSkillDetailOpen) { return; }

            if (_skillDetailRoot != null && _skillDetailRoot.Contains(target)) { return; }
            if (IsSkillNodeElement(target)) { return; }

            _outGameUIEvent.OnSkillDetailClosed?.Invoke(0);
        }

        /// <summary>
        ///     解放済みノードなどパネルへフォーカスを移していない状態でのキャンセル操作を処理する。
        ///     <para>
        ///         この場合フォーカスはスキルノード側に残ったままのため、何もしなければ
        ///         キャンセル操作がそのままスキルツリー画面自体のキャンセル(ホームへ戻る)まで
        ///         バブリングしてしまう。画面側の処理より先に処理するため、
        ///         バブリングではなくトリクルダウンで購読する。
        ///     </para>
        /// </summary>
        /// <param name="evt"> ナビゲーションキャンセルイベント。 </param>
        private void HandleRootNavigationCancelHandler(NavigationCancelEvent evt)
        {
            // 解放確認ダイアログを表示中は、そちら自身のキャンセル処理に任せる。
            if (_isUnlockConfirmOpen || !_isSkillDetailOpen)
            {
                return;
            }

            _outGameUIEvent.OnSkillDetailClosed?.Invoke(0);
            evt.StopPropagation();
        }

        /// <summary>
        ///     スキルノードおよびトップバーの設定ショートカットボタン間のコントローラー移動先を解決する。
        ///     <para>
        ///         スキルノードが起点の場合は、実際に接続されているノード(親子関係)だけを候補にし、
        ///         そのうちどれが押した方向に一致するかを実座標で判定する。木構造上つながっていない
        ///         ノードへ移動してしまうことがないようにするため。設定ボタンへは、木構造の候補に
        ///         含めて上方向で到達できるようにする。
        ///     </para>
        ///     <para>
        ///         設定ボタンが起点の場合は接続関係を持たないため、画面内の全ノードから
        ///         実座標で一番近いものへ戻る(木構造への再進入)。
        ///     </para>
        /// </summary>
        /// <param name="evt"> ナビゲーション移動イベント。 </param>
        private void HandleSkillNodeNavigationMoveHandler(NavigationMoveEvent evt)
        {
            if (_skillTreeNavigationCandidates == null
                || evt.target is not VisualElement target
                || !IsSkillTreeSpatialNavigationSource(target))
            {
                NavigationDebugLog.Log(
                    $"[SkillTreeNav] skip target={NavigationDebugLog.Describe(evt.target as VisualElement)} "
                    + $"candidates={_skillTreeNavigationCandidates?.Count}");
                return;
            }

            IReadOnlyList<VisualElement> candidates = ResolveSkillTreeNavigationCandidates(
                target, out bool isGraphBased);
            // 接続グラフに基づく候補では、実際につながっているノードへは画面外でも
            // 移動できるようにするため、画面内かどうかの絞り込みは行わない。
            // その代わり移動後にEnsureVisibleで画面をノードへ追従させる。
            Rect? viewportFilter = isGraphBased ? null : _skillTreeViewportView?.ViewportWorldBound;
            VisualElement next = SpatialNavigationResolver.FindNearestInDirection(
                target, candidates, evt.direction, viewportFilter);
            NavigationDebugLog.Log(
                $"[SkillTreeNav] from={NavigationDebugLog.Describe(target)} dir={evt.direction} "
                + $"candidates={candidates.Count} graphBased={isGraphBased} -> {NavigationDebugLog.Describe(next)}");

            if (next == null)
            {
                return;
            }

            next.Focus();
            evt.StopPropagation();
            target.panel?.focusController?.IgnoreEvent(evt);

            if (isGraphBased)
            {
                _skillTreeViewportView?.EnsureVisible(next);
            }
        }

        /// <summary>
        ///     移動先解決に使う候補一覧を選ぶ。スキルノードが起点なら実際に接続されている
        ///     隣接ノード+設定ボタンのみに絞り、設定ボタンが起点なら木構造への再進入のため
        ///     全ノードを候補にする。
        /// </summary>
        /// <param name="source"> 移動元の要素。 </param>
        /// <param name="isGraphBased"> 接続グラフに基づく候補を返した場合はtrue。 </param>
        /// <returns> 移動先候補の一覧。 </returns>
        private IReadOnlyList<VisualElement> ResolveSkillTreeNavigationCandidates(
            VisualElement source, out bool isGraphBased)
        {
            if (_skillNodeAdjacency != null
                && _skillNodeAdjacency.TryGetValue(source, out List<VisualElement> adjacentNodes))
            {
                isGraphBased = true;
                List<VisualElement> candidates = new List<VisualElement>(adjacentNodes.Count + 1);
                candidates.AddRange(adjacentNodes);
                if (_settingShortcutButtonRoot != null)
                {
                    candidates.Add(_settingShortcutButtonRoot);
                }

                return candidates;
            }

            isGraphBased = false;
            return _skillTreeNavigationCandidates;
        }

        /// <summary>
        ///     指定要素が、実座標ベースの移動解決の起点として扱う対象(スキルノードまたは
        ///     設定ショートカットボタン)かどうかを判定する。
        /// </summary>
        /// <param name="element"> 判定対象の要素。 </param>
        /// <returns> 起点として扱う場合はtrue。 </returns>
        private bool IsSkillTreeSpatialNavigationSource(VisualElement element)
        {
            return element.ClassListContains(UssClassNameConstants.USS_CLASS_SKILL_NODE)
                || ReferenceEquals(element, _settingShortcutButtonRoot);
        }

        /// <summary>
        ///     指定要素が祖先要素自身、またはその子孫かどうかを判定する。
        /// </summary>
        /// <param name="ancestor"> 判定の基準となる要素。 </param>
        /// <param name="target"> 判定対象の要素。 </param>
        /// <returns> targetがancestor自身またはその子孫であればtrue。 </returns>
        private static bool IsSameOrDescendant(VisualElement ancestor, VisualElement target)
        {
            return ancestor != null && (target == ancestor || ancestor.Contains(target));
        }

        /// <summary>
        ///     指定要素がスキルノード要素(またはその子孫)かどうかを判定する。
        /// </summary>
        /// <param name="element"> 判定対象の要素。 </param>
        /// <returns> スキルノード要素の内側であればtrue。 </returns>
        private static bool IsSkillNodeElement(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.ClassListContains(UssClassNameConstants.USS_CLASS_SKILL_NODE)) { return true; }
            }

            return false;
        }

        /// <summary>
        ///     スキルを解放する時の処理です。
        /// </summary>
        private void HandleSkillUnlocked()
        {
            PlayUnlockCameraWorkIfNeeded();
            _skillTreeController.OnSkillUnlocked();
        }

        /// <summary>
        ///     スキル解放の確認ダイアログを表示する時の処理です。
        /// </summary>
        private void HandleSkillUnlockConfirmationRequested()
        {
            if (_skipUnlockConfirmation)
            {
                PlayUnlockCameraWorkIfNeeded();
                _skillTreeController.OnSkillUnlocked();
                return;
            }

            _unlockConfirmDialogView.Show(_skillTreeController.GetUnlockConfirmation());
            _isUnlockConfirmOpen = true;
            _dialogNavigationScope.Activate(_unlockConfirmDialogView.DialogRoot);
        }

        /// <summary>
        ///     解放確認ダイアログで解放が確定された時の処理です。
        /// </summary>
        private void HandleSkillUnlockConfirmed()
        {
            if (_unlockConfirmDialogView.IsSkipConfirmationChecked)
            {
                _skipUnlockConfirmation = true;
            }

            PlayUnlockCameraWorkIfNeeded();
            _skillTreeController.OnSkillUnlocked();
            _unlockConfirmDialogView.Hide();
            _isUnlockConfirmOpen = false;
            _dialogNavigationScope.Deactivate();

            // 解放が確定すると解放ボタンが無効化されパネル内に操作できる要素が
            // 無くなるため、情報パネル自体を閉じてスキルツリー側へフォーカスを戻す。
            HandleSkillDetailClosed(0);
        }

        /// <summary>
        ///     コントローラーのキャンセル操作で解放確認ダイアログが閉じられた時の処理です。
        /// </summary>
        private void HandleUnlockConfirmDialogCancelledHandler()
        {
            _isUnlockConfirmOpen = false;
            _dialogNavigationScope.Deactivate();
        }

        /// <summary>
        ///     複数ノードを一度に解放する場合、解放パスの最上ノードと、それへ直接つながる
        ///     解放済みノード(接続元)の最下部が画面の上端・下端に揃うよう一時的にズームし、
        ///     演出終了後に自動で元のズーム(選択ノードへのフォーカス)へ戻します。
        /// </summary>
        private void PlayUnlockCameraWorkIfNeeded()
        {
            IReadOnlyList<int> pendingUnlockNodeIds = _skillTreeController.GetPendingUnlockNodeIds();
            if (pendingUnlockNodeIds.Count <= 1 || _skillTreeViewportView == null)
            {
                return;
            }

            int selectedNodeId = _skillTreeController.SelectedNodeId;
            IReadOnlyList<int> framingNodeIds = _skillTreeController.GetUnlockCameraFramingNodeIds();
            _skillTreeViewportView.FocusOnNodeRange(framingNodeIds);
            SetOtherUiVisibleDuringUnlockCameraWork(false);

            long overviewHoldMilliseconds =
                (pendingUnlockNodeIds.Count - 1) * SkillTreeController.UNLOCK_STAGGER_INTERVAL_MILLISECONDS
                + UNLOCK_LAST_NODE_ANIMATION_MILLISECONDS
                + UNLOCK_OVERVIEW_DWELL_MILLISECONDS;
            _unlockCameraRestoreItem?.Pause();
            _unlockCameraRestoreItem = _rootElement.schedule
                .Execute(() => RestoreUnlockCameraFocus(selectedNodeId))
                .StartingIn(overviewHoldMilliseconds);
        }

        /// <summary>
        ///     連続解放のカメラワークを終了し、選択ノードへのフォーカス(未選択の場合はズーム解除)へ戻します。
        /// </summary>
        /// <param name="selectedNodeId"> 復帰先の選択ノードID。未選択の場合は-1。 </param>
        private void RestoreUnlockCameraFocus(int selectedNodeId)
        {
            _unlockCameraRestoreItem = null;
            SetOtherUiVisibleDuringUnlockCameraWork(true);
            if (selectedNodeId != -1)
            {
                _skillTreeViewportView.FocusOnNode(selectedNodeId);
            }
            else
            {
                _skillTreeViewportView.ClearFocusZoom();
            }
        }

        /// <summary>
        ///     連続解放のカメラワーク中、ツリー以外のUI(トップバー・スキル詳細・
        ///     プレイヤーステータス)を一時的に非表示、または元に戻します。
        /// </summary>
        /// <param name="isVisible"> 表示する場合はtrue。 </param>
        private void SetOtherUiVisibleDuringUnlockCameraWork(bool isVisible)
        {
            DisplayStyle displayStyle = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_topBarBackgroundRoot == null) { return; }

            _topBarBackgroundRoot.style.display = displayStyle;
            _backButtonRoot.style.display = displayStyle;
            _settingShortcutButtonRoot.style.display = displayStyle;
            _titleRoot.style.display = displayStyle;
            _currentPointsLabel.style.display = displayStyle;
            _skillDetailRoot.style.display = displayStyle;
            _playerStatusRoot.style.display = displayStyle;
        }

        /// <summary>
        ///     連続解放演出の再生中であれば、入力を合図にノード演出とカメラワークを即座に完了させます。
        /// </summary>
        /// <returns> 何らかの演出をスキップした場合はtrue。 </returns>
        private bool TrySkipUnlockPerformance()
        {
            bool skippedNodeAnimation = _skillTreeController.SkipUnlockAnimation();
            bool skippedCameraWork = _unlockCameraRestoreItem != null;
            if (skippedCameraWork)
            {
                _unlockCameraRestoreItem.Pause();
                RestoreUnlockCameraFocus(_skillTreeController.SelectedNodeId);
            }

            return skippedNodeAnimation || skippedCameraWork;
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
            DumpSkillNodePositionsForDebug();
        }

        /// <summary>
        ///     診断用: 各スキルノードの画面上の実座標を一覧出力する。
        ///     フェーズコンテナの重なりなど、レイアウト起因の不具合調査に使用する。
        /// </summary>
        private void DumpSkillNodePositionsForDebug()
        {
            if (_skillNodeElementList == null)
            {
                return;
            }

            _rootElement.schedule.Execute(() =>
            {
                for (int i = 0; i < _skillNodeElementList.Count; i++)
                {
                    VisualElement node = _skillNodeElementList[i];
                    NavigationDebugLog.Log($"[SkillTreeNodePos] {node.name} bound={node.worldBound}");
                }
            });
        }

        /// <summary>
        ///     画面を閉じる時に保留中の初期フォーカス処理を中止する。
        /// </summary>
        private void HandleScreenClosedHandler()
        {
            _isSkillDetailOpen = false;
            _isUnlockConfirmOpen = false;
            _unlockConfirmDialogView?.Hide();
            _dialogNavigationScope.Deactivate();
            _skillDetailNavigationScope.Deactivate();
            _unlockCameraRestoreItem?.Pause();
            _unlockCameraRestoreItem = null;
            SetOtherUiVisibleDuringUnlockCameraWork(true);
            _skillTreeViewportView?.CancelFocus();
            _skillTreeViewportView?.ClearFocusZoom();
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
            _dialogNavigationScope.Activate(_skillTreeResetDialogView.DialogRoot);
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
            _isSkillDetailOpen = false;
            _isUnlockConfirmOpen = false;
            _unlockConfirmDialogView?.Hide();
            _dialogNavigationScope.Deactivate();
            _skillDetailNavigationScope.Deactivate();
            _skillDetailScreenView?.HideImmediately();
            dialogView.SetResetButtonVisible(true);
            _skillTreeViewportView?.ClearFocusZoom();
        }

        /// <summary>
        ///     スキルツリーリセットをキャンセルする。
        /// </summary>
        private void HandleSkillTreeResetCancelled()
        {
            _skillTreeResetDialogView.Hide();
            _dialogNavigationScope.Deactivate();
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
