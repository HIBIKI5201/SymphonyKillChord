using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        public void Initialize(
            ViewModel viewModel,
            IReadOnlyDictionary<string, Sprite> backgroundByKey)
        {
            UnsubscribeFromViewModel();
            _viewModel = viewModel;
            SubscribeToViewModel();
            BuildCatalogMaps(backgroundByKey);
        }

        [SerializeField]
        private TMP_Text _chat;
        [SerializeField]
        private Image _backgroundImage;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private GameObject _fadeObj;

        private bool _onFade;
        private float _time;
        private float _start;
        private float _end;
        private float _duration;

        private readonly Dictionary<string, Sprite> _backgroundByKey = new(System.StringComparer.Ordinal);
        private readonly HashSet<int> _animatorTriggerHashes = new();
        private ViewModel _viewModel;

        private void Update()
        {
            Fade();
        }

        private void InputViewModel(string chat)
        {
            if (_chat == null)
            {
                Debug.LogWarning("ScenarioView: _chat is not assigned.");
                return;
            }
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

        private void InputAnimation(string animationId)
        {
            if (string.IsNullOrWhiteSpace(animationId)) return;

            if (TryPlayByAnimator(animationId))
            {
                return;
            }

            Debug.LogWarning($"[ScenarioView] Animation id not found in Animator: {animationId}", this);
        }

        private bool TryPlayByAnimator(string animationId)
        {
            if (_animator == null)
            {
                return false;
            }

            int hash = Animator.StringToHash(animationId);
            if (_animatorTriggerHashes.Contains(hash))
            {
                _animator.SetTrigger(hash);
                return true;
            }

            for (int layer = 0; layer < _animator.layerCount; layer++)
            {
                if (!_animator.HasState(layer, hash))
                {
                    continue;
                }

                // String id can be either a trigger parameter name or a state name.
                _animator.CrossFade(hash, 0.05f, layer);
                return true;
            }

            return false;
        }

        private void Fade()
        {
            if (!_onFade) return;
            _time += Time.deltaTime;

            if (_duration <= 0f)
            {
                if (_chat == null)
                {
                    Debug.LogWarning("ScenarioView: _chat is not assigned.");
                    _onFade = false;
                    return;
                }
                _chat.alpha = _end;
                _onFade = false;
                return;
            }

            float t = _time / _duration;
            t = Mathf.Clamp01(t);
            if (_chat == null)
            {
                Debug.LogWarning("ScenarioView: _chat is not assigned.");
                _onFade = false;
                return;
            }
            _chat.alpha = Mathf.Lerp(_start, _end, t);

            if (t >= 1f) _onFade = false;
        }

        private void InputScenarioCompleted(bool skipped)
        {
            gameObject.SetActive(false);
        }

        private void BuildCatalogMaps(IReadOnlyDictionary<string, Sprite> backgroundByKey)
        {
            _backgroundByKey.Clear();
            _animatorTriggerHashes.Clear();

            if (backgroundByKey != null)
            {
                foreach (var entry in backgroundByKey)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null) continue;
                    _backgroundByKey[entry.Key] = entry.Value;
                }
            }

            if (_animator != null)
            {
                for (int i = 0; i < _animator.parameters.Length; i++)
                {
                    AnimatorControllerParameter parameter = _animator.parameters[i];
                    if (parameter.type != AnimatorControllerParameterType.Trigger) continue;
                    _animatorTriggerHashes.Add(parameter.nameHash);
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromViewModel();
        }

        private void SubscribeToViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.OnChat += InputViewModel;
            _viewModel.OnFade += InputViewModel;
            _viewModel.OnBackground += InputBackground;
            _viewModel.OnAnimation += InputAnimation;
            _viewModel.OnScenarioCompleted += InputScenarioCompleted;
        }

        private void UnsubscribeFromViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.OnChat -= InputViewModel;
            _viewModel.OnFade -= InputViewModel;
            _viewModel.OnBackground -= InputBackground;
            _viewModel.OnAnimation -= InputAnimation;
            _viewModel.OnScenarioCompleted -= InputScenarioCompleted;
            _viewModel = null;
        }
    }
}
