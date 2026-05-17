using System.Threading.Tasks;
using System.Collections.Generic;
using AnimationEventData = KillChord.Runtime.Domain.OutGame.Scenario.AnimationEvent;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;
using KillChord.Runtime.InfraStructure.OutGame.Scenario;
using KillChord.Runtime.View.OutGame.Scenario;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生に必要な依存関係を組み立てて起動する。
    /// </summary>
    public class ScenarioCom : MonoBehaviour
    {
        [SerializeField]
        private ScenarioView _chatText;
        [SerializeField]
        private ScenarioInputView _inputView;
        [SerializeField]
        private BackgroundCatalogAsset _backgroundCatalog;
        [SerializeField]
        private AnimationCatalogAsset _animationCatalog;
        [SerializeField]
        private PortraitCatalogAsset _portraitCatalog;
        [SerializeField]
        private ScenarioSettingsAsset _scenarioSettings;
        private ScenarioUsecase _usecase;

        /// <summary>
        /// シナリオ再生の初期化を開始する。
        /// </summary>
        private async void Start()
        {
            try
            {
                await Init();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex, this);
                enabled = false;
            }
        }
        /// <summary>
        /// 依存関係を組み立ててシナリオ再生を開始する。
        /// </summary>
        private async ValueTask Init()
        {
            ScenarioAdvanceGate gate = new ScenarioAdvanceGate();
            ViewModel viewModel = new ViewModel();
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            IScenarioRepository repository = new ScenarioRepository();
            IBackgroundRepository backgroundRepository = new BackgroundRepository(_backgroundCatalog);
            IAnimationRepository animationRepository = new AnimationRepository(_animationCatalog);
            IPortraitRepository portraitRepository = new PortraitRepository(_portraitCatalog);
            IScenarioSettingsRepository scenarioSettingsRepository = new ScenarioSettingsRepository(_scenarioSettings);

            TextPresenter textPresenter = new TextPresenter(viewModel);
            FadePresenter fadePresenter = new FadePresenter(viewModel);
            BackgroundPresenter backgroundPresenter = new BackgroundPresenter(viewModel);
            AnimationPresenter animationPresenter = new AnimationPresenter(viewModel);
            PortraitPresenter portraitPresenter = new PortraitPresenter(viewModel);
            LayerPresenter layerPresenter = new LayerPresenter(viewModel);
            ScenarioPresenterFacade presenterFacade = new ScenarioPresenterFacade(
                textPresenter,
                fadePresenter,
                backgroundPresenter,
                animationPresenter,
                portraitPresenter,
                layerPresenter,
                viewModel);

            _usecase = new ScenarioUsecase(
                repository,
                handlerRepo,
                gate,
                presenterFacade,
                scenarioSettingsRepository);
            InputController controller = new InputController(gate, _usecase);
            TextEventHandler textHandle = new TextEventHandler(
                presenterFacade,
                _usecase,
                _usecase,
                scenarioSettingsRepository);
            FadeEventHandler fadeEventHandle = new FadeEventHandler(presenterFacade);
            BackgroundEventHandler backgroundEventHandle = new BackgroundEventHandler(presenterFacade, backgroundRepository);
            AnimationEventHandler animationEventHandle = new AnimationEventHandler(presenterFacade, animationRepository);
            PortraitEventHandler portraitEventHandler = new PortraitEventHandler(presenterFacade, portraitRepository);
            LayerEventHandler layerEventHandler = new LayerEventHandler(presenterFacade);
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeEventHandle.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundEventHandle.HandleAsync);
            handlerRepo.Register<AnimationEventData>(animationEventHandle.HandleAsync);
            handlerRepo.Register<PortraitEvent>(portraitEventHandler.HandleAsync);
            handlerRepo.Register<LayerEvent>(layerEventHandler.HandleAsync);

            // View を生成する。
            ScenarioView view = Instantiate(_chatText, Vector3.zero, Quaternion.identity);
            var backgroundMap = BuildBackgroundMap(_backgroundCatalog);
            var animationMap = BuildAnimationMap(_animationCatalog);
            var portraitMap = BuildPortraitMap(_portraitCatalog);
            view.Initialize(viewModel, backgroundMap, animationMap, portraitMap);
            ScenarioInputView inputView = Instantiate(_inputView, Vector3.zero, Quaternion.identity);
            inputView.Initialize(controller);
            await _usecase.PlayScenario();
        }

        /// <summary>
        /// 無効化時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDisable()
        {
            _usecase?.RequestSkip();
        }

        /// <summary>
        /// 破棄時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDestroy()
        {
            _usecase?.RequestSkip();
        }

        /// <summary>
        /// 背景アセット参照用の辞書を構築する。
        /// </summary>
        private static IReadOnlyDictionary<string, Sprite> BuildBackgroundMap(BackgroundCatalogAsset catalog)
        {
            var map = new Dictionary<string, Sprite>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null) continue;
                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        /// アニメーションアセット参照用の辞書を構築する。
        /// </summary>
        private static IReadOnlyDictionary<string, AnimationClip> BuildAnimationMap(AnimationCatalogAsset catalog)
        {
            var map = new Dictionary<string, AnimationClip>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null) continue;
                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }

        /// <summary>
        /// 立ち絵アセット参照用の辞書を構築する。
        /// </summary>
        private static IReadOnlyDictionary<string, Sprite> BuildPortraitMap(PortraitCatalogAsset catalog)
        {
            var map = new Dictionary<string, Sprite>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null) continue;
                string key = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
                map[key] = entry.Asset;
            }

            return map;
        }
    }
}