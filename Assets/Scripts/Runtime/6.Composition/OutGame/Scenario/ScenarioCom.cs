using System.Threading.Tasks;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure.OutGame.Scenario;
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
            IScenarioRepository repo = new ScenarioRepository();
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            ScenarioUsecase usecase = new ScenarioUsecase(repo, handlerRepo, gate);

            TextEventHandler textHandle = new TextEventHandler(viewModel, usecase);
            FadeEventHandler fadeHandle = new FadeEventHandler(viewModel);
            BackgroundEventHandler backgroundHandle = new BackgroundEventHandler(viewModel);
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            handlerRepo.Register<FadeEvent>(fadeHandle.HandleAsync);
            handlerRepo.Register<BackgroundEvent>(backgroundHandle.HandleAsync);

            ScenarioView view = Instantiate(_chatText, Vector3.zero, Quaternion.identity);
            view.Initilize(viewModel);
            ScenarioInputView inputView = Instantiate(_inputView, Vector3.zero, Quaternion.identity);
            inputView.Initilize(controller);
            await usecase.PlayScenario();
            Debug.Log(Time.time);
        }
    }
}
