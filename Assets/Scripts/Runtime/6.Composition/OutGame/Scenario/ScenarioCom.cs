using System.Threading.Tasks;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
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
            TextEventHandler textHandle = new TextEventHandler(viewModel);
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo();
            handlerRepo.Register<TextEvent>(textHandle.HandleAsync);
            ScenarioUsecase usecase = new ScenarioUsecase(repo, handlerRepo, gate);
            ScenarioView view = Instantiate(_chatText, Vector3.zero, Quaternion.identity);
            view.Initilize(viewModel);
            ScenarioInputView inputView = Instantiate(_inputView, Vector3.zero, Quaternion.identity);
            inputView.Initilize(controller);
            await usecase.PlayScenario();
            Debug.Log(Time.time);
        }
    }
}
