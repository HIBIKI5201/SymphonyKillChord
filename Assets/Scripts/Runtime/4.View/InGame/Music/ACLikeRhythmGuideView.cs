using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.PostEffect;
using KillChord.Runtime.View.InGame.Sequence;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     AC風リズムガイドのビート表示と判定ゾーンを描画するViewです。
    /// </summary>
    public sealed class ACLikeRhythmGuideView : MonoBehaviour, IGameplayControllable, IRhythmGuideBeatViewModel, IRhythmGuideTargetFeedbackViewModel
    {
        /// <summary> ガイド表示の更新タイミングを通知します。 </summary>
        public event Action OnUpdate;

        /// <summary> ゲームプレイ開始を通知します。 </summary>
        public event Action OnStartGameplay;

        /// <summary> ゲームプレイ停止を通知します。 </summary>
        public event Action OnStopGameplay;

        /// <summary> ゲージ座標の変更をスキル表示へ通知する。 </summary>
        public event Action OnLayoutChanged;

        /// <summary> 音楽同期サービスから受け取った現在のジャスト成否。 </summary>
        public bool IsOnJustTiming { get; private set; }

        /// <summary>
        ///     ゲームプレイ開始を購読側へ通知する。
        /// </summary>
        public void StartGameplay()
        {
            OnStartGameplay?.Invoke();
        }

        /// <summary>
        ///     ゲームプレイ停止を購読側へ通知する。
        /// </summary>
        public void StopGameplay()
        {
            IsOnJustTiming = false;
            OnStopGameplay?.Invoke();
        }

        /// <summary>
        ///     CanvasGroupの透明度を更新する。
        /// </summary>
        /// <param name="hasTarget"> 攻撃目標の有無。 </param>
        public void SetAlpha(bool hasTarget)
        {
            _canvasGroup.alpha = hasTarget ? _targetAlpha : _noTargetAlpha;
        }

        /// <summary>
        ///     チュートリアル等で対象となっているBeatCountを設定し、対象外ビートの表示を薄くする。
        /// </summary>
        /// <param name="targetBeatCount"> 対象のBeatCount。対象がない場合はnull。 </param>
        public void SetTargetBeatCount(int? targetBeatCount)
        {
            if (_targetBeatCount != targetBeatCount)
            {
                _targetBeatCount = targetBeatCount;
                UpdateBeatColors();
                // 現在ビートの表示色は次のブロック遷移まで更新されないため、ここで即座に反映する。
                UpdateCurrentBeatColor();
                UpdateJustOutlineColors();
                RebuildTargetBeatFrames();
            }
        }

        /// <summary>
        ///     チュートリアル対象拍の攻撃成功時に赤枠の拡縮演出を再生する。
        /// </summary>
        public void PlayTargetBeatSuccessFeedback()
        {
            if (_targetBeatFrames == null || _targetBeatFrames.Length == 0)
            {
                return;
            }

            _targetBeatFrameMotion.TryCancel();
            MotionSequenceBuilder sequence = LSequence.Create();
            Vector3 scaleStrength = Vector3.one * (TARGET_BEAT_FRAME_SCALE_MULTIPLIER - 1f);
            for (int i = 0; i < _targetBeatFrames.Length; i++)
            {
                RectTransform frame = _targetBeatFrames[i];
                frame.localScale = Vector3.one;
                sequence.Join(LMotion.Punch.Create(Vector3.one, scaleStrength, TARGET_BEAT_FRAME_MOTION_DURATION)
                    .WithEase(Ease.OutQuad)
                    .WithFrequency(1)
                    .BindToLocalScale(frame));
            }

            _targetBeatFrameMotion = sequence.Run(
                motion => motion.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }

        /// <summary>
        ///     全ビートブロックの色を現在の対象BeatCountに応じて更新する。
        /// </summary>
        private void UpdateBeatColors()
        {
            if (_leftBeatImages == null || _rightBeatImages == null || _totalBeatBoxCount <= 0) return;
            for (int i = 0; i < _totalBeatBoxCount; i++)
            {
                // 実行中のBeatAnimation(Just判定のフラッシュ等)が動いていると、
                // 直後の色代入がアニメーションのBindToColorに毎フレーム上書きされてしまうため、先に完了させる。
                if (_handles != null && i < _handles.Length)
                {
                    _handles[i].TryComplete();
                }

                Color color = GetTargetColorForIndex(i);
                _leftBeatImages[i].color = color;
                _rightBeatImages[i].color = color;
            }
        }

        /// <summary>
        ///     指定ブロックが属する判定ゾーンの色を、対象外ビートの減光を適用せずに取得する。
        ///     判定ゾーンが未構築などで解決できない場合はfalseを返す。
        /// </summary>
        /// <param name="blockIndex"> ブロックのインデックス。 </param>
        /// <param name="color"> 判定ゾーンの色。取得できない場合は既定値。 </param>
        /// <param name="zoneIndex"> 解決した判定ゾーンのインデックス。取得できない場合は-1。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        private bool TryGetZoneColor(int blockIndex, out Color color, out int zoneIndex)
        {
            color = default;
            zoneIndex = GetBeatSectionIndex(blockIndex);

            // 判定ゾーン未構築時はGetBeatSectionIndexが-1を返すため、色を解決できない状態として扱う。
            if (zoneIndex < 0 || zoneIndex >= _beatColor.Length)
            {
                zoneIndex = -1;
                return false;
            }

            color = _beatColor[zoneIndex];
            return true;
        }

        /// <summary>
        ///     ガイド表示用に、対象BeatCountと一致しない判定ゾーンの色を薄くする。
        /// </summary>
        /// <param name="color"> 減光前の色。 </param>
        /// <param name="zoneIndex"> 対象の判定ゾーンのインデックス。 </param>
        /// <returns> 減光を適用した色。 </returns>
        private Color ApplyTargetDim(Color color, int zoneIndex)
        {
            if (!_targetBeatCount.HasValue || zoneIndex < 0 || zoneIndex >= _zoneBeatCounts.Length)
            {
                return color;
            }

            // 対象外ビートを薄くする。
            if (_zoneBeatCounts[zoneIndex] != _targetBeatCount.Value)
            {
                color.a *= _dimAlpha;
            }

            return color;
        }

        /// <summary>
        ///     ガイド表示に適用するブロックの色を取得する。対象BeatCountと一致しない場合は薄くする。
        ///     判定ゾーンを解決できない場合は既定値を返す。
        /// </summary>
        /// <param name="blockIndex"> ブロックのインデックス。 </param>
        /// <returns> 適用する色。 </returns>
        private Color GetTargetColorForIndex(int blockIndex)
        {
            if (!TryGetZoneColor(blockIndex, out Color color, out int zoneIndex))
            {
                return default;
            }

            return ApplyTargetDim(color, zoneIndex);
        }

        /// <summary>
        ///     ジャスト位置の枠線を現在の対象BeatCountに応じた透明度へ更新する。
        /// </summary>
        private void UpdateJustOutlineColors()
        {
            if (_effectConfig == null || _justOutlineImages == null)
            {
                return;
            }

            for (int i = 0; i < _justOutlineImages.Length; i++)
            {
                if (_justOutlineImages[i] == null)
                {
                    continue;
                }

                _justOutlineImages[i].color = GetJustOutlineColor(_justOutlineZoneIndices[i]);
            }
        }

        /// <summary>
        ///     指定ゾーンのジャスト位置の枠線色を取得する。
        /// </summary>
        /// <param name="zoneIndex"> 対象の判定ゾーンのインデックス。 </param>
        /// <returns> 対象外の場合はゲージ色と同じ透明度を適用した枠線色。 </returns>
        private Color GetJustOutlineColor(int zoneIndex)
        {
            Color outlineColor = _effectConfig.JustOutlineColor;
            if (_beatColor == null ||
                !_targetBeatCount.HasValue ||
                zoneIndex < 0 ||
                zoneIndex >= _zoneBeatCounts.Length ||
                zoneIndex >= _beatColor.Length ||
                _zoneBeatCounts[zoneIndex] == _targetBeatCount.Value)
            {
                return outlineColor;
            }

            outlineColor.a = ApplyTargetDim(_beatColor[zoneIndex], zoneIndex).a;
            return outlineColor;
        }

        /// <summary>
        ///     判定ゾーン定義に応じてビートGUIを再構築する。
        /// </summary>
        /// <param name="zones"> 判定ゾーンの一覧。 </param>
        /// <param name="guideLengthInBars"> 判定側と共有するタイムアウト小節数。 </param>
        public void ConfigureZones(IReadOnlyList<RhythmGuideZoneDto> zones, float guideLengthInBars)
        {
            float halfWidth = CalculateHalfWidth();
            bool layoutChanged = !Mathf.Approximately(_layout.HalfWidth, halfWidth)
                || !Mathf.Approximately(_layout.LengthInBars, guideLengthInBars);
            if (!layoutChanged && !NeedsRebuild(zones))
            {
                return;
            }

            _layout = new RhythmGuideLayout(halfWidth, _beatWidth, guideLengthInBars, zones);
            CacheZones(zones);
            RebuildBeatRectTransforms();
        }

        /// <summary>
        ///    ビートの位置を更新する。
        /// </summary>
        /// <param name="normalizeOffset"> ビートの位置(1が1小節。ジャスト通過分として1を超える値も受け取る)</param>
        /// <param name="isJustTiming"> 音楽同期サービスで確定したジャスト成否。 </param>
        /// <param name="currentBeatCount"> 判定側で確定した現在の攻撃拍。 </param>
        public void SetBeatsOffset(float normalizeOffset, bool isJustTiming, int? currentBeatCount)
        {
            bool beatChanged = _currentBeatCount != currentBeatCount;
            _currentBeatCount = currentBeatCount;
            bool wasJustTiming = IsOnJustTiming;
            IsOnJustTiming = isJustTiming;
            if (_beatPositionImages == null || _beatPositionImages.Length == 0 || _totalBeatBoxCount <= 0)
            {
                return;
            }

            // 進捗、Just、色帯とアイコンは同じ実描画幅を使う。
            float gaugeNormalized = _layout.GetNormalizedProgress(normalizeOffset);

            for (int i = 0; i < _beatPositionImages.Length; i++)
            {
                _beatPositionImages[i].fillAmount = gaugeNormalized;
            }

            int activeIndex = _layout.GetBlockIndex(normalizeOffset);
            if (activeIndex == _currentOpenIndex && wasJustTiming == isJustTiming && !beatChanged)
            {
                return;
            }
            SetBeatAnimation(activeIndex, isJustTiming);
            _currentOpenIndex = activeIndex;
            UpdateCurrentBeatColor();
        }

        /// <summary>
        ///     指定した拍子（BeatTypeの整数値）に対応するジャストタイミング位置のX座標（中心からの距離。左右対称に±で使う）を取得する。
        /// </summary>
        /// <param name="beatType"> 対象の拍子（BeatTypeの整数値）。 </param>
        /// <param name="xPosition"> 中心からの距離（絶対値）。取得できない場合は0。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        public bool TryGetJustTimingXPosition(int beatType, out float xPosition)
        {
            xPosition = 0f;
            if (_totalBeatBoxCount <= 0)
            {
                return false;
            }
            for (int i = 0; i < _zoneBeatCounts.Length; i++)
            {
                if (_zoneBeatCounts[i] == beatType)
                {
                    xPosition = _layout.GetPosition((_justStarts[i] + _justEnds[i]) * 0.5f);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     指定拍のJust位置を含む同色ブロック区間の中心と幅を取得する。
        ///     アイコンのJust中心と色帯の区間中心を分け、帯が隣の拍へはみ出すのを防ぐ。
        /// </summary>
        /// <param name="beatType"> 対象拍の整数値。 </param>
        /// <param name="xPosition"> 区間中心の、ゲージ中心からの距離。 </param>
        /// <param name="width"> 同色区間の実描画幅。 </param>
        /// <returns> 対応する区間を取得できた場合はtrue。 </returns>
        public bool TryGetBeatRange(int beatType, out float xPosition, out float width)
        {
            xPosition = 0f;
            width = 0f;
            int zoneIndex = Array.IndexOf(_zoneBeatCounts, beatType);
            if (zoneIndex < 0 || _totalBeatBoxCount <= 0)
            {
                return false;
            }

            float justCenter = (_justStarts[zoneIndex] + _justEnds[zoneIndex]) * 0.5f;
            int firstBlock = _layout.GetBlockIndex(justCenter);
            if (firstBlock < 0 || GetBeatSectionIndex(firstBlock) != zoneIndex)
            {
                return false;
            }

            int lastBlock = firstBlock;
            while (firstBlock > 0 && GetBeatSectionIndex(firstBlock - 1) == zoneIndex)
            {
                firstBlock--;
            }
            while (lastBlock + 1 < _totalBeatBoxCount && GetBeatSectionIndex(lastBlock + 1) == zoneIndex)
            {
                lastBlock++;
            }

            float start = _layout.GetBlockBoundary(firstBlock);
            float end = _layout.GetBlockBoundary(lastBlock + 1);
            xPosition = (start + end) * 0.5f;
            width = end - start;
            return true;
        }

        /// <summary>
        ///    ビートのアニメーションを更新する。
        /// </summary>
        /// <param name="openIndex"> アニメーション対象のブロック番号。 </param>
        /// <param name="isJustTiming"> ジャストタイミング位置か。 </param>
        public void SetBeatAnimation(int openIndex, bool isJustTiming)
        {
            if (_handles == null
                || _leftBeatRectTransforms == null
                || _rightBeatRectTransforms == null
                || _leftBeatImages == null
                || _rightBeatImages == null
                || openIndex < 0
                || openIndex >= _handles.Length
                || _effectConfig == null)
            {
                return;
            }

            _handles[openIndex].TryComplete();
            Color beatColor = GetTargetColorForIndex(openIndex);

            if (isJustTiming)
            {
                _handles[openIndex] = CreateJustTimingMotion(openIndex, beatColor);
                return;
            }

            // ジャストタイミングは上で処理済みのため、ここは常に通常タイミングの縮小モーション。
            Ease ease = _effectConfig.NormalTimingEase;
            _handles[openIndex] = CreateNormalTimingMotion(openIndex, _inTimingSizeDelta, ease);
        }

        /// <summary>
        ///     入力時に確定した拍種のビート色を取得する。
        ///     全画面演出へ渡す色のため、ガイド表示上の減光は適用しない。
        ///     減光はガイド上で対象ビートを強調するための表現であり、演出の明るさまで変えると
        ///     対象外ビートの入力だけ演出が弱くなってしまう。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した拍種の整数値。 </param>
        /// <param name="color"> ビートブロックの色。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        public bool TryGetBeatColor(int beatCount, out Color color)
        {
            color = default;

            if (_beatColor == null || _beatColor.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < _zoneBeatCounts.Length && i < _beatColor.Length; i++)
            {
                if (_zoneBeatCounts[i] == beatCount)
                {
                    color = _beatColor[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     現在のビート色を表示用Imageへ反映する。
        /// </summary>
        private void UpdateCurrentBeatColor()
        {
            if (_currentBeatColorImages == null || _currentBeatColorImages.Length == 0)
            {
                return;
            }

            // 境界上や初回入力の色も判定側の値を使い、ブロックの丸めで変えない。
            int zoneIndex = _currentBeatCount.HasValue
                ? Array.IndexOf(_zoneBeatCounts, _currentBeatCount.Value) : -1;
            if (zoneIndex < 0 || _beatColor == null || zoneIndex >= _beatColor.Length)
            {
                return;
            }

            Color color = ApplyTargetDim(_beatColor[zoneIndex], zoneIndex);

            for (int i = 0; i < _currentBeatColorImages.Length; i++)
            {
                if (_currentBeatColorImages[i] != null)
                {
                    _currentBeatColorImages[i].color = color;
                }
            }
        }

        /// <summary> ジャスト位置の枠線1つを構成する線の本数。上下左右の4本。 </summary>
        private const int OUTLINE_LINE_COUNT = 4;

        /// <summary>
        ///     ジャスト帯の幅（小節単位）。判定幅に関係なく、この幅で描く。
        ///     値は従来の見た目（ジャスト判定幅の1/32小節）に合わせている。
        /// </summary>
        private const float JUST_BAND_WIDTH_IN_BARS = 0.03125f;

        /// <summary> ジャスト位置の枠線の最小の太さ。 </summary>
        private const float MIN_OUTLINE_THICKNESS = 0.1f;

        /// <summary> チュートリアル対象枠の線幅。 </summary>
        private const float TARGET_BEAT_FRAME_THICKNESS = 2f;

        /// <summary> チュートリアル対象枠の水平方向余白。 </summary>
        private const float TARGET_BEAT_FRAME_HORIZONTAL_PADDING = 2f;

        /// <summary> チュートリアル対象枠の垂直方向余白。 </summary>
        private const float TARGET_BEAT_FRAME_VERTICAL_PADDING = 8f;

        /// <summary> 対象拍成功時の枠拡大倍率。 </summary>
        private const float TARGET_BEAT_FRAME_SCALE_MULTIPLIER = 1.3f;

        /// <summary> 対象拍成功時の枠拡縮時間。 </summary>
        private const float TARGET_BEAT_FRAME_MOTION_DURATION = 0.2f;

        /// <summary> チュートリアル対象枠の色。 </summary>
        private static readonly Color TARGET_BEAT_FRAME_COLOR = Color.red;

        [Space]

        [SerializeField, Tooltip("ジャストタイミング演出の設定。")]
        private ACLikeRhythmGuideEffectConfig _effectConfig;

        [Tooltip("ビートの色。判定ゾーンの順番に対応します。")]
        [SerializeField] private Color[] _beatColor;

        [Tooltip("ビートの幅")]
        [SerializeField] private float _beatWidth;

        [SerializeField, Min(0f), Tooltip("基準解像度で安全領域の左右に確保する余白。")]
        private float _screenEdgeMargin = 100f;

        [Space]
        [Tooltip("ビート位置を表示するImage")]
        [SerializeField] private Image[] _beatPositionImages;
        
        [Space]
        [Tooltip("現在のビート色を表示するImage")]
        [SerializeField]
        private Image[] _currentBeatColorImages;

        [Space]

        [Tooltip("ビートのAlphaを決めるためのCanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("スキル強調より前面に赤枠を描画する、リズムガイドと同じ座標基準の親。")]
        private RectTransform _targetBeatFrameRoot;

        [Tooltip("ターゲット時の透明度。")]
        [Range(0f, 1f)]
        [SerializeField] private float _targetAlpha;
        [Tooltip("非ターゲット時の透明度。")]
        [Range(0f, 1f)]
        [SerializeField] private float _noTargetAlpha;

        [Tooltip("チュートリアル中、ミッション対象外ビートの透明度倍率。")]
        [Range(0f, 1f)]
        [SerializeField] private float _dimAlpha = 0.3f;

        [Space]
        [Tooltip("ジャストタイミング内にあるビートのSizeDelta")]
        [SerializeField] private float _justTimingSizeDelta;
        [Tooltip("タイミング内にあるビートのSizeDelta")]
        [SerializeField] private float _inTimingSizeDelta;
        [Tooltip("タイミング外にあるビートのSizeDelta")]
        [SerializeField] private float _outTimingSizeDelta;
        [Tooltip("ビートのアニメーションのDuration")]
        [SerializeField] private float _outTimingDuration;

        private RectTransform[] _leftBeatRectTransforms;
        private Image[] _leftBeatImages;
        private RectTransform[] _rightBeatRectTransforms;
        private Image[] _rightBeatImages;
        private Image[] _justOutlineImages = Array.Empty<Image>();
        private int[] _justOutlineZoneIndices = Array.Empty<int>();
        private MotionHandle[] _handles;
        private RectTransform[] _targetBeatFrames = Array.Empty<RectTransform>();
        private MotionHandle _targetBeatFrameMotion;
        private int _totalBeatBoxCount;
        private int _currentOpenIndex = -1;
        private float[] _zoneStarts = Array.Empty<float>();
        private float[] _zoneEnds = Array.Empty<float>();
        private float[] _justStarts = Array.Empty<float>();
        private float[] _justEnds = Array.Empty<float>();
        private int[] _zoneBeatCounts = Array.Empty<int>();
        private int? _targetBeatCount;
        private int? _currentBeatCount;
        private RhythmGuideLayout _layout;
        private Canvas _rootCanvas;

        /// <summary>
        ///     演出設定の設定漏れを検知し、ビートGUIを構築する。
        /// </summary>
        private void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            _rootCanvas = canvas != null ? canvas.rootCanvas : null;
            if (_targetBeatFrameRoot == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideView)}] チュートリアル対象枠の描画先が未設定です。", this);
            }
            if (_effectConfig == null)
            {
                Debug.LogWarning($"[{nameof(ACLikeRhythmGuideView)}] ジャストタイミング演出設定が未設定です。", this);
            }

            RebuildBeatRectTransforms();
        }

        /// <summary>
        ///     毎フレームの更新タイミングを購読側（ViewModel）へ通知する。
        /// </summary>
        private void Update()
        {
            OnUpdate?.Invoke();
        }

        /// <summary>
        ///     破棄時にイベントと生成した演出リソースを解放する。
        /// </summary>
        private void OnDestroy()
        {
            OnUpdate = null;
            OnStartGameplay = null;
            OnStopGameplay = null;
            OnLayoutChanged = null;

            ClearTargetBeatFrames();
            if (_handles != null)
            {
                for (int i = 0; i < _handles.Length; i++)
                {
                    _handles[i].TryCancel();
                }
            }
        }

        /// <summary>
        ///     生成済みのビートオブジェクトを破棄し、現在の判定ゾーン定義でビートGUIを作り直す。
        /// </summary>
        [ContextMenu("ビートの位置を初期化")]
        private void RebuildBeatRectTransforms()
        {
            ClearGeneratedBeatObjects();
            InitBeatGUI(
                _canvasGroup.gameObject,
                _outTimingSizeDelta,
                out _totalBeatBoxCount,
                out _leftBeatImages,
                out _rightBeatImages,
                out _leftBeatRectTransforms,
                out _rightBeatRectTransforms,
                out _handles);

            CreateJustOutlines();
            RebuildTargetBeatFrames();
            _currentOpenIndex = -1;
            SynchronizeProgressImageWidths();
            OnLayoutChanged?.Invoke();
        }

        /// <summary>
        ///     画面安全領域から左右対称に確保できる描画幅を、ゲージのローカル座標へ変換する。
        /// </summary>
        private float CalculateHalfWidth()
        {
            RectTransform guideRect = (RectTransform)transform;
            UnityEngine.Camera canvasCamera = _rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _rootCanvas.worldCamera : null;
            float scaleFactor = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;
            Rect safeArea = Screen.safeArea;
            float margin = _screenEdgeMargin * scaleFactor;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCamera, guideRect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                guideRect, new Vector2(safeArea.xMin + margin, center.y), canvasCamera, out Vector2 left);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                guideRect, new Vector2(safeArea.xMax - margin, center.y), canvasCamera, out Vector2 right);
            return Mathf.Max(0f, Mathf.Min(-left.x, right.x));
        }

        /// <summary>
        ///     白枠・黒枠・現在色の進捗Imageをブロックと同じ中心と実幅へ揃える。
        /// </summary>
        private void SynchronizeProgressImageWidths()
        {
            if (_beatPositionImages == null)
            {
                return;
            }
            foreach (Image progressImage in _beatPositionImages)
            {
                if (progressImage == null)
                {
                    continue;
                }
                RectTransform progressRect = progressImage.rectTransform;
                Vector2 position = progressRect.anchoredPosition;
                position.x = 0f;
                progressRect.anchoredPosition = position;
                progressRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _layout.HalfWidth);
            }
        }

        /// <summary>
        ///     生成済みビートオブジェクトを破棄する。
        /// </summary>
        private void ClearGeneratedBeatObjects()
        {
            ClearTargetBeatFrames();
            if (_handles != null)
            {
                for (int i = 0; i < _handles.Length; i++)
                {
                    _handles[i].TryCancel();
                }
            }

            // 枠線はブロックの子のため、ブロックの破棄に付随して消える。参照だけを捨てる。
            _justOutlineImages = Array.Empty<Image>();
            _justOutlineZoneIndices = Array.Empty<int>();

            if (_leftBeatRectTransforms != null)
            {
                for (int i = 0; i < _leftBeatRectTransforms.Length; i++)
                {
                    if (_leftBeatRectTransforms[i] != null)
                    {
                        _leftBeatRectTransforms[i].gameObject.SetActive(false);
                        Destroy(_leftBeatRectTransforms[i].gameObject);
                    }
                }
            }

            if (_rightBeatRectTransforms != null)
            {
                for (int i = 0; i < _rightBeatRectTransforms.Length; i++)
                {
                    if (_rightBeatRectTransforms[i] != null)
                    {
                        _rightBeatRectTransforms[i].gameObject.SetActive(false);
                        Destroy(_rightBeatRectTransforms[i].gameObject);
                    }
                }
            }
        }

        /// <summary>
        ///     通常タイミングの縮小モーションを生成する。
        /// </summary>
        /// <param name="index"> 対象ブロック番号。 </param>
        /// <param name="targetSizeDelta"> モーション開始時の高さ。 </param>
        /// <param name="ease"> 縮小イージング。 </param>
        /// <returns> 生成したモーションのハンドル。 </returns>
        private MotionHandle CreateNormalTimingMotion(int index, float targetSizeDelta, Ease ease)
        {
            return LSequence.Create()
                .Append(LMotion.Create(targetSizeDelta, _outTimingSizeDelta, _outTimingDuration)
                    .WithEase(ease)
                    .BindToSizeDeltaY(_leftBeatRectTransforms[index]))
                .Join(LMotion.Create(targetSizeDelta, _outTimingSizeDelta, _outTimingDuration)
                    .WithEase(ease)
                    .BindToSizeDeltaY(_rightBeatRectTransforms[index]))
                .Run(sequence => sequence.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }

        /// <summary>
        ///     ジャストタイミング専用のオーバーシュートと色フラッシュを生成する。
        /// </summary>
        /// <param name="index"> 対象ブロック番号。 </param>
        /// <param name="beatColor"> ブロックの通常色。 </param>
        /// <returns> 生成したモーションのハンドル。 </returns>
        private MotionHandle CreateJustTimingMotion(int index, Color beatColor)
        {
            // 各演出の長さは 0 にならないよう下限を設ける。
            float overshootSizeDelta = _justTimingSizeDelta + Mathf.Max(0f, _effectConfig.JustOvershootAmount);
            float overshootDuration = Mathf.Max(0.01f, _effectConfig.JustOvershootDuration);
            float returnDuration = Mathf.Max(0.01f, _effectConfig.JustReturnDuration);
            float flashDuration = Mathf.Max(0.01f, _effectConfig.FlashDuration);

            // 左右の枠を一度大きく伸ばしながら色をフラッシュさせ、その後で通常の大きさへ戻す。
            return LSequence.Create()
                .Append(LMotion.Create(_justTimingSizeDelta, overshootSizeDelta, overshootDuration)
                    .WithEase(_effectConfig.JustOvershootEase)
                    .BindToSizeDeltaY(_leftBeatRectTransforms[index]))
                .Join(LMotion.Create(_justTimingSizeDelta, overshootSizeDelta, overshootDuration)
                    .WithEase(_effectConfig.JustOvershootEase)
                    .BindToSizeDeltaY(_rightBeatRectTransforms[index]))
                .Join(LMotion.Create(_effectConfig.FlashColor, beatColor, flashDuration)
                    .WithEase(_effectConfig.FlashEase)
                    .BindToColor(_leftBeatImages[index]))
                .Join(LMotion.Create(_effectConfig.FlashColor, beatColor, flashDuration)
                    .WithEase(_effectConfig.FlashEase)
                    .BindToColor(_rightBeatImages[index]))
                .Append(LMotion.Create(overshootSizeDelta, _outTimingSizeDelta, returnDuration)
                    .WithEase(_effectConfig.JustReturnEase)
                    .BindToSizeDeltaY(_leftBeatRectTransforms[index]))
                .Join(LMotion.Create(overshootSizeDelta, _outTimingSizeDelta, returnDuration)
                    .WithEase(_effectConfig.JustReturnEase)
                    .BindToSizeDeltaY(_rightBeatRectTransforms[index]))
                .Run(sequence => sequence.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }

        /// <summary>
        ///     ジャストタイミング位置のブロックへ枠線を生成する。
        ///     枠線はブロックの子として生成するため、高さのアニメーションへ自動的に追従する。
        /// </summary>
        private void CreateJustOutlines()
        {
            if (_effectConfig == null
                || _totalBeatBoxCount <= 0
                || _leftBeatRectTransforms == null
                || _rightBeatRectTransforms == null)
            {
                _justOutlineImages = Array.Empty<Image>();
                _justOutlineZoneIndices = Array.Empty<int>();
                return;
            }

            // 判定ゾーンごとに1ブロック、それを左右のガイド分で2倍の枠線を生成する。
            int lineCount = _zoneBeatCounts.Length * OUTLINE_LINE_COUNT * 2;
            _justOutlineImages = new Image[lineCount];
            _justOutlineZoneIndices = new int[lineCount];

            int writeIndex = 0;
            for (int i = 0; i < _zoneBeatCounts.Length; i++)
            {
                // 共通判定定義のジャスト範囲の中央が乗るブロックを枠線の対象にする。
                // 範囲の端で求めると浮動小数の誤差で隣のブロックへずれるため中央で求める。
                float justCenter = (_justStarts[i] + _justEnds[i]) * 0.5f;
                int blockIndex = _layout.GetBlockIndex(justCenter);
                if (blockIndex < 0 || blockIndex >= _leftBeatRectTransforms.Length)
                {
                    continue;
                }

                // 帯の太さは判定幅に依存させず、ジャスト範囲の中央を中心に固定幅で描く。
                int zoneIndex = GetBeatSectionIndex(blockIndex);
                float bandWidth = _layout.LengthInBars > 0f
                    ? _layout.HalfWidth * JUST_BAND_WIDTH_IN_BARS / _layout.LengthInBars
                    : 0f;
                float centerOffset = _layout.GetPosition(justCenter) - _layout.GetBlockBoundary(blockIndex);
                RectTransform leftBand = CreateJustBand(
                    _leftBeatRectTransforms[blockIndex], $"JustBand_Left_{i}", 1f, -centerOffset, bandWidth);
                RectTransform rightBand = CreateJustBand(
                    _rightBeatRectTransforms[blockIndex], $"JustBand_Right_{i}", 0f, centerOffset, bandWidth);
                writeIndex = CreateJustOutline(leftBand, $"JustOutline_Left_{i}", zoneIndex, writeIndex);
                writeIndex = CreateJustOutline(rightBand, $"JustOutline_Right_{i}", zoneIndex, writeIndex);
            }

            // ブロック番号を解決できず生成を飛ばした分の空きを詰める。
            if (writeIndex < lineCount)
            {
                Array.Resize(ref _justOutlineImages, writeIndex);
                Array.Resize(ref _justOutlineZoneIndices, writeIndex);
            }
        }

        /// <summary>
        ///     ジャスト帯の枠線を載せる、固定幅の入れ物を生成する。
        ///     縦方向はブロックへストレッチさせ、高さのアニメーションへ追従させる。
        /// </summary>
        /// <param name="block"> 帯を付けるブロック。 </param>
        /// <param name="objectName"> 生成するオブジェクト名。 </param>
        /// <param name="innerEdgeAnchorX"> ブロックの中心側の端を表すアンカーX。左側は1、右側は0。 </param>
        /// <param name="centerOffset"> 中心側の端から帯の中心までの距離。左側は負の値。 </param>
        /// <param name="bandWidth"> 帯の幅。 </param>
        /// <returns> 生成した入れ物のRectTransform。 </returns>
        private static RectTransform CreateJustBand(
            RectTransform block, string objectName, float innerEdgeAnchorX, float centerOffset, float bandWidth)
        {
            GameObject bandObject = new GameObject(objectName, typeof(RectTransform));
            bandObject.layer = block.gameObject.layer;
            bandObject.transform.SetParent(block, false);

            RectTransform bandRectTransform = bandObject.GetComponent<RectTransform>();
            bandRectTransform.anchorMin = new Vector2(innerEdgeAnchorX, 0f);
            bandRectTransform.anchorMax = new Vector2(innerEdgeAnchorX, 1f);
            bandRectTransform.pivot = new Vector2(0.5f, 0.5f);
            bandRectTransform.anchoredPosition = new Vector2(centerOffset, 0f);
            bandRectTransform.sizeDelta = new Vector2(bandWidth, 0f);
            return bandRectTransform;
        }

        /// <summary>
        ///     ジャスト帯1つ分の枠線を上下左右の4本で生成する。
        ///     左右の線は帯の内側へ描き、上下の線だけ外へ張り出す。
        ///     上と下の張り出し量は個別に設定できる。
        /// </summary>
        /// <param name="parent"> 枠線を付けるジャスト帯。 </param>
        /// <param name="objectName"> 生成するオブジェクト名の接頭辞。 </param>
        /// <param name="zoneIndex"> ブロックが属する判定ゾーンのインデックス。 </param>
        /// <param name="writeIndex"> 生成した枠線を書き込む位置。 </param>
        /// <returns> 次に書き込む位置。 </returns>
        private int CreateJustOutline(RectTransform parent, string objectName, int zoneIndex, int writeIndex)
        {
            float thickness = Mathf.Max(MIN_OUTLINE_THICKNESS, _effectConfig.JustOutlineThickness);
            float upperExtend = Mathf.Max(0f, _effectConfig.JustOutlineUpperExtend);
            float lowerExtend = Mathf.Max(0f, _effectConfig.JustOutlineLowerExtend);
            Color color = GetJustOutlineColor(zoneIndex);

            // 縦の線は上下の張り出しを足した高さになり、上下で張り出し量が違う分だけ中心がずれる。
            float verticalLineSizeDelta = upperExtend + lowerExtend;
            float verticalLineOffset = (upperExtend - lowerExtend) * 0.5f;

            // 上辺。ブロック上端からupperExtendだけ外に出した位置へ、横いっぱいの線を引く。
            Image topLine = CreateJustOutlineLine(
                parent, $"{objectName}_Top", color,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, upperExtend), new Vector2(0f, thickness));

            // 下辺。ブロック下端からlowerExtendだけ外に出した位置へ引く。
            Image bottomLine = CreateJustOutlineLine(
                parent, $"{objectName}_Bottom", color,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, -lowerExtend), new Vector2(0f, thickness));

            // 左辺。縦方向はストレッチアンカーで親へ追従させ、上辺と下辺の間をつなぐ高さにする。
            Image leftLine = CreateJustOutlineLine(
                parent, $"{objectName}_Left", color,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                new Vector2(0f, verticalLineOffset), new Vector2(thickness, verticalLineSizeDelta));

            // 右辺。左辺と左右対称。
            Image rightLine = CreateJustOutlineLine(
                parent, $"{objectName}_Right", color,
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                new Vector2(0f, verticalLineOffset), new Vector2(thickness, verticalLineSizeDelta));

            Image[] lines = { topLine, bottomLine, leftLine, rightLine };
            for (int i = 0; i < lines.Length; i++)
            {
                _justOutlineImages[writeIndex] = lines[i];
                _justOutlineZoneIndices[writeIndex] = zoneIndex;
                writeIndex++;
            }

            return writeIndex;
        }

        /// <summary>
        ///     枠線を構成する線を1本生成する。
        /// </summary>
        /// <param name="parent"> 線を付けるブロック。 </param>
        /// <param name="objectName"> 生成するオブジェクト名。 </param>
        /// <param name="color"> 線の色。 </param>
        /// <param name="anchorMin"> アンカーの最小値。 </param>
        /// <param name="anchorMax"> アンカーの最大値。 </param>
        /// <param name="pivot"> ピボット。 </param>
        /// <param name="anchoredPosition"> アンカーからの位置。 </param>
        /// <param name="sizeDelta"> アンカー基準の大きさ。 </param>
        /// <returns> 生成した線のImage。 </returns>
        private Image CreateJustOutlineLine(
            RectTransform parent,
            string objectName,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject lineObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            lineObject.layer = parent.gameObject.layer;
            lineObject.transform.SetParent(parent, false);

            RectTransform lineRectTransform = lineObject.GetComponent<RectTransform>();
            lineRectTransform.anchorMin = anchorMin;
            lineRectTransform.anchorMax = anchorMax;
            lineRectTransform.pivot = pivot;
            lineRectTransform.anchoredPosition = anchoredPosition;
            lineRectTransform.sizeDelta = sizeDelta;

            Image lineImage = lineObject.GetComponent<Image>();
            lineImage.color = color;
            lineImage.raycastTarget = false;
            return lineImage;
        }

        /// <summary>
        ///     対象拍と同色の連続ブロックを囲み、中央で接する左右の範囲は一つの赤枠にする。
        ///     描画済みブロックと同じ境界を使い、小節末尾より先の表示範囲も含める。
        /// </summary>
        private void RebuildTargetBeatFrames()
        {
            ClearTargetBeatFrames();
            if (!_targetBeatCount.HasValue || _totalBeatBoxCount <= 0 || _canvasGroup == null || _targetBeatFrameRoot == null)
            {
                return;
            }

            var frames = new List<RectTransform>();
            for (int blockIndex = 0; blockIndex < _totalBeatBoxCount; blockIndex++)
            {
                if (!TryGetZoneColor(blockIndex, out Color color, out int zoneIndex) ||
                    _zoneBeatCounts[zoneIndex] != _targetBeatCount.Value)
                {
                    continue;
                }

                // ゾーン境界を別計算せず、色付けと同じ解決方法で連続区間をまとめる。
                int firstBlockIndex = blockIndex;
                while (blockIndex + 1 < _totalBeatBoxCount &&
                    TryGetZoneColor(blockIndex + 1, out Color nextColor, out int nextZoneIndex) &&
                    _zoneBeatCounts[nextZoneIndex] == _targetBeatCount.Value &&
                    nextColor == color)
                {
                    blockIndex++;
                }

                float start = _layout.GetBlockBoundary(firstBlockIndex);
                float end = _layout.GetBlockBoundary(blockIndex + 1);
                if (firstBlockIndex == 0)
                {
                    frames.Add(CreateTargetBeatFrame(
                        "TargetBeatFrame_Center",
                        Vector2.zero,
                        end * 2f));
                    continue;
                }

                float horizontalPosition = (start + end) * 0.5f;
                float width = end - start;
                frames.Add(CreateTargetBeatFrame(
                    $"TargetBeatFrame_Left_{firstBlockIndex}",
                    Vector2.left * horizontalPosition,
                    width));
                frames.Add(CreateTargetBeatFrame(
                    $"TargetBeatFrame_Right_{firstBlockIndex}",
                    Vector2.right * horizontalPosition,
                    width));
            }

            _targetBeatFrames = frames.ToArray();
        }

        /// <summary>
        ///     生成済みのチュートリアル対象枠と再生中の拡縮演出を破棄する。
        /// </summary>
        private void ClearTargetBeatFrames()
        {
            _targetBeatFrameMotion.TryCancel();
            if (_targetBeatFrames != null)
            {
                for (int i = 0; i < _targetBeatFrames.Length; i++)
                {
                    if (_targetBeatFrames[i] != null)
                    {
                        _targetBeatFrames[i].gameObject.SetActive(false);
                        Destroy(_targetBeatFrames[i].gameObject);
                    }
                }
            }

            _targetBeatFrames = Array.Empty<RectTransform>();
        }

        /// <summary>
        ///     指定ゾーンを囲う赤枠を生成し、既存ゲージとスキル強調より前面へ配置する。
        /// </summary>
        /// <param name="objectName"> 生成するオブジェクト名。 </param>
        /// <param name="anchoredPosition"> 対象ゾーン中央の位置。 </param>
        /// <param name="width"> 対象ゾーンの幅。 </param>
        /// <returns> 生成した赤枠のRectTransform。 </returns>
        private RectTransform CreateTargetBeatFrame(string objectName, Vector2 anchoredPosition, float width)
        {
            // ガイドの子として枠のオブジェクトを作り、中央基準で配置する。
            GameObject frameObject = new GameObject(objectName, typeof(RectTransform));
            frameObject.layer = gameObject.layer;
            frameObject.transform.SetParent(_targetBeatFrameRoot, false);
            frameObject.transform.SetAsLastSibling();

            RectTransform frameRectTransform = frameObject.GetComponent<RectTransform>();
            frameRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            frameRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            frameRectTransform.pivot = new Vector2(0.5f, 0.5f);
            frameRectTransform.anchoredPosition = anchoredPosition;
            // 枠の大きさは、対象の幅と高さに余白を足した大きさにする。
            float frameWidth = Mathf.Max(
                TARGET_BEAT_FRAME_THICKNESS,
                width + TARGET_BEAT_FRAME_HORIZONTAL_PADDING * 2f);
            float frameHeight = Mathf.Max(
                TARGET_BEAT_FRAME_THICKNESS,
                _outTimingSizeDelta + TARGET_BEAT_FRAME_VERTICAL_PADDING * 2f);
            frameRectTransform.sizeDelta = new Vector2(frameWidth, frameHeight);

            // 上下左右の辺をそれぞれ作る。
            float horizontalEdgeY = (frameHeight - TARGET_BEAT_FRAME_THICKNESS) * 0.5f;
            float verticalEdgeX = (frameWidth - TARGET_BEAT_FRAME_THICKNESS) * 0.5f;
            CreateTargetBeatFrameEdge(
                frameRectTransform,
                "Top",
                Vector2.up * horizontalEdgeY,
                new Vector2(frameWidth, TARGET_BEAT_FRAME_THICKNESS));
            CreateTargetBeatFrameEdge(
                frameRectTransform,
                "Bottom",
                Vector2.down * horizontalEdgeY,
                new Vector2(frameWidth, TARGET_BEAT_FRAME_THICKNESS));
            CreateTargetBeatFrameEdge(
                frameRectTransform,
                "Left",
                Vector2.left * verticalEdgeX,
                new Vector2(TARGET_BEAT_FRAME_THICKNESS, frameHeight));
            CreateTargetBeatFrameEdge(
                frameRectTransform,
                "Right",
                Vector2.right * verticalEdgeX,
                new Vector2(TARGET_BEAT_FRAME_THICKNESS, frameHeight));
            return frameRectTransform;
        }

        /// <summary>
        ///     チュートリアル対象枠を構成する一辺を生成する。
        /// </summary>
        /// <param name="parent"> 枠の親RectTransform。 </param>
        /// <param name="edgeName"> 辺を識別する名前。 </param>
        /// <param name="anchoredPosition"> 辺の位置。 </param>
        /// <param name="sizeDelta"> 辺の大きさ。 </param>
        private void CreateTargetBeatFrameEdge(
            RectTransform parent,
            string edgeName,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject edgeObject = new GameObject(edgeName, typeof(RectTransform), typeof(Image));
            edgeObject.layer = gameObject.layer;
            edgeObject.transform.SetParent(parent, false);

            RectTransform edgeRectTransform = edgeObject.GetComponent<RectTransform>();
            edgeRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            edgeRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            edgeRectTransform.pivot = new Vector2(0.5f, 0.5f);
            edgeRectTransform.anchoredPosition = anchoredPosition;
            edgeRectTransform.sizeDelta = sizeDelta;

            Image edgeImage = edgeObject.GetComponent<Image>();
            edgeImage.color = TARGET_BEAT_FRAME_COLOR;
            edgeImage.raycastTarget = false;
        }

        /// <summary>
        ///     判定ゾーン再構築が必要か判定する。
        /// </summary>
        /// <param name="zones"> 判定ゾーンの一覧。 </param>
        /// <returns> 再構築が必要な場合はtrue。 </returns>
        private bool NeedsRebuild(IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            if (zones == null)
            {
                return false;
            }

            if (zones.Count != _zoneStarts.Length)
            {
                return true;
            }

            for (int i = 0; i < zones.Count; i++)
            {
                if (!Mathf.Approximately(_zoneStarts[i], zones[i].StartNormalized) ||
                    !Mathf.Approximately(_zoneEnds[i], zones[i].EndNormalized) ||
                    _justStarts[i] != zones[i].JustStartNormalized ||
                    _justEnds[i] != zones[i].JustEndNormalized ||
                    _zoneBeatCounts[i] != zones[i].BeatCount)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     判定ゾーン内容をキャッシュする。
        /// </summary>
        /// <param name="zones"> 判定ゾーンの一覧。 </param>
        private void CacheZones(IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            if (zones == null || zones.Count == 0)
            {
                _zoneStarts = Array.Empty<float>();
                _zoneEnds = Array.Empty<float>();
                _justStarts = Array.Empty<float>();
                _justEnds = Array.Empty<float>();
                _zoneBeatCounts = Array.Empty<int>();
                return;
            }

            _zoneStarts = new float[zones.Count];
            _zoneEnds = new float[zones.Count];
            _justStarts = new float[zones.Count];
            _justEnds = new float[zones.Count];
            _zoneBeatCounts = new int[zones.Count];

            for (int i = 0; i < zones.Count; i++)
            {
                _zoneStarts[i] = zones[i].StartNormalized;
                _zoneEnds[i] = zones[i].EndNormalized;
                _justStarts[i] = zones[i].JustStartNormalized;
                _justEnds[i] = zones[i].JustEndNormalized;
                _zoneBeatCounts[i] = zones[i].BeatCount;
            }
        }

        /// <summary>
        ///     判定ゾーン定義からスペクトラム風ビートのブロックを左右対称に生成し、
        ///     生成した表示オブジェクトを出力する。
        /// </summary>
        /// <param name="parent"> 生成したブロックの親オブジェクト。 </param>
        /// <param name="beatHeight"> 1ブロックの初期高さ。 </param>
        /// <param name="totalBeatBoxCount"> 生成したブロック総数。生成できない場合は0。 </param>
        /// <param name="leftBeatImages"> 左側ブロックのImage。 </param>
        /// <param name="rightBeatImages"> 右側ブロックのImage。 </param>
        /// <param name="leftBeatRT"> 左側ブロックのRectTransform。 </param>
        /// <param name="rightBeatRT"> 右側ブロックのRectTransform。 </param>
        /// <param name="handles"> ブロックごとのモーションハンドル。 </param>
        private void InitBeatGUI(
            in GameObject parent,
            float beatHeight,
            out int totalBeatBoxCount,
            out Image[] leftBeatImages,
            out Image[] rightBeatImages,
            out RectTransform[] leftBeatRT,
            out RectTransform[] rightBeatRT,
            out MotionHandle[] handles)
        {
            //Outの初期化
            leftBeatImages = null;
            rightBeatImages = null;
            leftBeatRT = null;
            rightBeatRT = null;
            handles = null;

            if (_zoneStarts == null || _zoneStarts.Length == 0)
            {
                totalBeatBoxCount = 0;
                return;
            }

            if (_beatColor == null || _beatColor.Length < _zoneStarts.Length)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideView)}] _beatColor の長さが判定ゾーン数より少ないです。", this);
                totalBeatBoxCount = 0;
                return;
            }

            //スペクトラム風ビートのブロック数を計算
            int beatBlockCount = _layout.BlockCount;
            totalBeatBoxCount = beatBlockCount;

            //Out配列の初期化
            leftBeatImages = new Image[beatBlockCount];
            rightBeatImages = new Image[beatBlockCount];
            leftBeatRT = new RectTransform[beatBlockCount];
            rightBeatRT = new RectTransform[beatBlockCount];
            handles = new MotionHandle[beatBlockCount];

            //スペクトラム風ビートのブロックを生成
            for (int i = 0; i < beatBlockCount; i++)
            {
                float start = _layout.GetBlockBoundary(i);
                float beatWidth = _layout.GetBlockBoundary(i + 1) - start;
                Color color = GetTargetColorForIndex(i);

                GameObject leftBeat = new GameObject($"LeftBeat_{i}", typeof(RectTransform), typeof(Image));
                leftBeat.transform.SetParent(parent.transform, false);
                RectTransform leftRT = leftBeat.GetComponent<RectTransform>();
                Image leftImage = leftBeat.GetComponent<Image>();
                leftImage.color = color; //ビートの色を設定
                leftRT.anchoredPosition = Vector2.left * start;
                leftRT.sizeDelta = new Vector2(beatWidth, beatHeight);
                leftRT.pivot = new Vector2(1f, 0.5f);
                leftBeatImages[i] = leftImage;
                leftBeatRT[i] = leftRT;

                GameObject rightBeat = new GameObject($"RightBeat_{i}", typeof(RectTransform), typeof(Image));
                rightBeat.transform.SetParent(parent.transform, false);
                RectTransform rightRT = rightBeat.GetComponent<RectTransform>();
                Image rightImage = rightBeat.GetComponent<Image>();
                rightImage.color = color; //ビートの色を設定
                rightRT.anchoredPosition = Vector2.right * start;
                rightRT.sizeDelta = new Vector2(beatWidth, beatHeight);
                rightRT.pivot = new Vector2(0f, 0.5f);
                rightBeatImages[i] = rightImage;
                rightBeatRT[i] = rightRT;
            }
        }

        /// <summary>
        ///     ブロックインデックスがどの判定ゾーンに属するかを返す。
        /// </summary>
        /// <param name="blockIndex"> ブロックのインデックス。 </param>
        /// <returns> 属する判定ゾーンのインデックス。 </returns>
        private int GetBeatSectionIndex(int blockIndex)
        {
            float position = _layout.GetBlockBarProgress(blockIndex);

            // 判定側と同じくJustを先に解決する。全境界で分割済みなのでブロック内の拍は一定。
            for (int i = 0; i < _justStarts.Length; i++)
            {
                if (position >= _justStarts[i] && position < _justEnds[i])
                {
                    return i;
                }
            }
            // 通常判定は1小節末尾へクランプし、最後の色をタイムアウト端まで延長する。
            position = Mathf.Clamp01(position);
            for (int i = 0; i < _zoneStarts.Length; i++)
            {
                float start = _zoneStarts[i];
                float end = _zoneEnds[i];

                if (position >= start && position <= end)
                {
                    return i;
                }
            }

            return _zoneStarts.Length - 1;
        }
    }
}
