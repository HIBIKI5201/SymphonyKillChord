using R3;
using System;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MissionHudView : MonoBehaviour
    {
        public void Initialize(MissionHudViewModel viewModel)
        {
            _mainMissionDisposable = viewModel.MainMissionText.Subscribe(value =>
            {
                if (_mainMissionText != null)
                    _mainMissionText.text = value;
            });

            _resultDisposable = viewModel.ResultText.Subscribe(value =>
            {
                if (_resultText != null)
                    _resultText.text = value;
            });
        }

        private void OnDestroy()
        {
            _mainMissionDisposable?.Dispose();
            _resultDisposable?.Dispose();

        }

        [SerializeField] private TMP_Text _mainMissionText;
        [SerializeField] private TMP_Text _resultText;

        private IDisposable _mainMissionDisposable;
        private IDisposable _resultDisposable;
    }
}
