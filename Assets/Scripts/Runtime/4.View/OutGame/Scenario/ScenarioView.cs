using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        public void Initilize(ViewModel viewModel)
        {
            viewModel.OnChat += InputViewModel;
            viewModel.OnFade += InputViewModel;
            viewModel.OnBackground += InputBackground;
            viewModel.OnAnimation += InputAnimation;
            viewModel.OnScenarioCompleted += InputScenarioCompleted;
        }

        [SerializeField]
        private TMP_Text _chat;
        [SerializeField]
        private Image _backgroundImage;
        [SerializeField]
        private Animation _animationPlayer;
        [SerializeField]
        private GameObject _fadeObj;

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

        private void InputBackground(Sprite background)
        {
            if (_backgroundImage == null || background == null) return;
            _backgroundImage.sprite = background;
        }

        private void InputAnimation(AnimationClip animationClip)
        {
            if (_animationPlayer == null || animationClip == null) return;
            _animationPlayer.clip = animationClip;
            _animationPlayer.Play();
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

        private void InputScenarioCompleted(bool skipped)
        {
            gameObject.SetActive(false);
        }

    }
}
