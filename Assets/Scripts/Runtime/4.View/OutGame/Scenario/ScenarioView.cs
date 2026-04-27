using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        public void Initialize(
            ViewModel viewModel,
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey)
        {
            UnsubscribeFromViewModel();
            _viewModel = viewModel;
            SubscribeToViewModel();
            BuildCatalogMaps(backgroundByKey, portraitByKey);
        }

        [SerializeField]
        private TMP_Text _chat;
        [SerializeField]
        private Image _backgroundImage;
        [SerializeField]
        private PortraitSlotBinding[] _portraitSlots = System.Array.Empty<PortraitSlotBinding>();
        // Compatibility only: keep old serialized reference but hide it from inspector.
        [FormerlySerializedAs("_portraitImage")]
        [SerializeField, HideInInspector]
        private Image _legacyPortraitImage;
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
        private readonly Dictionary<string, Sprite> _portraitByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Image> _portraitImageBySlot = new(System.StringComparer.OrdinalIgnoreCase);
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

        private void InputPortrait(string slotId, string assetKey)
        {
            if (string.IsNullOrWhiteSpace(assetKey)) return;
            if (!_portraitByKey.TryGetValue(assetKey, out Sprite portrait)) return;
            if (portrait == null) return;

            if (!TryResolvePortraitImage(slotId, out Image targetImage)) return;
            targetImage.sprite = portrait;
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

        private void BuildCatalogMaps(
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey)
        {
            _backgroundByKey.Clear();
            _portraitByKey.Clear();
            _portraitImageBySlot.Clear();
            _animatorTriggerHashes.Clear();
            BuildPortraitSlotMap();

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

            if (portraitByKey != null)
            {
                foreach (var entry in portraitByKey)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null) continue;
                    _portraitByKey[entry.Key] = entry.Value;
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
            _viewModel.OnPortrait += InputPortrait;
            _viewModel.OnScenarioCompleted += InputScenarioCompleted;
        }

        private void UnsubscribeFromViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.OnChat -= InputViewModel;
            _viewModel.OnFade -= InputViewModel;
            _viewModel.OnBackground -= InputBackground;
            _viewModel.OnAnimation -= InputAnimation;
            _viewModel.OnPortrait -= InputPortrait;
            _viewModel.OnScenarioCompleted -= InputScenarioCompleted;
            _viewModel = null;
        }

        private bool TryResolvePortraitImage(string slotId, out Image image)
        {
            string normalizedSlot = string.IsNullOrWhiteSpace(slotId)
                ? ViewModel.DefaultPortraitSlotId
                : slotId;

            if (_portraitImageBySlot.TryGetValue(normalizedSlot, out image) && image != null)
            {
                return true;
            }

            if (_portraitImageBySlot.TryGetValue(ViewModel.DefaultPortraitSlotId, out image) && image != null)
            {
                return true;
            }

            if (_legacyPortraitImage != null)
            {
                image = _legacyPortraitImage;
                return true;
            }

            foreach (var slot in _portraitImageBySlot)
            {
                if (slot.Value == null) continue;
                image = slot.Value;
                return true;
            }

            image = null;
            return false;
        }

        private void BuildPortraitSlotMap()
        {
            if (_portraitSlots != null)
            {
                for (int i = 0; i < _portraitSlots.Length; i++)
                {
                    PortraitSlotBinding slot = _portraitSlots[i];
                    if (slot.Image == null) continue;
                    if (string.IsNullOrWhiteSpace(slot.SlotId)) continue;
                    _portraitImageBySlot[slot.SlotId] = slot.Image;
                }
            }

            // Migrate old single-image setup to Default slot if slots are not configured yet.
            if (_portraitImageBySlot.Count == 0 && _legacyPortraitImage != null)
            {
                _portraitImageBySlot[ViewModel.DefaultPortraitSlotId] = _legacyPortraitImage;
            }
        }

        [System.Serializable]
        private struct PortraitSlotBinding
        {
            public string SlotId;
            public Image Image;
        }
    }
}
