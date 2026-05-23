using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    [DefaultExecutionOrder(10)]
    /// <summary>
    /// シナリオの表示状態を Unity UI に反映するビュー。
    /// </summary>
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
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _chat;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Animation _animationPlayer;
        [SerializeField] private GameObject _fadeObj;
        [SerializeField] private RectTransform _portraitRoot;
        [SerializeField] private Vector2 _portraitSize = new(700f, 1000f);

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

        /// <summary>
        /// 依存先を受け取りシナリオ表示を初期化する。
        /// </summary>
        public void Initialize(
            ViewModel viewModel,
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey)
        {
            TryAutoAssignReferences();
            UnsubscribeFromViewModel();
            _viewModel = viewModel;
            SubscribeToViewModel();
            BuildCatalogMaps(backgroundByKey, animationByKey, portraitByKey);
            EnsurePortraitSlots();
            ResetFadeState();
        }

        /// <summary>
        /// 表示に必要な参照を初期化する。
        /// </summary>
        private void Awake()
        {
            TryAutoAssignReferences();
            EnsurePortraitSlots();
        }

        /// <summary>
        /// インスペクター変更時に参照と表示設定を補正する。
        /// </summary>
        private void OnValidate()
        {
            TryAutoAssignReferences();
            ApplyPortraitSizeToExistingSlots();
        }

        /// <summary>
        /// 毎フレームの入力監視または演出更新を行う。
        /// </summary>
        private void Update()
        {
            Fade();
        }

        /// <summary>
        /// 受け取ったテキストを表示へ反映する。
        /// </summary>
        private void OnTextReceived(string chat)
        {
            if (_chat == null)
            {
                Debug.LogWarning("ScenarioView: _chat is not assigned.");
                return;
            }

            _chat.text = chat;
        }

        /// <summary>
        /// フェード要求を受け取りアニメーション状態を更新する。
        /// </summary>
        private void OnFadeReceived(float start, float end, float duration)
        {
            _onFade = true;
            _time = 0f;
            _start = start;
            _end = end;
            _duration = duration;
        }

        /// <summary>
        /// 背景表示要求を背景画像へ反映する。
        /// </summary>
        private void InputBackground(string assetKey)
        {
            if (_backgroundImage == null || string.IsNullOrWhiteSpace(assetKey))
            {
                return;
            }

            if (!_backgroundByKey.TryGetValue(assetKey, out Sprite background) || background == null)
            {
                return;
            }

            _backgroundImage.sprite = background;
        }

        /// <summary>
        /// アニメーション再生要求を表示へ反映する。
        /// </summary>
        private void InputAnimation(string assetKey)
        {
            if (_animationPlayer == null || string.IsNullOrWhiteSpace(assetKey))
            {
                return;
            }

            if (!_animationByKey.TryGetValue(assetKey, out AnimationClip animationClip) || animationClip == null)
            {
                return;
            }

            _animationPlayer.clip = animationClip;
            _animationPlayer.Play();
        }

        /// <summary>
        /// 立ち絵表示要求を対象スロットへ反映する。
        /// </summary>
        private void InputPortrait(string slot, string assetKey, float positionX, float positionY, float scale, bool visible)
        {
            EnsurePortraitSlots();
            if (!_portraitBySlot.TryGetValue(slot, out Image portraitImage) || portraitImage == null)
            {
                return;
            }

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

        /// <summary>
        /// レイヤー順変更要求を対象 UI へ反映する。
        /// </summary>
        private void InputLayerOrder(string target, int order)
        {
            if (string.Equals(target, TargetCanvas, System.StringComparison.OrdinalIgnoreCase))
            {
                Canvas canvas =   GetComponent<Canvas>();
                if (canvas == null)
                {
                    return;
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = order;
                return;
            }

            RectTransform targetRect = ResolveLayerTargetRect(target);
            if (targetRect == null)
            {
                return;
            }

            if (targetRect.parent == null)
            {
                return;
            }

            int childCount = targetRect.parent.childCount;
            if (childCount <= 0)
            {
                return;
            }

            int clampedOrder = Mathf.Clamp(order, 0, childCount - 1);
            targetRect.SetSiblingIndex(clampedOrder);
        }

        /// <summary>
        /// 進行中のフェード演出を 1 フレーム分更新する。
        /// </summary>
        private void Fade()
        {
            if (!_onFade)
            {
                return;
            }

            _time += Time.deltaTime;
            if ( _canvasGroup == null)
            {
                Debug.LogWarning("ScenarioView: _canvasGroup is not assigned.");
                _onFade = false;
                return;
            }

            if (_duration <= 0f)
            {
                 _canvasGroup.alpha = _end;
                _onFade = false;
                return;
            }

            float t = Mathf.Clamp01(_time / _duration);
            _canvasGroup.alpha = Mathf.Lerp(_start, _end, t);
            if (t >= 1f)
            {
                _onFade = false;
            }
        }

        /// <summary>
        /// シナリオ完了時の後処理を表示へ反映する。
        /// </summary>
        private void InputScenarioCompleted(bool skipped)
        {
            Debug.Log(skipped
                ? "シナリオ再生完了: スキップ終了。"
                : "シナリオ再生完了: 全テキスト表示終了。");
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 表示用カタログ辞書を構築する。
        /// </summary>
        private void BuildCatalogMaps(
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey)
        {
            CopyCatalogEntries(backgroundByKey, _backgroundByKey);
            CopyCatalogEntries(animationByKey, _animationByKey);
            CopyCatalogEntries(portraitByKey, _portraitByKey);
        }

        /// <summary>
        /// 未設定の参照を自動で補完する。
        /// </summary>
        private void TryAutoAssignReferences()
        {
            if (_chat == null)
            {
                _chat = GetComponentInChildren<TMP_Text>(true);
            }

            if (_backgroundImage == null)
            {
                Transform panel = transform.Find("Panel");
                _backgroundImage = panel != null ? panel.GetComponent<Image>() : GetComponentInChildren<Image>(true);
            }

            if (_fadeObj == null)
            {
                _fadeObj = gameObject;
            }

            if (_portraitRoot == null)
            {
                _portraitRoot = transform as RectTransform;
            }
        }

        /// <summary>
        /// 立ち絵表示に必要なスロットをそろえる。
        /// </summary>
        private void EnsurePortraitSlots()
        {
            EnsurePortraitSlot(SlotLeft, PortraitObjectLeft, new Vector2(-420f, -120f));
            EnsurePortraitSlot(SlotCenter, PortraitObjectCenter, new Vector2(0f, -120f));
            EnsurePortraitSlot(SlotRight, PortraitObjectRight, new Vector2(420f, -120f));
            ApplyPortraitSizeToExistingSlots();
        }

        /// <summary>
        /// 指定スロットの表示オブジェクトを確保する。
        /// </summary>
        private void EnsurePortraitSlot(string slot, string objectName, Vector2 defaultPosition)
        {
            if (_portraitBySlot.ContainsKey(slot))
            {
                return;
            }

            RectTransform root = _portraitRoot != null ? _portraitRoot : transform as RectTransform;
            if (root == null)
            {
                return;
            }

            Transform existing = root.Find(objectName);
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (existing == null)
            {
                rectTransform.SetParent(root, false);
                rectTransform.anchorMin = new Vector2(0.5f, 0f);
                rectTransform.anchorMax = new Vector2(0.5f, 0f);
                rectTransform.pivot = new Vector2(0.5f, 0f);
                rectTransform.anchoredPosition = defaultPosition;
            }

            rectTransform.sizeDelta = GetValidatedPortraitSize();

            Image image = go.GetComponent<Image>();
            image.preserveAspect = true;
            image.enabled = image.sprite != null;
            _portraitBySlot[slot] = image;
        }

        /// <summary>
        /// 既存の立ち絵スロットへサイズ設定を適用する。
        /// </summary>
        private void ApplyPortraitSizeToExistingSlots()
        {
            Vector2 validatedSize = GetValidatedPortraitSize();
            foreach (Image portraitImage in _portraitBySlot.Values)
            {
                if (portraitImage == null)
                {
                    continue;
                }

                portraitImage.rectTransform.sizeDelta = validatedSize;
            }
        }

        /// <summary>
        /// 最小値を保証した立ち絵サイズを取得する。
        /// </summary>
        private Vector2 GetValidatedPortraitSize()
        {
            return new Vector2(
                Mathf.Max(1f, _portraitSize.x),
                Mathf.Max(1f, _portraitSize.y));
        }

        /// <summary>
        /// 指定スロットの RectTransform を取得する。
        /// </summary>
        private RectTransform GetPortraitRect(string slot)
        {
            return _portraitBySlot.TryGetValue(slot, out Image image) && image != null
                ? image.rectTransform
                : null;
        }

        /// <summary>
        /// レイヤー制御対象に対応する RectTransform を取得する。
        /// </summary>
        private RectTransform ResolveLayerTargetRect(string target)
        {
            EnsurePortraitSlots();
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
            if (source == null)
            {
                return;
            }

            foreach (KeyValuePair<string, T> entry in source)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null)
                {
                    continue;
                }

                destination[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// フェード演出の内部状態を初期化する。
        /// </summary>
        private void ResetFadeState()
        {
            _onFade = false;
            _time = 0f;
            _start = 0f;
            _end = 0f;
            _duration = 0f;
        }

        /// <summary>
        /// 破棄時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromViewModel();
        }

        /// <summary>
        /// ViewModel の通知を購読する。
        /// </summary>
        private void SubscribeToViewModel()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.OnChat += OnTextReceived;
            _viewModel.OnFade += OnFadeReceived;
            _viewModel.OnBackground += InputBackground;
            _viewModel.OnAnimation += InputAnimation;
            _viewModel.OnPortrait += InputPortrait;
            _viewModel.OnLayerOrder += InputLayerOrder;
            _viewModel.OnScenarioCompleted += InputScenarioCompleted;
        }

        /// <summary>
        /// ViewModel の通知購読を解除する。
        /// </summary>
        private void UnsubscribeFromViewModel()
        {
            if (_viewModel == null)
            {
                return;
            }

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