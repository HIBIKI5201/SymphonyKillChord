
using System.Collections.Generic;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class CompositionRoot : MonoBehaviour
    {
        [SerializeField]
        private ScenalioView _view;
        private void Awake()
        {
            var viewModel = new ScenalioViewModel(_view);
            var player = new ScenalioPlayer(viewModel);
            viewModel.Initialize(player);

            viewModel.RegisterHandle(new ShowTextViewHandler());
            viewModel.RegisterHandle(new FadeBackgroundViewHandler());

            var data = CreateMockData();
            player.Start(data);
        }

        private ScenalioData CreateMockData()
        {
            List<IScenalioEvent> events = new List<IScenalioEvent>
            {
              new ShowTextEvent("システムを起動します。"),
              new FadeBackgroundEvent(2.0f),
              new ShowTextEvent("シナリオシステムが起動しました。"),
              new ShowTextEvent("これからゲームをはじめます。"),
            };

            return new ScenalioData(events);
        }
    }
}
