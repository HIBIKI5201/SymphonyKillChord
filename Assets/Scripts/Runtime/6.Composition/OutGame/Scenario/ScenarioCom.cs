using System.Threading.Tasks;
using System.Collections.Generic;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class ScenarioCom : MonoBehaviour
    {
        [SerializeField]
        private ScenarioView _chatText;
        [SerializeField]
        private ScenarioInputView _inputView;
        [SerializeField]
        private BackgroundCatalogAsset _backgroundCatalog;
        [SerializeField]
        private ScenarioSettingsAsset _scenarioSettings;

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
        private async ValueTask Init()
        {
            Debug.Log(Time.time);
            ScenarioAdvanceGate gate = new ScenarioAdvanceGate();
            ViewModel viewModel = new ViewModel();
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            IScenarioRepository repository = new ScenarioRepository();
            IBackgroundRepository backgroundRepository = new BackgroundRepository(_backgroundCatalog);
            IScenarioSettingsRepository scenarioSettingsRepository = new ScenarioSettingsRepository(_scenarioSettings);

            TextPresenter textPresenter = new TextPresenter(viewModel);
            FadePresenter fadePresenter = new FadePresenter(viewModel);
            BackgroundPresenter backgroundPresenter = new BackgroundPresenter(viewModel);
            AnimationPresenter animationPresenter = new AnimationPresenter(viewModel);
            ScenarioPresenterFacade presenterFacade = new ScenarioPresenterFacade(
                textPresenter,
                fadePresenter,
                backgroundPresenter,
                animationPresenter,
                viewModel);

            ScenarioUsecase usecase = new ScenarioUsecase(
                repository,
                handlerRepo,
                gate,
                presenterFacade,
                scenarioSettingsRepository);
            InputController controller = new InputController(gate, usecase);
            TextEventHandler textHandle = new TextEventHandler(
                presenterFacade,
                usecase,
                usecase,
                scenarioSettingsRepository);
            FadeEventHandler fadeEventHandle = new FadeEventHandler(presenterFacade);
            BackgroundEventHandler backgroundEventHandle = new BackgroundEventHandler(presenterFacade, backgroundRepository);
            AnimationEventHandler animationEventHandle = new AnimationEventHandler(presenterFacade);
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeEventHandle.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundEventHandle.HandleAsync);
            handlerRepo.Register<Domain.AnimationEvent>(animationEventHandle.HandleAsync);

            //View生成
            ScenarioView view = Instantiate(_chatText, Vector3.zero, Quaternion.identity);
            var backgroundMap = BuildBackgroundMap(_backgroundCatalog);
            view.Initilize(viewModel, backgroundMap);
            ScenarioInputView inputView = Instantiate(_inputView, Vector3.zero, Quaternion.identity);
            inputView.Initilize(controller);
            await usecase.PlayScenario();
            Debug.Log(Time.time);
        }

        private static IReadOnlyDictionary<string, Sprite> BuildBackgroundMap(BackgroundCatalogAsset catalog)
        {
            var map = new Dictionary<string, Sprite>(System.StringComparer.Ordinal);
            if (catalog == null) return map;

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null) continue;
                map[entry.Id] = entry.Asset;
            }

            return map;
        }

    }
}
