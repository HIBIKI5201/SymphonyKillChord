using System;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class ScenalioPlayer : IScenalioUsecase
    {
        public ScenalioPlayer(IScenalioOutputPort outputPort)
        {
            _outputPort = outputPort;
        }
        public void Start(ScenalioData data)
        {
            _currentData = data;
            _index = 0;
            PlayeNext();
        }


        public void NotifyCompleted()
        {
            _index++;
            PlayeNext();
        }
        private void PlayeNext()
        {
            if (_index < _currentData.Events.Count)
            {
                _outputPort.Present(_currentData.Events[_index]);
            }
        }

        private readonly IScenalioOutputPort _outputPort;
        private ScenalioData _currentData;
        private int _index;

    }
}
