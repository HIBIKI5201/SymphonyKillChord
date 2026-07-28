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
            EnsureNonFadingUi();
            UnsubscribeFromViewModel();
            _viewModel = viewModel;
            SubscribeToViewModel();
            BuildCatalogMaps(backgroundByKey, animationByKey, portraitByKey);
            EnsurePortraitSlots();
            ResetFadeState();
        }

        // CanvasGroup.alpha が 0 だと配下がカリングされ、ignoreParentGroups の
        // テキストも消える。実質不可視だがカリングは避けられる最小値。
        private const float MinCanvasAlpha = 0.004f;
        private const string SlotLeft = "Left";
        private const string SlotCenter = "Center";
        private const string SlotRight = "Right";
        private const string PortraitObjectLeft = "PortraitLeft";
        private const string PortraitObjectCenter = "PortraitCenter";
        private const string PortraitObjectRight = "PortraitRight";
        private const string TargetScreen = "Screen";
        private const string TargetCanvas = "Canvas";
        private const string TargetBackground = "Background";
        private const string TargetPortraitLeft = "PortraitLeft";
        private const string TargetPortraitCenter = "PortraitCenter";
        private const string TargetPortraitRight = "PortraitRight";
        private const string TargetText = "Text";

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Tooltip("フェード対象から除外するUI（テキストボックス等）。指定したCanvasGroupはフェードの影響を受けません。未設定ならテキストへ自動付与します。")]
        private CanvasGroup _nonFadingUi;
        [SerializeField] private TMP_Text _chat;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Animation _animationPlayer;
        [SerializeField] private GameObject _fadeObj;
        [SerializeField] private RectTransform _portraitRoot;
        [SerializeField] private Vector2 _portraitSize = new(700f, 1000f);

        // 対象ごとに独立してフェードできるよう、進行中フェードを対象キーで保持する。
        private readonly Dictionary<string, FadeState> _activeFades = new(System.StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _completedFadeKeys = new();

        private readonly Dictionary<string, Sprite> _backgroundByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, AnimationClip> _animationByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Sprite> _portraitByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Image> _portraitBySlot = new(System.StringComparer.OrdinalIgnoreCase);
        private ViewModel _viewModel;

        /// <summary>
        /// 表示に必要な参照を初期化する。
        /// </summary>
        private void Awake()
        {
            TryAutoAssignReferences();
            EnsureNonFadingUi();
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
        /// 破棄時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromViewModel();
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
        private void OnFadeReceived(string target, float start, float end, float duration)
        {
            // フェード直前に、テキストを除外するCanvasGroup設定を確実に適用する。
            EnsureNonFadingUi();

            CanvasGroup group = ResolveFadeTarget(target, out bool floorAlpha);
            if (group == null)
            {
                Debug.LogWarning($"ScenarioView: fade target '{target}' が見つかりません。");
                return;
            }

            string key = string.IsNullOrWhiteSpace(target) ? TargetScreen : target.Trim();

            if (duration <= 0f)
            {
                // 即時反映。進行中フェードがあれば打ち切る。
                ApplyFadeAlpha(group, end, floorAlpha);
                _activeFades.Remove(key);
                return;
            }

            _activeFades[key] = new FadeState
            {
                Group = group,
                Time = 0f,
                Start = start,
                End = end,
                Duration = duration,
                FloorAlpha = floorAlpha,
            };
            // 開始値を即時反映する。
            ApplyFadeAlpha(group, start, floorAlpha);
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

            // テキスト自体の並びを明示指定した場合を除き、テキストは最前面に保つ。
            if (!string.Equals(target, TargetText, System.StringComparison.OrdinalIgnoreCase))
            {
                EnsureTextInFront();
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

        /// <summary>
        /// 進行中の各対象フェードを 1 フレーム分更新する。
        /// </summary>
        private void Fade()
        {
            if (_activeFades.Count == 0)
            {
                return;
            }

            _completedFadeKeys.Clear();
            foreach (KeyValuePair<string, FadeState> entry in _activeFades)
            {
                FadeState fade = entry.Value;
                if (fade.Group == null)
                {
                    _completedFadeKeys.Add(entry.Key);
                    continue;
                }

                fade.Time += Time.deltaTime;
                float t = fade.Duration <= 0f ? 1f : Mathf.Clamp01(fade.Time / fade.Duration);
                ApplyFadeAlpha(fade.Group, Mathf.Lerp(fade.Start, fade.End, t), fade.FloorAlpha);
                if (t >= 1f)
                {
                    _completedFadeKeys.Add(entry.Key);
                }
            }

            for (int i = 0; i < _completedFadeKeys.Count; i++)
            {
                _activeFades.Remove(_completedFadeKeys[i]);
            }
        }

        /// <summary>
        /// フェードの alpha を対象の CanvasGroup へ反映する。
        /// 画面全体（Screen）フェードでは alpha がちょうど 0 になると配下の
        /// CanvasRenderer がカリングされ、ignoreParentGroups で除外したテキストまで
        /// 消えてしまうため、僅かな最小値でクランプして完全な 0 にはしない。
        /// 個別対象（背景・立ち絵）はテキストを含まないので 0 まで許容する。
        /// </summary>
        private void ApplyFadeAlpha(CanvasGroup group, float alpha, bool floorAlpha)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = floorAlpha ? Mathf.Max(alpha, MinCanvasAlpha) : Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// 対象文字列から、フェードを適用する CanvasGroup を解決する。
        /// テキストは対象に含めない（常にフェードしない）。
        /// </summary>
        /// <param name="target"> 対象名（Screen/Background/PortraitLeft/…）。 </param>
        /// <param name="floorAlpha"> 画面全体フェードで alpha を 0 にしないか。 </param>
        private CanvasGroup ResolveFadeTarget(string target, out bool floorAlpha)
        {
            floorAlpha = false;

            // 未指定・Screen・Canvas は画面全体（ルート CanvasGroup）。
            if (string.IsNullOrWhiteSpace(target)
                || target.Equals(TargetScreen, System.StringComparison.OrdinalIgnoreCase)
                || target.Equals(TargetCanvas, System.StringComparison.OrdinalIgnoreCase))
            {
                floorAlpha = true;
                return _canvasGroup;
            }

            if (target.Equals(TargetBackground, System.StringComparison.OrdinalIgnoreCase))
            {
                return _backgroundImage != null ? EnsureCanvasGroup(_backgroundImage.gameObject) : null;
            }

            if (target.Equals(TargetPortraitLeft, System.StringComparison.OrdinalIgnoreCase))
            {
                return EnsurePortraitCanvasGroup(SlotLeft);
            }

            if (target.Equals(TargetPortraitCenter, System.StringComparison.OrdinalIgnoreCase))
            {
                return EnsurePortraitCanvasGroup(SlotCenter);
            }

            if (target.Equals(TargetPortraitRight, System.StringComparison.OrdinalIgnoreCase))
            {
                return EnsurePortraitCanvasGroup(SlotRight);
            }

            return null;
        }

        /// <summary>
        /// 指定 GameObject に CanvasGroup を確保する。
        /// </summary>
        private static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            CanvasGroup group = go.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = go.AddComponent<CanvasGroup>();
            }

            return group;
        }

        /// <summary>
        /// 指定スロットの立ち絵に CanvasGroup を確保する。
        /// </summary>
        private CanvasGroup EnsurePortraitCanvasGroup(string slot)
        {
            EnsurePortraitSlots();
            return _portraitBySlot.TryGetValue(slot, out Image image) && image != null
                ? EnsureCanvasGroup(image.gameObject)
                : null;
        }

        /// <summary>
        /// フェード演出の内部状態を初期化する。
        /// </summary>
        private void ResetFadeState()
        {
            _activeFades.Clear();
            _completedFadeKeys.Clear();
        }

        /// <summary>
        /// フェード対象から除外するUI（テキストボックス）を確保する。
        /// CanvasGroup.ignoreParentGroups を有効にし、_canvasGroup のフェードで
        /// テキストまで一緒に消えないようにする。
        /// </summary>
        private void EnsureNonFadingUi()
        {
            if (_nonFadingUi == null && _chat != null)
            {
                // 明示指定が無い場合はテキスト自身に CanvasGroup を用意する。
                _nonFadingUi = _chat.GetComponent<CanvasGroup>();
                if (_nonFadingUi == null)
                {
                    _nonFadingUi = _chat.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (_nonFadingUi != null)
            {
                // 親（_canvasGroup）のフェードを無視して常に不透明に保つ。
                _nonFadingUi.ignoreParentGroups = true;
                _nonFadingUi.alpha = 1f;
            }
        }

        /// <summary>
        /// テキストを兄弟の最後（＝最前面）に移動する。
        /// 立ち絵は実行時に生成されテキストより後ろの兄弟になり手前に描画されるため、
        /// 常にテキストが前面に来るようにする。
        /// </summary>
        private void EnsureTextInFront()
        {
            if (_chat != null)
            {
                _chat.transform.SetAsLastSibling();
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
            // 立ち絵生成でテキストが背面へ回るため、常に最前面へ戻す。
            EnsureTextInFront();
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
        /// カタログ辞書のエントリを検証しつつ複製する。
        /// </summary>
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
        /// 未設定の参照を自動で補完する。
        /// </summary>
        private void TryAutoAssignReferences()
        {
            if (_canvasGroup == null)
            {
                // 未設定だとフェードが無反応になるため、自身または子から補完する。
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
                }
            }

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
        /// 1 つの対象に対する進行中フェードの状態。
        /// </summary>
        private sealed class FadeState
        {
            public CanvasGroup Group;
            public float Time;
            public float Start;
            public float End;
            public float Duration;
            public bool FloorAlpha;
        }
    }
}
