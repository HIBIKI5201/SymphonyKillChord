using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        public void Initilize(
            ViewModel viewModel,
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey)
        {
            viewModel.OnChat += InputViewModel;
            viewModel.OnFade += InputViewModel;
            viewModel.OnBackground += InputBackground;
            viewModel.OnAnimation += InputAnimation;
            viewModel.OnScenarioCompleted += InputScenarioCompleted;
            BuildCatalogMaps(backgroundByKey, animationByKey);
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

        private readonly Dictionary<string, Sprite> _backgroundByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, AnimationClip> _animationByKey = new(System.StringComparer.Ordinal);

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

        private void InputBackground(string assetKey)
        {
            if (_backgroundImage == null) return;
            if (string.IsNullOrWhiteSpace(assetKey)) return;
            if (!_backgroundByKey.TryGetValue(assetKey, out Sprite background)) return;
            if (background == null) return;

            _backgroundImage.sprite = background;
        }

        private void InputAnimation(string assetKey)
        {
            if (_animationPlayer == null) return;
            if (string.IsNullOrWhiteSpace(assetKey)) return;
            if (!_animationByKey.TryGetValue(assetKey, out AnimationClip animationClip)) return;
            if (animationClip == null) return;

            _animationPlayer.clip = animationClip;
            _animationPlayer.Play();
        }

        private void Fade()
        {
            if (!_onFade) return;
            _time += Time.deltaTime;

            if (_duration <= 0f)
            {
                _chat.alpha = _end;
                _onFade = false;
                return;
            }

            float t = _time / _duration;
            t = Mathf.Clamp01(t);
            _chat.alpha = Mathf.Lerp(_start, _end, t);

            if (t >= 1f) _onFade = false;
        }

        private void InputScenarioCompleted(bool skipped)
        {
            gameObject.SetActive(false);
        }

        private void BuildCatalogMaps(
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey)
        {
            _backgroundByKey.Clear();
            _animationByKey.Clear();

            if (backgroundByKey != null)
            {
                foreach (var entry in backgroundByKey)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null) continue;
                    _backgroundByKey[entry.Key] = entry.Value;
                }
            }

            if (animationByKey != null)
            {
                foreach (var entry in animationByKey)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null) continue;
                    _animationByKey[entry.Key] = entry.Value;
                }
            }
        }
    }
}
