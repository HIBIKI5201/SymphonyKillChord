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

        private void Start()
        {
            Init();
        }
        private async ValueTask Init()
        {
            Debug.Log(Time.time);
            IOutPutPort outPutPort = new ViewModel();
            IScenarioRepository repo = new ScenarioRepository();
            TextEventHandler textHandle = new TextEventHandler(outPutPort);
            IScenarioEventHandler[] handlers = { textHandle };
            ScenarioHandlerRepo handlerRepo = new ScenarioHandlerRepo(handlers);
            ScenarioUsecase usecase = new ScenarioUsecase(repo, handlerRepo);
            await usecase.PlayScenario();
            Debug.Log(Time.time);
        }
    }
}
