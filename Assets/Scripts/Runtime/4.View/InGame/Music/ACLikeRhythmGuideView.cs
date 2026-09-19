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
        ///     チュートリアル等で対象となっているBeatCountを設定し、対象外ビートの表示を暗くする。
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
                UpdateJustTimingMarkerColors();
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
        ///     ガイド表示用に、対象BeatCountと一致しない判定ゾーンの色を暗くする。
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

            // 対象外ビートを暗くする。
            if (_zoneBeatCounts[zoneIndex] != _targetBeatCount.Value)
            {
                color.a *= _dimAlpha;
            }

            return color;
        }

        /// <summary>
        ///     ガイド表示に適用するブロックの色を取得する。対象BeatCountと一致しない場合は暗くする。
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
        ///     ジャストタイミング表示用の帯を現在の対象BeatCountに応じた透明度へ更新する。
        /// </summary>
        private void UpdateJustTimingMarkerColors()
        {
            if (_justTimingMarkers == null || _effectConfig == null)
            {
                return;
            }

            for (int zoneIndex = 0; zoneIndex < _zoneBeatCounts.Length; zoneIndex++)
            {
                Color color = GetJustTimingMarkerColor(zoneIndex);
                for (int sideIndex = 0; sideIndex < 2; sideIndex++)
                {
                    int markerIndex = zoneIndex * 2 + sideIndex;
                    if (markerIndex < _justTimingMarkers.Length &&
                        _justTimingMarkers[markerIndex] != null &&
                        _justTimingMarkers[markerIndex].TryGetComponent(out Image markerImage))
                    {
                        markerImage.color = color;
                    }
                }
            }
        }

        /// <summary>
        ///     指定ゾーンのジャストタイミング表示用の帯色を取得する。
        /// </summary>
        /// <param name="zoneIndex"> 対象の判定ゾーンのインデックス。 </param>
        /// <returns> 対象外の場合はゲージ色と同じ透明度を適用した帯色。 </returns>
        private Color GetJustTimingMarkerColor(int zoneIndex)
        {
            Color markerColor = _effectConfig.MarkerColor;
            if (_beatColor == null ||
                !_targetBeatCount.HasValue ||
                zoneIndex < 0 ||
                zoneIndex >= _zoneBeatCounts.Length ||
                zoneIndex >= _beatColor.Length ||
                _zoneBeatCounts[zoneIndex] == _targetBeatCount.Value)
            {
                return markerColor;
            }

            markerColor.a = ApplyTargetDim(_beatColor[zoneIndex], zoneIndex).a;
            return markerColor;
        }

        /// <summary>
        ///     判定ゾーン定義に応じてビートGUIを再構築する。
        /// </summary>
        /// <param name="zones"> 判定ゾーンの一覧。 </param>
        public void ConfigureZones(IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            if (!NeedsRebuild(zones))
            {
                return;
            }

            CacheZones(zones);
            RebuildBeatRectTransforms();
        }

        /// <summary>
        ///    ビートの位置を更新する。
        /// </summary>
        /// <param name="normalizeOffset"> ビートの位置(1が1小節。ジャスト通過分として1を超える値も受け取る)</param>
        /// <param name="isJustTiming"> 音楽同期サービスで確定したジャスト成否。 </param>
        public void SetBeatsOffset(float normalizeOffset, bool isJustTiming)
        {
            bool wasJustTiming = IsOnJustTiming;
            IsOnJustTiming = isJustTiming;
            if (_beatPositionImages == null || _beatPositionImages.Length == 0 || _totalBeatBoxCount <= 0)
            {
                return;
            }

            // normalizeOffsetは1小節基準の進捗。ゲージ全長はGUIDE_LENGTH_IN_BARS小節分のため、
            // ゾーン・Just位置と同じ基準に揃えるためゲージ全長に対する位置へ変換する。
            // 1を超える超過分も同じ換算で扱うことでゾーンとのズレを生まずにJust位置を通過させ、
            // ゲージ全長(=1)で頭打ちにする。
            float gaugeNormalized = Mathf.Clamp01(Mathf.Max(0f, normalizeOffset) / GUIDE_LENGTH_IN_BARS);

            for (int i = 0; i < _beatPositionImages.Length; i++)
            {
                _beatPositionImages[i].fillAmount = gaugeNormalized;
            }

            int activeIndex = Mathf.Clamp(
                                (int)(_totalBeatBoxCount * gaugeNormalized),
                                0,
                                _totalBeatBoxCount - 1);
            if (activeIndex == _currentOpenIndex && wasJustTiming == isJustTiming)
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
            return TryGetJustTimingRange(beatType, out xPosition, out _);
        }

        /// <summary>
        ///     指定した拍子に対応するジャストタイミング区間の中心X座標と幅を取得する。
        /// </summary>
        /// <param name="beatType"> 対象の拍子（BeatTypeの整数値）。 </param>
        /// <param name="xPosition"> 中心からの距離（絶対値）。取得できない場合は0。 </param>
        /// <param name="width"> 入力を受け付ける区間の表示幅。取得できない場合は0。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        public bool TryGetJustTimingRange(int beatType, out float xPosition, out float width)
        {
            xPosition = 0f;
            width = 0f;

            if (_totalBeatBoxCount <= 0)
            {
                return false;
            }

            for (int i = 0; i < _zoneBeatCounts.Length; i++)
            {
                if (_zoneBeatCounts[i] != beatType)
                {
                    continue;
                }

                float center = (_justStarts[i] + _justEnds[i]) * 0.5f;
                float barWidth = _totalBeatBoxCount * _beatWidth / GUIDE_LENGTH_IN_BARS;
                xPosition = center * barWidth;
                width = (_zoneEnds[i] - _zoneStarts[i]) * barWidth;
                return true;
            }

            return false;
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

            int beatIndex = Mathf.Max(0, _currentOpenIndex);

            // 現在ビートの表示はガイドUIの一部のため、対象外ビートの減光を適用する。
            if (!TryGetZoneColor(beatIndex, out Color color, out int zoneIndex))
            {
                return;
            }

            color = ApplyTargetDim(color, zoneIndex);

            for (int i = 0; i < _currentBeatColorImages.Length; i++)
            {
                if (_currentBeatColorImages[i] != null)
                {
                    _currentBeatColorImages[i].color = color;
                }
            }
        }

        /// <summary> ビート描画の基準全長の既定値。 </summary>
        private const float DEFAULT_DISPLAY_LENGTH = 120f;

        /// <summary> ゲージ全長が表す小節数。共通判定定義の小節進捗を描画位置へ換算する。 </summary>
        private const float GUIDE_LENGTH_IN_BARS = 1.5f;

        /// <summary> ジャストタイミング位置を示す帯の横幅倍率。 </summary>
        private const float JUST_TIMING_MARKER_WIDTH_SCALE = 1f / 3f;

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

        [Tooltip("ビート描画全長に掛けるスケールです。")]
        [SerializeField] private float _scale;
        [Tooltip("ビート描画の基準全長です。")]
        [SerializeField] private float _displayLength = DEFAULT_DISPLAY_LENGTH;

        [Space]
        [Tooltip("ビート位置を表示するImage")]
        [SerializeField] private Image[] _beatPositionImages;
        [Tooltip("ビート位置を表示するRectTransform")]
        [SerializeField] private RectTransform[] _beatPositionRectTransforms;
        
        [Space]
        [Tooltip("現在のビート色を表示するImage")]
        [SerializeField]
        private Image[] _currentBeatColorImages;

        [Space]

        [Tooltip("ビートのAlphaを決めるためのCanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

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
        private RectTransform[] _justTimingMarkers;
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

        /// <summary>
        ///     演出設定の設定漏れを検知し、ビートGUIを構築する。
        /// </summary>
        private void Awake()
        {
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

            _targetBeatFrameMotion.TryCancel();
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
                _beatWidth,
                _outTimingSizeDelta,
                _scale,
                out _totalBeatBoxCount,
                out _leftBeatImages,
                out _rightBeatImages,
                out _leftBeatRectTransforms,
                out _rightBeatRectTransforms,
                out _handles);

            CreateJustTimingMarkers();
            RebuildTargetBeatFrames();
            _currentOpenIndex = -1;
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

            if (_justTimingMarkers != null)
            {
                for (int i = 0; i < _justTimingMarkers.Length; i++)
                {
                    if (_justTimingMarkers[i] != null)
                    {
                        Destroy(_justTimingMarkers[i].gameObject);
                    }
                }
            }

            if (_leftBeatRectTransforms != null)
            {
                for (int i = 0; i < _leftBeatRectTransforms.Length; i++)
                {
                    if (_leftBeatRectTransforms[i] != null)
                    {
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
            float overshootSizeDelta = _justTimingSizeDelta + Mathf.Max(0f, _effectConfig.JustOvershootAmount);
            float overshootDuration = Mathf.Max(0.01f, _effectConfig.JustOvershootDuration);
            float returnDuration = Mathf.Max(0.01f, _effectConfig.JustReturnDuration);
            float flashDuration = Mathf.Max(0.01f, _effectConfig.FlashDuration);

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
        ///     ジャストタイミング位置を事前表示する帯を生成する。
        /// </summary>
        private void CreateJustTimingMarkers()
        {
            if (_effectConfig == null || _totalBeatBoxCount <= 0 || _canvasGroup == null)
            {
                _justTimingMarkers = Array.Empty<RectTransform>();
                return;
            }

            _justTimingMarkers = new RectTransform[_zoneBeatCounts.Length * 2];
            float barWidth = _totalBeatBoxCount * _beatWidth / GUIDE_LENGTH_IN_BARS;
            for (int i = 0; i < _zoneBeatCounts.Length; i++)
            {
                float horizontalPosition = (_justStarts[i] + _justEnds[i]) * 0.5f * barWidth;
                float width = (_justEnds[i] - _justStarts[i]) * barWidth * JUST_TIMING_MARKER_WIDTH_SCALE;
                _justTimingMarkers[i * 2] = CreateJustTimingMarker(
                    $"JustTimingMarker_Left_{i}",
                    Vector2.left * horizontalPosition, width, i);
                _justTimingMarkers[i * 2 + 1] = CreateJustTimingMarker(
                    $"JustTimingMarker_Right_{i}",
                    Vector2.right * horizontalPosition, width, i);
            }
        }

        /// <summary>
        ///     指定位置へジャストタイミング表示用の帯を生成する。
        /// </summary>
        /// <param name="objectName"> 生成するオブジェクト名。 </param>
        /// <param name="anchoredPosition"> 生成位置。 </param>
        /// <param name="width"> 共通ジャスト範囲から換算した帯の幅。 </param>
        /// <param name="zoneIndex"> 対応する判定ゾーンのインデックス。 </param>
        /// <returns> 生成した帯のRectTransform。 </returns>
        private RectTransform CreateJustTimingMarker(string objectName, Vector2 anchoredPosition, float width, int zoneIndex)
        {
            GameObject markerObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            markerObject.layer = gameObject.layer;
            markerObject.transform.SetParent(_canvasGroup.transform, false);
            markerObject.transform.SetAsFirstSibling();

            RectTransform markerRectTransform = markerObject.GetComponent<RectTransform>();
            markerRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            markerRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            markerRectTransform.pivot = new Vector2(0.5f, 0.5f);
            // 帯はpivot中央のため、高さの半分だけ持ち上げると下端がガイドの基準線に揃う。
            // そこからの微調整はレイアウト依存のためConfigの補正値で行う。
            float verticalOffset = _effectConfig.MarkerHeight * 0.5f + _effectConfig.MarkerVerticalOffset;
            markerRectTransform.anchoredPosition = anchoredPosition + Vector2.up * verticalOffset;
            markerRectTransform.sizeDelta = new Vector2(
                width,
                Mathf.Max(0.1f, _effectConfig.MarkerHeight));

            Image markerImage = markerObject.GetComponent<Image>();
            markerImage.color = GetJustTimingMarkerColor(zoneIndex);
            markerImage.raycastTarget = false;
            return markerRectTransform;
        }

        /// <summary>
        ///     対象拍と同色の連続ブロックを囲み、中央で接する左右の範囲は一つの赤枠にする。
        ///     描画済みブロックと同じ境界を使い、小節末尾より先の表示範囲も含める。
        /// </summary>
        private void RebuildTargetBeatFrames()
        {
            ClearTargetBeatFrames();
            if (!_targetBeatCount.HasValue || _totalBeatBoxCount <= 0 || _canvasGroup == null)
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

                float start = firstBlockIndex * _beatWidth;
                float end = (blockIndex + 1) * _beatWidth;
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
        ///     指定ゾーンを囲う赤枠を生成し、既存ゲージと白黒枠より前面へ配置する。
        /// </summary>
        /// <param name="objectName"> 生成するオブジェクト名。 </param>
        /// <param name="anchoredPosition"> 対象ゾーン中央の位置。 </param>
        /// <param name="width"> 対象ゾーンの幅。 </param>
        /// <returns> 生成した赤枠のRectTransform。 </returns>
        private RectTransform CreateTargetBeatFrame(string objectName, Vector2 anchoredPosition, float width)
        {
            GameObject frameObject = new GameObject(objectName, typeof(RectTransform));
            frameObject.layer = gameObject.layer;
            frameObject.transform.SetParent(transform, false);
            frameObject.transform.SetAsLastSibling();

            RectTransform frameRectTransform = frameObject.GetComponent<RectTransform>();
            frameRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            frameRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            frameRectTransform.pivot = new Vector2(0.5f, 0.5f);
            frameRectTransform.anchoredPosition = anchoredPosition;
            float frameWidth = Mathf.Max(
                TARGET_BEAT_FRAME_THICKNESS,
                width + TARGET_BEAT_FRAME_HORIZONTAL_PADDING * 2f);
            float frameHeight = Mathf.Max(
                TARGET_BEAT_FRAME_THICKNESS,
                _outTimingSizeDelta + TARGET_BEAT_FRAME_VERTICAL_PADDING * 2f);
            frameRectTransform.sizeDelta = new Vector2(frameWidth, frameHeight);

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
        /// <param name="beatWidth"> 1ブロックの幅。 </param>
        /// <param name="beatHeight"> 1ブロックの初期高さ。 </param>
        /// <param name="scale"> ビート描画全長に掛けるスケール。 </param>
        /// <param name="totalBeatBoxCount"> 生成したブロック総数。生成できない場合は0。 </param>
        /// <param name="leftBeatImages"> 左側ブロックのImage。 </param>
        /// <param name="rightBeatImages"> 右側ブロックのImage。 </param>
        /// <param name="leftBeatRT"> 左側ブロックのRectTransform。 </param>
        /// <param name="rightBeatRT"> 右側ブロックのRectTransform。 </param>
        /// <param name="handles"> ブロックごとのモーションハンドル。 </param>
        private void InitBeatGUI(
            in GameObject parent,
            float beatWidth,
            float beatHeight,
            float scale,
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
            float guiLength = _displayLength * scale;
            int beatBlockCount = Mathf.Max(1, Mathf.FloorToInt(guiLength / beatWidth));
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
                Color color = GetTargetColorForIndex(i);

                GameObject leftBeat = new GameObject($"LeftBeat_{i}", typeof(RectTransform), typeof(Image));
                leftBeat.transform.SetParent(parent.transform, false);
                RectTransform leftRT = leftBeat.GetComponent<RectTransform>();
                Image leftImage = leftBeat.GetComponent<Image>();
                leftImage.color = color; //ビートの色を設定
                leftRT.anchoredPosition = Vector2.left * (i * beatWidth);
                leftRT.sizeDelta = new Vector2(beatWidth, beatHeight);
                leftRT.pivot = new Vector2(1f, 0.5f);
                leftBeatImages[i] = leftImage;
                leftBeatRT[i] = leftRT;

                GameObject rightBeat = new GameObject($"RightBeat_{i}", typeof(RectTransform), typeof(Image));
                rightBeat.transform.SetParent(parent.transform, false);
                RectTransform rightRT = rightBeat.GetComponent<RectTransform>();
                Image rightImage = rightBeat.GetComponent<Image>();
                rightImage.color = color; //ビートの色を設定
                rightRT.anchoredPosition = Vector2.right * (i * beatWidth);
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
            float position = (float)blockIndex / _totalBeatBoxCount * GUIDE_LENGTH_IN_BARS;

            // ブロック数の切り捨て後の実描画全長を使い、進捗・Just位置と同じ小節単位で比較する。
            // 最終ゾーンは、1小節を超えるゲージ末端まで表示する。
            for (int i = 0; i < _zoneStarts.Length; i++)
            {
                float start = _zoneStarts[i];
                float end = _zoneEnds[i];

                if (position >= start && position < end)
                {
                    return i;
                }
            }

            return _zoneStarts.Length - 1;
        }
    }
}
