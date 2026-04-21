using System.Threading.Tasks;
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
        private AnimationCatalogAsset _animationCatalog;
        [SerializeField]
        private ScenarioSettingsAsset _scenarioSettings;

        private async void Start()
        {
            await Init();
        }
        private async ValueTask Init()
        {
            Debug.Log(Time.time);
            ScenarioAdvanceGate gate = new ScenarioAdvanceGate();
            ViewModel viewModel = new ViewModel();
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            IScenarioRepository repository = new ScenarioRepository();
            IBackgroundRepository backgroundRepository = new BackgroundRepository(_backgroundCatalog);
            IAnimationRepository animationRepository = new AnimationRepository(_animationCatalog);
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
            AnimationEventHandler animationEventHandle = new AnimationEventHandler(presenterFacade, animationRepository);
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeEventHandle.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundEventHandle.HandleAsync);
            handlerRepo.Register<KillChord.Runtime.Domain.AnimationEvent>(animationEventHandle.HandleAsync);
            ScenarioView view = Instantiate(_chatText, Vector3.zero, Quaternion.identity);
            view.Initilize(viewModel);
            ScenarioInputView inputView = Instantiate(_inputView, Vector3.zero, Quaternion.identity);
            inputView.Initilize(controller);
            await usecase.PlayScenario();
            Debug.Log(Time.time);
        }
    }
}
