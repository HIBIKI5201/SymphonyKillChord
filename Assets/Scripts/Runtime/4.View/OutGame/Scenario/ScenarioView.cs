using UnityEngine;
using TMPro;
namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        public void Initilize(ViewModel viewModel)
        {
            viewModel.OnChat += InputViewModel;
            viewModel.OnFade += InputViewModel;
            viewModel.OnBackground += InputBackground;
        }

        [SerializeField]
        private TMP_Text _chat;
        [SerializeField]
        private TMP_Text _backgroundLabel;

        private bool _onFade;
        private float _time;
        private float _start;
        private float _end;
        private float _duration;

        private void Update()
        {
            Fade();
        }
        private void InputViewModel(string chat)
        {
            _chat.text = chat;
        }

        private void InputViewModel(float start, float end, float duration)
        {
            _onFade = true;
            _time = 0f;
            _start = start;
            _end = end;
            _duration = duration;
        }

        private void InputBackground(string backgroundId)
        {
            if (_backgroundLabel != null)
            {
                _backgroundLabel.text = backgroundId;
            }
        }

        private void Fade()
        {
            if (!_onFade) return;
            _time += Time.deltaTime;
            float t = _time / _duration;
            t = Mathf.Clamp01(t);

            _chat.alpha = Mathf.Lerp(_start, _end, t);

            if (t >= 1f) _onFade = false;
        }

    }
}
