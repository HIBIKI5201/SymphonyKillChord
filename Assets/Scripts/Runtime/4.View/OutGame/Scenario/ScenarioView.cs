using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class ScenarioView : MonoBehaviour
    {
        private const string SlotLeft = "Left";
        private const string SlotCenter = "Center";
        private const string SlotRight = "Right";
        private const string PortraitObjectLeft = "PortraitLeft";
        private const string PortraitObjectCenter = "PortraitCenter";
        private const string PortraitObjectRight = "PortraitRight";
        private const string TargetCanvas = "Canvas";
        private const string TargetBackground = "Background";
        private const string TargetPortraitLeft = "PortraitLeft";
        private const string TargetPortraitCenter = "PortraitCenter";
        private const string TargetPortraitRight = "PortraitRight";
        private const string TargetText = "Text";

        public void Initialize(
            ViewModel viewModel,
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey)
        {
            UnsubscribeFromViewModel();
            _viewModel = viewModel;
            SubscribeToViewModel();
            BuildCatalogMaps(backgroundByKey, animationByKey, portraitByKey);
            EnsurePortraitSlots();
            ResetFadeState();
        }

        [SerializeField]
        private TMP_Text _chat;
        [SerializeField]
        private Image _backgroundImage;
        [SerializeField]
        private Animation _animationPlayer;
        [SerializeField]
        private GameObject _fadeObj;
        [SerializeField]
        private RectTransform _portraitRoot;

        private bool _onFade;
        private float _time;
        private float _start;
        private float _end;
        private float _duration;

        private readonly Dictionary<string, Sprite> _backgroundByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, AnimationClip> _animationByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Sprite> _portraitByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Image> _portraitBySlot = new(System.StringComparer.OrdinalIgnoreCase);
        private ViewModel _viewModel;

        private void Update()
        {
            Fade();
        }

        private void OnTextReceived(string chat)
        {
            if (_chat == null)
            {
                Debug.LogWarning("ScenarioView: _chat is not assigned.");
                return;
            }
            _chat.text = chat;
        }

        private void OnFadeReceived(float start, float end, float duration)
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

        private void InputPortrait(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible)
        {
            if (!_portraitBySlot.TryGetValue(slot, out Image portraitImage)) return;

            if (!string.IsNullOrWhiteSpace(assetKey) &&
                _portraitByKey.TryGetValue(assetKey, out Sprite portrait) &&
                portrait != null)
            {
                portraitImage.sprite = portrait;
            }

            RectTransform rectTransform = portraitImage.rectTransform;
            rectTransform.anchoredPosition = new Vector2(positionX, positionY);
            rectTransform.localScale = Vector3.one * (scale <= 0f ? 1f : scale);
            portraitImage.enabled = visible && portraitImage.sprite != null;
        }

        private void InputLayerOrder(string target, int order)
        {
            if (string.Equals(target, TargetCanvas, System.StringComparison.OrdinalIgnoreCase))
            {
                Canvas canvas = GetComponent<Canvas>();
                if (canvas == null) return;

                canvas.overrideSorting = true;
                canvas.sortingOrder = order;
                return;
            }

            RectTransform targetRect = ResolveLayerTargetRect(target);
            if (targetRect == null) return;

            int childCount = targetRect.parent != null ? targetRect.parent.childCount : 0;
            if (childCount <= 0) return;
            int clampedOrder = Mathf.Clamp(order, 0, childCount - 1);
            targetRect.SetSiblingIndex(clampedOrder);
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
            Debug.Log(skipped
                ? "シナリオ完了イベント発火: スキップ終了"
                : "シナリオ完了イベント発火: 全テキスト表示完了");
            gameObject.SetActive(false);
        }

        private void BuildCatalogMaps(
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey)
        {
            CopyCatalogEntries(backgroundByKey, _backgroundByKey);
            CopyCatalogEntries(animationByKey, _animationByKey);
            CopyCatalogEntries(portraitByKey, _portraitByKey);
        }

        private void EnsurePortraitSlots()
        {
            EnsurePortraitSlot(SlotLeft, PortraitObjectLeft, new Vector2(-420f, -120f));
            EnsurePortraitSlot(SlotCenter, PortraitObjectCenter, new Vector2(0f, -120f));
            EnsurePortraitSlot(SlotRight, PortraitObjectRight, new Vector2(420f, -120f));
        }

        private void EnsurePortraitSlot(string slot, string objectName, Vector2 defaultPosition)
        {
            if (_portraitBySlot.ContainsKey(slot)) return;

            RectTransform root = _portraitRoot;
            if (root == null)
            {
                root = transform as RectTransform;
            }
            if (root == null) return;

            Transform existing = root.Find(objectName);
            GameObject go = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (existing == null)
            {
                rectTransform.SetParent(root, false);
                rectTransform.anchorMin = new Vector2(0.5f, 0f);
                rectTransform.anchorMax = new Vector2(0.5f, 0f);
                rectTransform.pivot = new Vector2(0.5f, 0f);
                rectTransform.sizeDelta = new Vector2(700f, 1000f);
                rectTransform.anchoredPosition = defaultPosition;
            }

            Image image = go.GetComponent<Image>();
            image.preserveAspect = true;
            image.enabled = image.sprite != null;
            _portraitBySlot[slot] = image;
        }

        private RectTransform GetPortraitRect(string slot)
        {
            return _portraitBySlot.TryGetValue(slot, out Image image) && image != null ? image.rectTransform : null;
        }

        private RectTransform ResolveLayerTargetRect(string target)
        {
            if (string.Equals(target, TargetBackground, System.StringComparison.OrdinalIgnoreCase))
            {
                return _backgroundImage != null ? _backgroundImage.rectTransform : null;
            }
            if (string.Equals(target, TargetPortraitLeft, System.StringComparison.OrdinalIgnoreCase))
            {
                return GetPortraitRect(SlotLeft);
            }
            if (string.Equals(target, TargetPortraitCenter, System.StringComparison.OrdinalIgnoreCase))
            {
                return GetPortraitRect(SlotCenter);
            }
            if (string.Equals(target, TargetPortraitRight, System.StringComparison.OrdinalIgnoreCase))
            {
                return GetPortraitRect(SlotRight);
            }
            if (string.Equals(target, TargetText, System.StringComparison.OrdinalIgnoreCase))
            {
                return _chat != null ? _chat.rectTransform : null;
            }

            return null;
        }

        private static void CopyCatalogEntries<T>(
            IReadOnlyDictionary<string, T> source,
            Dictionary<string, T> destination)
            where T : class
        {
            destination.Clear();
            if (source == null) return;

            foreach (var entry in source)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null) continue;
                destination[entry.Key] = entry.Value;
            }
        }

        private void ResetFadeState()
        {
            _onFade = false;
            _time = 0f;
            _start = 0f;
            _end = 0f;
            _duration = 0f;
        }

        private void OnDestroy()
        {
            UnsubscribeFromViewModel();
        }

        private void SubscribeToViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.OnChat += OnTextReceived;
            _viewModel.OnFade += OnFadeReceived;
            _viewModel.OnBackground += InputBackground;
            _viewModel.OnAnimation += InputAnimation;
            _viewModel.OnPortrait += InputPortrait;
            _viewModel.OnLayerOrder += InputLayerOrder;
            _viewModel.OnScenarioCompleted += InputScenarioCompleted;
        }

        private void UnsubscribeFromViewModel()
        {
            if (_viewModel == null) return;

            _viewModel.OnChat -= OnTextReceived;
            _viewModel.OnFade -= OnFadeReceived;
            _viewModel.OnBackground -= InputBackground;
            _viewModel.OnAnimation -= InputAnimation;
            _viewModel.OnPortrait -= InputPortrait;
            _viewModel.OnLayerOrder -= InputLayerOrder;
            _viewModel.OnScenarioCompleted -= InputScenarioCompleted;
            _viewModel = null;
        }
    }
}

