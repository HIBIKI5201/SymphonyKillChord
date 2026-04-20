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

        private async void Start()
        {
            await Init();
        }
        private async ValueTask Init()
        {
            Debug.Log(Time.time);
            ScenarioAdvanceGate gate = new ScenarioAdvanceGate();
            InputController controller = new InputController(gate);
            ViewModel viewModel = new ViewModel();
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            IScenarioRepository repository = new ScenarioRepository();
            IBackgroundRepository backgroundRepository = new BackgroundRepository(_backgroundCatalog);
            IAnimationRepository animationRepository = new AnimationRepository(_animationCatalog);
            _ = animationRepository;
            ScenarioUsecase usecase = new ScenarioUsecase(repository, handlerRepo, gate);
            TextEventHandler textHandle = new TextEventHandler(viewModel, usecase);
            FadeEventHandler fadeEventHandle = new FadeEventHandler(viewModel);
            BackgroundEventHandler backgroundEventHandle = new BackgroundEventHandler(viewModel, backgroundRepository);
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeEventHandle.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundEventHandle.HandleAsync);
            ScenarioView view = Instantiate(_chatText, Vector3.zero, Quaternion.identity);
            view.Initilize(viewModel);
            ScenarioInputView inputView = Instantiate(_inputView, Vector3.zero, Quaternion.identity);
            inputView.Initilize(controller);
            await usecase.PlayScenario();
            Debug.Log(Time.time);
        }
    }
}
