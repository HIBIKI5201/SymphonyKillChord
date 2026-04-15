

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class ScenalioViewModel : IScenalioOutputPort
    {
        public ScenalioViewModel(IScenalioView view)
        {
            _view = view;
        }

        public async void Present(IScenalioEvent senalioEvent)
        {
            Type type = senalioEvent.GetType();

            if (_handlers.TryGetValue(type, out IScenalioViewEventHandler handle))
            {
                await handle.ExecuteAsync(senalioEvent, _view);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"ハンドラーが登録されていません : {type.Name}");
            }

            _useCase.NotifyCompleted();
        }

        public void RegisterHandle<T>(ScenalioViewEventHandlerBase<T> handle) where T : IScenalioEvent
        {
            _handlers[typeof(T)] = handle;
        }

        public void Initialize(IScenalioUsecase useCase)
        {
            _useCase = useCase;
        }

        private readonly Dictionary<Type, IScenalioViewEventHandler> _handlers = new();
        private readonly IScenalioView _view;
        private IScenalioUsecase _useCase;

    }
}
