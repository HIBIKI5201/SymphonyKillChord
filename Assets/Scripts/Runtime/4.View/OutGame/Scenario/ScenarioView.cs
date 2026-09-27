using KillChord.Runtime.Adaptor.OutGame.Scenario;
using LitMotion;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    /// シナリオの表示状態を Unity UI に反映するビュー。
    /// </summary>
    [DefaultExecutionOrder(10)]
    public class ScenarioView : MonoBehaviour
    {
        /// <summary>
        /// 依存先を受け取りシナリオ表示を初期化する。
        /// </summary>
        public void Initialize(
            ScenarioViewModel viewModel,
            IReadOnlyDictionary<string, Sprite> backgroundByKey,
            IReadOnlyDictionary<string, AnimationClip> animationByKey,
            IReadOnlyDictionary<string, Sprite> portraitByKey,
            IReadOnlyList<string> layerBackToFront)
        {
            _layerBackToFront = layerBackToFront;
            TryAutoAssignReferences();
            EnsureNonFadingUi();
            UnsubscribeFromViewModel();
            _viewModel = viewModel;
            SubscribeToViewModel();
            BuildCatalogMaps(backgroundByKey, animationByKey, portraitByKey);
            EnsurePortraitSlots();
            CaptureDefaultDisplayState();
            CapturePortraitBaseColors();
            ResetDisplayState();
        }

        /// <summary>
        /// 新しいシナリオ再生に必要な表示状態を準備する。
        /// </summary>
        public void PrepareForPlayback()
        {
            TryAutoAssignReferences();
            EnsureNonFadingUi();
            EnsurePortraitSlots();
            CapturePortraitBaseColors();
            CancelAllFadeMotions();
            ResetDisplayState();

            _viewModel?.ClearText();
        }

        /// <summary>
        /// 進行中の表示処理を有限に終端し、シナリオ表示を無効化する。
        /// </summary>
        public void EndPlayback()
        {
            CancelAllFadeMotions();
            _viewModel?.ClearText();
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        // CanvasGroup.alpha が 0 だと配下がカリングされ、ignoreParentGroups の
        // テキストも消える。実質不可視だがカリングは避けられる最小値。
        private const float MinCanvasAlpha = 0.004f;
        // 黒フェードが「完全に暗転した」とみなす alpha のしきい値。
        private const float FullyOpaqueThreshold = 0.99f;
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
        private const string BlackOverlayObject = "BlackOverlay";
        private const string LayerPortrait = "Portrait";
        private const string LayerEffect = "Effect";

        // 並び順が未指定のときに使う既定の背面→前面順（レイヤー名）。
        private static readonly string[] DEFAULT_LAYER_ORDER =
        {
            TargetBackground,
            LayerPortrait,
            LayerEffect,
            TargetText,
        };

        private static readonly Vector2 PORTRAIT_LEFT_DEFAULT_POSITION = new(-420f, -120f);
        private static readonly Vector2 PORTRAIT_CENTER_DEFAULT_POSITION = new(0f, -120f);
        private static readonly Vector2 PORTRAIT_RIGHT_DEFAULT_POSITION = new(420f, -120f);

        [SerializeField, Tooltip("シナリオ画面全体をフェードさせる CanvasGroup。")] private CanvasGroup _canvasGroup;
        [SerializeField, Tooltip("フェード対象から除外するUI（テキストボックス等）。指定したCanvasGroupはフェードの影響を受けません。未設定ならテキストへ自動付与します。")]
        private CanvasGroup _nonFadingUi;
        [SerializeField, Tooltip("会話枠、話者名、本文をまとめて制御するルート。")]
        private RectTransform _dialogueRoot;
        [SerializeField, Tooltip("テキストボックスの背景・枠を表示するImage。")]
        private UnityEngine.UI.Image _textBoxImage;
        [SerializeField, Tooltip("話者名を表示するTextMeshProUGUI。")]
        private TMP_Text _speakerNameText;
        [SerializeField, Tooltip("本文を表示するテキスト。")] private TMP_Text _chat;
        [SerializeField, Tooltip("背景を表示する Image。")] private Image _backgroundImage;
        [SerializeField, Tooltip("演出アニメーションを再生する Animation。")] private Animation _animationPlayer;
        [SerializeField, Tooltip("フェード用の GameObject。現在はコードから参照されていない。")] private GameObject _fadeObj;
        [SerializeField, Tooltip("立ち絵を配置する親の RectTransform。")] private RectTransform _portraitRoot;
        [SerializeField, Tooltip("立ち絵の表示サイズ。")] private Vector2 _portraitSize = new(700f, 1000f);

        // 対象と表示チャネルごとに独立した補間を所有する。
        private readonly Dictionary<FadeChannelKey, FadeState> _fadeStates = new();
        private readonly Dictionary<ScenarioFadeTarget, Color> _portraitBaseColors = new();

        private readonly Dictionary<string, Sprite> _backgroundByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, AnimationClip> _animationByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Sprite> _portraitByKey = new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, Image> _portraitBySlot = new(System.StringComparer.OrdinalIgnoreCase);
        private ScenarioViewModel _viewModel;
        private IReadOnlyList<string> _layerBackToFront;
        private Image _blackOverlay;
        private Sprite _defaultBackgroundSprite;
        private Sprite _defaultTextBoxSprite;
        private bool _hasCapturedDefaultDisplayState;

        /// <summary>
        /// 表示に必要な参照を初期化する。
        /// </summary>
        private void Awake()
        {
            TryAutoAssignReferences();
            EnsureNonFadingUi();
            EnsurePortraitSlots();
            CaptureDefaultDisplayState();
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
        /// 再有効化時に ViewModel の最新文字状態を反映する。
        /// </summary>
        private void OnEnable()
        {
            ApplyCurrentText();
        }

        /// <summary>
        /// 破棄時に進行中のシナリオ再生を停止する。
        /// </summary>
        private void OnDestroy()
        {
            CancelAllFadeMotions();
            UnsubscribeFromViewModel();
        }

        /// <summary>
        /// 受け取ったテキストを表示へ反映する。
        /// </summary>
        private void OnTextChanged()
        {
            ApplyCurrentText();
        }

        /// <summary>
        /// ViewModel の現在値から話者名と本文を同一フレームで更新する。
        /// </summary>
        private void ApplyCurrentText()
        {
            if (_viewModel == null)
            {
                return;
            }

            if (_speakerNameText != null)
            {
                bool hasSpeaker = !string.IsNullOrEmpty(_viewModel.Speaker);
                _speakerNameText.text = _viewModel.Speaker;
                _speakerNameText.gameObject.SetActive(hasSpeaker);
            }

            if (_chat != null)
            {
                _chat.text = _viewModel.Message;
                return;
            }

            Debug.LogWarning($"[{nameof(ScenarioView)}] {nameof(_chat)} が設定されていません。", this);
        }

        /// <summary>
        /// フェード要求を受け取りアニメーション状態を更新する。
        /// </summary>
        private ValueTask OnFadeRequested(in ScenarioFadeViewDTO dto, CancellationToken cancellationToken)
        {
            var request = new FadeRequest(dto.Target, dto.Mode, dto.Start, dto.End, dto.Duration);
            return RunFadeAsync(request, cancellationToken);
        }

        /// <summary>
        /// LitMotionを所有し、終了値と終了処理の適用まで待機する。
        /// </summary>
        private async ValueTask RunFadeAsync(FadeRequest request, CancellationToken cancellationToken)
        {
            EnsureNonFadingUi();
            if (!TryGetFadeState(request.Target, request.Mode, out FadeState state))
            {
                Debug.LogWarning(
                    $"[{nameof(ScenarioView)}] Fade対象が見つかりません。Target={request.Target}, Mode={request.Mode}",
                    this);
                return;
            }

            state.Handle.TryCancel();
            state.Handle = default;
            state.Apply(request.Start);

            if (request.Duration <= 0f)
            {
                state.Apply(request.End);
                ApplyFadeCompletion(request);
                return;
            }

            MotionHandle handle = LMotion.Create(request.Start, request.End, request.Duration)
                .WithEase(Ease.Linear)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(state, static (value, target) => target.Apply(value));
            state.Handle = handle;

            try
            {
                await handle.ToValueTask(cancellationToken);
                if (!state.Handle.Equals(handle))
                {
                    return;
                }

                state.Apply(request.End);
                ApplyFadeCompletion(request);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            catch (OperationCanceledException) when (!state.Handle.Equals(handle))
            {
                // 同一チャネルの後続要求、Prepare、Endによる置換は正常な所有権移行として扱う。
            }
            finally
            {
                if (state.Handle.Equals(handle))
                {
                    state.Handle = default;
                }
            }
        }

        /// <summary>
        /// フェード完了に伴う共通終了処理を反映する。
        /// </summary>
        private void ApplyFadeCompletion(FadeRequest request)
        {
            if (request.Target == ScenarioFadeTarget.Black
                && request.Mode == ScenarioFadeMode.Alpha
                && request.End >= FullyOpaqueThreshold)
            {
                _viewModel?.ClearText();
            }
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
                Canvas canvas = GetComponent<Canvas>();
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
        /// シナリオ完了時の後処理を表示へ反映する。
        /// </summary>
        private void InputScenarioCompleted(bool skipped)
        {
            Debug.Log(skipped
                ? "シナリオ再生完了: スキップ終了。"
                : "シナリオ再生完了: 全テキスト表示終了。");
            EndPlayback();
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

            _viewModel.OnTextChanged += OnTextChanged;
            _viewModel.BindFadeRequestHandler(OnFadeRequested);
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

            _viewModel.OnTextChanged -= OnTextChanged;
            _viewModel.UnbindFadeRequestHandler(OnFadeRequested);
            _viewModel.OnBackground -= InputBackground;
            _viewModel.OnAnimation -= InputAnimation;
            _viewModel.OnPortrait -= InputPortrait;
            _viewModel.OnLayerOrder -= InputLayerOrder;
            _viewModel.OnScenarioCompleted -= InputScenarioCompleted;
            _viewModel = null;
        }

        /// <summary>
        /// フェードの alpha を対象の CanvasGroup へ反映する。
        /// 画面全体（Screen）フェードでは alpha がちょうど 0 になると配下の
        /// CanvasRenderer がカリングされ、ignoreParentGroups で除外したテキストまで
        /// 消えてしまうため、僅かな最小値でクランプして完全な 0 にはしない。
        /// 個別対象（背景・立ち絵）はテキストを含まないので 0 まで許容する。
        /// </summary>
        private static void ApplyFadeAlpha(CanvasGroup group, float alpha, bool floorAlpha)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = floorAlpha ? Mathf.Max(alpha, MinCanvasAlpha) : Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// 対象と表示チャネルに対応するフェード状態を取得する。
        /// </summary>
        private bool TryGetFadeState(
            ScenarioFadeTarget target,
            ScenarioFadeMode mode,
            out FadeState state)
        {
            // 既存の状態があれば使う。対象が無くなっていれば破棄して作り直す。
            var key = new FadeChannelKey(target, mode);
            if (_fadeStates.TryGetValue(key, out state))
            {
                if (state.IsValid)
                {
                    return true;
                }

                state.Handle.TryCancel();
                state.Handle = default;
                _fadeStates.Remove(key);
            }

            // 黒フェードは立ち絵の色を変える。元の色を記録してから状態を作る。
            if (mode == ScenarioFadeMode.Black)
            {
                Image portraitImage = ResolvePortraitImage(target);
                if (portraitImage == null)
                {
                    return false;
                }

                CapturePortraitBaseColor(target, portraitImage);
                state = FadeState.ForPortraitBlack(portraitImage, _portraitBaseColors[target]);
                _fadeStates.Add(key, state);
                return true;
            }

            // それ以外は CanvasGroup の透明度を変える。
            CanvasGroup group = ResolveAlphaFadeTarget(target, out bool floorAlpha);
            if (group == null)
            {
                return false;
            }

            state = FadeState.ForAlpha(group, floorAlpha);
            _fadeStates.Add(key, state);
            return true;
        }

        /// <summary>
        /// 型付き対象から透明度を適用する CanvasGroup を解決する。
        /// </summary>
        private CanvasGroup ResolveAlphaFadeTarget(ScenarioFadeTarget target, out bool floorAlpha)
        {
            floorAlpha = false;

            if (target == ScenarioFadeTarget.Screen)
            {
                floorAlpha = true;
                return _canvasGroup;
            }

            if (target == ScenarioFadeTarget.Background)
            {
                return _backgroundImage != null ? EnsureCanvasGroup(_backgroundImage.gameObject) : null;
            }

            if (target == ScenarioFadeTarget.PortraitLeft)
            {
                return EnsurePortraitCanvasGroup(SlotLeft);
            }

            if (target == ScenarioFadeTarget.PortraitCenter)
            {
                return EnsurePortraitCanvasGroup(SlotCenter);
            }

            if (target == ScenarioFadeTarget.PortraitRight)
            {
                return EnsurePortraitCanvasGroup(SlotRight);
            }

            if (target == ScenarioFadeTarget.Text)
            {
                EnsureNonFadingUi();
                return _nonFadingUi;
            }

            if (target == ScenarioFadeTarget.Black)
            {
                return EnsureBlackOverlay();
            }

            return null;
        }

        /// <summary>
        /// 型付き対象に対応する立ち絵Imageを取得する。
        /// </summary>
        private Image ResolvePortraitImage(ScenarioFadeTarget target)
        {
            EnsurePortraitSlots();
            string slot = target switch
            {
                ScenarioFadeTarget.PortraitLeft => SlotLeft,
                ScenarioFadeTarget.PortraitCenter => SlotCenter,
                ScenarioFadeTarget.PortraitRight => SlotRight,
                _ => null,
            };

            return slot != null && _portraitBySlot.TryGetValue(slot, out Image image) ? image : null;
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
        /// 再生開始時の表示内容、透明度、立ち絵色を初期状態へ戻す。
        /// </summary>
        private void ResetDisplayState()
        {
            ApplyAlphaValue(ScenarioFadeTarget.Screen, 1f);
            ApplyAlphaValue(ScenarioFadeTarget.Background, 1f);
            ApplyAlphaValue(ScenarioFadeTarget.Text, 1f);
            ApplyAlphaValue(ScenarioFadeTarget.PortraitLeft, 1f);
            ApplyAlphaValue(ScenarioFadeTarget.PortraitCenter, 1f);
            ApplyAlphaValue(ScenarioFadeTarget.PortraitRight, 1f);
            ApplyAlphaValue(ScenarioFadeTarget.Black, 0f);

            ApplyPortraitBlackValue(ScenarioFadeTarget.PortraitLeft, 0f);
            ApplyPortraitBlackValue(ScenarioFadeTarget.PortraitCenter, 0f);
            ApplyPortraitBlackValue(ScenarioFadeTarget.PortraitRight, 0f);

            if (_backgroundImage != null)
            {
                _backgroundImage.sprite = _defaultBackgroundSprite;
            }

            if (_textBoxImage != null)
            {
                _textBoxImage.sprite = _defaultTextBoxSprite;
            }

            ResetPortraitSlot(SlotLeft, PORTRAIT_LEFT_DEFAULT_POSITION);
            ResetPortraitSlot(SlotCenter, PORTRAIT_CENTER_DEFAULT_POSITION);
            ResetPortraitSlot(SlotRight, PORTRAIT_RIGHT_DEFAULT_POSITION);
        }

        /// <summary>
        /// Prefabまたはシーンに設定された初期表示を一度だけ記録する。
        /// </summary>
        private void CaptureDefaultDisplayState()
        {
            if (_hasCapturedDefaultDisplayState)
            {
                return;
            }

            _defaultBackgroundSprite = _backgroundImage != null ? _backgroundImage.sprite : null;
            _defaultTextBoxSprite = _textBoxImage != null ? _textBoxImage.sprite : null;
            _hasCapturedDefaultDisplayState = true;
        }

        /// <summary>
        /// 立ち絵スロットの表示内容とTransformを初期状態へ戻す。
        /// </summary>
        /// <param name="slot">初期化する立ち絵スロット。</param>
        /// <param name="defaultPosition">スロットの初期座標。</param>
        private void ResetPortraitSlot(string slot, Vector2 defaultPosition)
        {
            if (!_portraitBySlot.TryGetValue(slot, out Image portraitImage) || portraitImage == null)
            {
                return;
            }

            portraitImage.sprite = null;
            portraitImage.enabled = false;
            portraitImage.rectTransform.anchoredPosition = defaultPosition;
            portraitImage.rectTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// 対象の透明度を即時反映する。
        /// </summary>
        private void ApplyAlphaValue(ScenarioFadeTarget target, float value)
        {
            CanvasGroup group = ResolveAlphaFadeTarget(target, out bool floorAlpha);
            ApplyFadeAlpha(group, value, floorAlpha);
        }

        /// <summary>
        /// 立ち絵の基準色を維持したまま黒さを即時反映する。
        /// </summary>
        private void ApplyPortraitBlackValue(ScenarioFadeTarget target, float value)
        {
            Image image = ResolvePortraitImage(target);
            if (image == null)
            {
                return;
            }

            CapturePortraitBaseColor(target, image);
            ApplyPortraitBlack(image, _portraitBaseColors[target], value);
        }

        /// <summary>
        /// 初回確保時の立ち絵Tintを基準色として記録する。
        /// </summary>
        private void CapturePortraitBaseColors()
        {
            CapturePortraitBaseColor(ScenarioFadeTarget.PortraitLeft, ResolvePortraitImage(ScenarioFadeTarget.PortraitLeft));
            CapturePortraitBaseColor(ScenarioFadeTarget.PortraitCenter, ResolvePortraitImage(ScenarioFadeTarget.PortraitCenter));
            CapturePortraitBaseColor(ScenarioFadeTarget.PortraitRight, ResolvePortraitImage(ScenarioFadeTarget.PortraitRight));
        }

        /// <summary>
        /// 未記録の立ち絵Tintだけを基準色として保存する。
        /// </summary>
        private void CapturePortraitBaseColor(ScenarioFadeTarget target, Image image)
        {
            if (image != null && !_portraitBaseColors.ContainsKey(target))
            {
                _portraitBaseColors.Add(target, image.color);
            }
        }

        /// <summary>
        /// 立ち絵のalphaを変えずにRGBだけを黒へ補間する。
        /// </summary>
        private static void ApplyPortraitBlack(Image image, Color baseColor, float blackness)
        {
            if (image == null)
            {
                return;
            }

            float colorScale = 1f - Mathf.Clamp01(blackness);
            image.color = new Color(
                baseColor.r * colorScale,
                baseColor.g * colorScale,
                baseColor.b * colorScale,
                baseColor.a);
        }

        /// <summary>
        /// 所有する全ての補間をキャンセルする。
        /// </summary>
        private void CancelAllFadeMotions()
        {
            foreach (FadeState state in _fadeStates.Values)
            {
                state.Handle.TryCancel();
                state.Handle = default;
            }
        }

        /// <summary>
        /// フェード対象から除外するUI（テキストボックス）を確保する。
        /// CanvasGroup.ignoreParentGroups を有効にし、_canvasGroup のフェードで
        /// テキストまで一緒に消えないようにする。
        /// </summary>
        private void EnsureNonFadingUi()
        {
            if (_nonFadingUi == null && _dialogueRoot != null)
            {
                _nonFadingUi = EnsureCanvasGroup(_dialogueRoot.gameObject);
            }
            else if (_nonFadingUi == null && _chat != null)
            {
                // 移行前Prefabでも表示を失わないための互換フォールバック。
                _nonFadingUi = EnsureCanvasGroup(_chat.gameObject);
            }

            if (_nonFadingUi != null)
            {
                // 親（Screenフェード）を無視する。alpha は Text フェードでのみ変化させる。
                _nonFadingUi.ignoreParentGroups = true;
            }
        }

        /// <summary>
        /// 演出用の黒オーバーレイ（全画面）を確保し、その CanvasGroup を返す。
        /// </summary>
        private CanvasGroup EnsureBlackOverlay()
        {
            if (_blackOverlay == null)
            {
                RectTransform root = _portraitRoot != null ? _portraitRoot : transform as RectTransform;
                if (root == null)
                {
                    return null;
                }

                Transform existing = root.Find(BlackOverlayObject);
                GameObject go = existing != null
                    ? existing.gameObject
                    : new GameObject(
                        BlackOverlayObject,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image),
                        typeof(CanvasGroup));
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                if (existing == null)
                {
                    // 全画面を覆うように配置する。
                    rectTransform.SetParent(root, false);
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                }

                Image image = go.GetComponent<Image>();
                image.color = Color.black;
                image.raycastTarget = false;
                _blackOverlay = image;

                CanvasGroup overlayGroup = go.GetComponent<CanvasGroup>();
                overlayGroup.alpha = 0f;

                // 生成した演出要素を優先度順へ並べ直す。
                ApplyLayerOrder();
            }

            return EnsureCanvasGroup(_blackOverlay.gameObject);
        }

        /// <summary>
        /// UI 要素を並び順アセットの背面→前面順に並べ替える。
        /// 背面から順に SetAsLastSibling することで、末尾の要素が最前面になる。
        /// </summary>
        private void ApplyLayerOrder()
        {
            IReadOnlyList<string> order =
                _layerBackToFront != null && _layerBackToFront.Count > 0 ? _layerBackToFront : DEFAULT_LAYER_ORDER;

            foreach (string layer in order)
            {
                BringLayerToFront(layer);
            }
        }

        /// <summary>
        /// 指定レイヤーに属する要素を最前面へ移動する。
        /// </summary>
        private void BringLayerToFront(string layer)
        {
            if (string.Equals(layer, TargetBackground, System.StringComparison.OrdinalIgnoreCase))
            {
                if (_backgroundImage != null)
                {
                    _backgroundImage.transform.SetAsLastSibling();
                }
            }
            else if (string.Equals(layer, LayerPortrait, System.StringComparison.OrdinalIgnoreCase))
            {
                BringPortraitToFront(SlotLeft);
                BringPortraitToFront(SlotCenter);
                BringPortraitToFront(SlotRight);
            }
            else if (string.Equals(layer, TargetText, System.StringComparison.OrdinalIgnoreCase))
            {
                if (_dialogueRoot != null)
                {
                    _dialogueRoot.SetAsLastSibling();
                }
            }
            else if (string.Equals(layer, LayerEffect, System.StringComparison.OrdinalIgnoreCase))
            {
                if (_blackOverlay != null)
                {
                    _blackOverlay.transform.SetAsLastSibling();
                }
            }
        }

        /// <summary>
        /// 指定スロットの立ち絵を最前面へ移動する。
        /// </summary>
        private void BringPortraitToFront(string slot)
        {
            if (_portraitBySlot.TryGetValue(slot, out Image image) && image != null)
            {
                image.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 立ち絵表示に必要なスロットをそろえる。
        /// </summary>
        private void EnsurePortraitSlots()
        {
            EnsurePortraitSlot(SlotLeft, PortraitObjectLeft, PORTRAIT_LEFT_DEFAULT_POSITION);
            EnsurePortraitSlot(SlotCenter, PortraitObjectCenter, PORTRAIT_CENTER_DEFAULT_POSITION);
            EnsurePortraitSlot(SlotRight, PortraitObjectRight, PORTRAIT_RIGHT_DEFAULT_POSITION);
            ApplyPortraitSizeToExistingSlots();
            // 立ち絵生成で重なり順が変わるため、優先度順へ並べ直す。
            ApplyLayerOrder();
        }

        /// <summary>
        /// 指定スロットの表示オブジェクトを確保する。
        /// </summary>
        private void EnsurePortraitSlot(string slot, string objectName, Vector2 defaultPosition)
        {
            // すでに有効な立ち絵がある場合は何もしない。
            if (_portraitBySlot.TryGetValue(slot, out Image existingImage) && existingImage != null)
            {
                return;
            }

            // 古い登録と、スロットの元の色の記録を消す。
            _portraitBySlot.Remove(slot);
            ScenarioFadeTarget? target = slot switch
            {
                SlotLeft => ScenarioFadeTarget.PortraitLeft,
                SlotCenter => ScenarioFadeTarget.PortraitCenter,
                SlotRight => ScenarioFadeTarget.PortraitRight,
                _ => null,
            };
            if (target.HasValue)
            {
                _portraitBaseColors.Remove(target.Value);
            }

            // 立ち絵のオブジェクトを探し、無ければ下中央基準で作成する。
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

            // 大きさと表示状態を設定して登録する。
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
                return _dialogueRoot != null ? _dialogueRoot : _chat != null ? _chat.rectTransform : null;
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

            if (_dialogueRoot == null)
            {
                _dialogueRoot = transform.Find("DialogueRoot") as RectTransform;
            }

            if (_textBoxImage == null && _dialogueRoot != null)
            {
                Transform textBox = _dialogueRoot.Find("TextBoxBackground");
                _textBoxImage = textBox != null ? textBox.GetComponent<Image>() : null;
            }

            if (_speakerNameText == null && _dialogueRoot != null)
            {
                Transform speakerName = _dialogueRoot.Find("SpeakerName");
                _speakerNameText = speakerName != null ? speakerName.GetComponent<TMP_Text>() : null;
            }

            if (_chat == null && _dialogueRoot != null)
            {
                Transform bodyText = _dialogueRoot.Find("BodyText");
                _chat = bodyText != null ? bodyText.GetComponent<TMP_Text>() : null;
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
        /// async境界へ持ち越せる通常のフェード要求値。
        /// </summary>
        private readonly struct FadeRequest
        {
            /// <summary>
            ///     フェードの要求内容を生成する。
            /// </summary>
            public FadeRequest(
                ScenarioFadeTarget target,
                ScenarioFadeMode mode,
                float start,
                float end,
                float duration)
            {
                Target = target;
                Mode = mode;
                Start = start;
                End = end;
                Duration = duration;
            }

            /// <summary> フェードの対象。 </summary>
            public ScenarioFadeTarget Target { get; }
            /// <summary> フェードの種類。 </summary>
            public ScenarioFadeMode Mode { get; }
            /// <summary> 開始時の値。 </summary>
            public float Start { get; }
            /// <summary> 終了時の値。 </summary>
            public float End { get; }
            /// <summary> フェードにかける秒数。 </summary>
            public float Duration { get; }
        }

        /// <summary>
        /// フェード対象と表示チャネルの組を表す辞書キー。
        /// </summary>
        private readonly struct FadeChannelKey : IEquatable<FadeChannelKey>
        {
            /// <summary>
            ///     対象と種類を指定して生成する。
            /// </summary>
            public FadeChannelKey(ScenarioFadeTarget target, ScenarioFadeMode mode)
            {
                Target = target;
                Mode = mode;
            }

            /// <summary> フェードの対象。 </summary>
            public ScenarioFadeTarget Target { get; }
            /// <summary> フェードの種類。 </summary>
            public ScenarioFadeMode Mode { get; }

            /// <summary>
            ///     対象と種類が一致するかを判定する。
            /// </summary>
            public bool Equals(FadeChannelKey other)
            {
                return Target == other.Target && Mode == other.Mode;
            }

            /// <summary>
            ///     他のオブジェクトと値が等しいかを判定する。
            /// </summary>
            public override bool Equals(object obj)
            {
                return obj is FadeChannelKey other && Equals(other);
            }

            /// <summary>
            ///     対象と種類から算出したハッシュコードを返す。
            /// </summary>
            public override int GetHashCode()
            {
                return HashCode.Combine((int)Target, (int)Mode);
            }
        }

        /// <summary>
        /// 1つの対象・表示チャネルに対するLitMotion所有状態。
        /// </summary>
        private sealed class FadeState
        {
            /// <summary>
            ///     フェード先の CanvasGroup または立ち絵の Image を指定して生成する。
            /// </summary>
            private FadeState(
                CanvasGroup group,
                bool floorAlpha,
                Image portraitImage,
                Color portraitBaseColor)
            {
                _group = group;
                _floorAlpha = floorAlpha;
                _portraitImage = portraitImage;
                _portraitBaseColor = portraitBaseColor;
            }

            /// <summary>
            ///     CanvasGroup の透明度をフェードさせる状態を生成する。
            /// </summary>
            public static FadeState ForAlpha(CanvasGroup group, bool floorAlpha)
            {
                return new FadeState(group, floorAlpha, null, default);
            }

            /// <summary>
            ///     立ち絵を黒くフェードさせる状態を生成する。
            /// </summary>
            public static FadeState ForPortraitBlack(Image image, Color baseColor)
            {
                return new FadeState(null, false, image, baseColor);
            }

            /// <summary> フェードの対象が設定されているか。 </summary>
            public bool IsValid => _group != null || _portraitImage != null;
            /// <summary> 再生中のフェードのモーション。 </summary>
            public MotionHandle Handle { get; set; }

            /// <summary>
            ///     フェードの値を対象に反映する。
            /// </summary>
            public void Apply(float value)
            {
                if (_group != null)
                {
                    ApplyFadeAlpha(_group, value, _floorAlpha);
                    return;
                }

                ApplyPortraitBlack(_portraitImage, _portraitBaseColor, value);
            }

            private readonly CanvasGroup _group;
            private readonly bool _floorAlpha;
            private readonly Image _portraitImage;
            private readonly Color _portraitBaseColor;
        }
    }
}
