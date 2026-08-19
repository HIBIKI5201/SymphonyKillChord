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
    public sealed class ACLikeRhythmGuideView : MonoBehaviour, IGameplayControllable, IRhythmGuideBeatViewModel
    {
        /// <summary> ガイド表示の更新タイミングを通知します。 </summary>
        public event Action OnUpdate;

        /// <summary> ゲームプレイ開始を通知します。 </summary>
        public event Action OnStartGameplay;

        /// <summary> ゲームプレイ停止を通知します。 </summary>
        public event Action OnStopGameplay;

        /// <summary>
        ///     現在のビート位置がジャストタイミングのブロック上にあるか。
        ///     ガイドに表示しているJustTimingMarkerと同じ基準で判定する。
        /// </summary>
        public bool IsOnJustTiming
        {
            get
            {
                if (_justTimingBeatBoxIndex == null || _currentOpenIndex < 0)
                {
                    return false;
                }

                for (int i = 0; i < _justTimingBeatBoxIndex.Length; i++)
                {
                    if (_currentOpenIndex == _justTimingBeatBoxIndex[i])
                    {
                        return true;
                    }
                }

                return false;
            }
        }

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
        /// <param name="normalizeOffset"> ビートの位置(0～1の値範囲)</param>
        public void SetBeatsOffset(float normalizeOffset)
        {
            if (_beatPositionImages == null || _beatPositionImages.Length == 0 || _totalBeatBoxCount <= 0)
            {
                return;
            }

            // normalizeOffsetは1小節基準の進捗(0～1)。ゲージ全長はGUIDE_LENGTH_IN_BARS小節分のため、
            // ゾーン・Just位置と同じ基準に揃えるためゲージ全長に対する位置へ変換する。
            float gaugeNormalized = Mathf.Clamp01(normalizeOffset) / GUIDE_LENGTH_IN_BARS;

            for (int i = 0; i < _beatPositionImages.Length; i++)
            {
                _beatPositionImages[i].fillAmount = gaugeNormalized;
            }

            int activeIndex = Mathf.Clamp(
                                (int)(_totalBeatBoxCount * gaugeNormalized),
                                0,
                                _totalBeatBoxCount - 1);
            if (activeIndex == _currentOpenIndex)
            {
                return;
            }
            bool isJustTiming = false;
            for (int i = 0; i < _justTimingBeatBoxIndex.Length; i++)
            {
                if (activeIndex == _justTimingBeatBoxIndex[i])
                {
                    isJustTiming = true;
                    break;
                }
            }
            SetBeatAnimation(activeIndex, isJustTiming);
            _currentOpenIndex = activeIndex;
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

            if (_zoneBeatCounts == null || _justTimingBeatBoxIndex == null)
            {
                return false;
            }

            for (int i = 0; i < _zoneBeatCounts.Length && i < _justTimingBeatBoxIndex.Length; i++)
            {
                if (_zoneBeatCounts[i] != beatType)
                {
                    continue;
                }

                xPosition = (_justTimingBeatBoxIndex[i] + 0.5f) * _beatWidth;
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
            Color beatColor = _beatColor[GetBeatSectionIndex(openIndex, _scale, _beatWidth)];

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
        ///     現在カーソルが乗っているビートブロックの色を取得する。
        /// </summary>
        /// <param name="color"> ビートブロックの色。 </param>
        /// <returns> 取得できた場合はtrue。 </returns>
        public bool TryGetCurrentBeatColor(out Color color)
        {
            color = default;

            if (_beatColor == null || _beatColor.Length == 0)
            {
                return false;
            }

            int beatIndex = Mathf.Max(0, _currentOpenIndex);
            int index = GetBeatSectionIndex(beatIndex, _scale, _beatWidth);
            if (index < 0)
            {
                return false;
            }
            color = _beatColor[index];
            return true;
        }

        /// <summary> ビート描画の基準全長の既定値。 </summary>
        private const float DEFAULT_DISPLAY_LENGTH = 120f;

        /// <summary> ゲージ全長が表す小節数。Justは小節内正規化位置(1/BeatCount)をこの値で割った位置になる。 </summary>
        private const float GUIDE_LENGTH_IN_BARS = 1.5f;

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

        [Tooltip("ビートのAlphaを決めるためのCanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Tooltip("ターゲット時の透明度。")]
        [Range(0f, 1f)]
        [SerializeField] private float _targetAlpha;
        [Tooltip("非ターゲット時の透明度。")]
        [Range(0f, 1f)]
        [SerializeField] private float _noTargetAlpha;

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
        private int[] _justTimingBeatBoxIndex;
        private MotionHandle[] _handles;
        private int _totalBeatBoxCount;
        private int _currentOpenIndex = -1;
        private float[] _zoneStarts = Array.Empty<float>();
        private float[] _zoneEnds = Array.Empty<float>();
        private int[] _zoneBeatCounts = Array.Empty<int>();

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
                out _handles,
                out _justTimingBeatBoxIndex);

            CreateJustTimingMarkers();
            _currentOpenIndex = -1;
        }

        /// <summary>
        ///     生成済みビートオブジェクトを破棄する。
        /// </summary>
        private void ClearGeneratedBeatObjects()
        {
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
            if (_effectConfig == null || _justTimingBeatBoxIndex == null || _canvasGroup == null)
            {
                _justTimingMarkers = Array.Empty<RectTransform>();
                return;
            }

            _justTimingMarkers = new RectTransform[_justTimingBeatBoxIndex.Length * 2];
            for (int i = 0; i < _justTimingBeatBoxIndex.Length; i++)
            {
                float horizontalPosition = (_justTimingBeatBoxIndex[i] + 0.5f) * _beatWidth;
                _justTimingMarkers[i * 2] = CreateJustTimingMarker(
                    $"JustTimingMarker_Left_{i}",
                    Vector2.left * horizontalPosition);
                _justTimingMarkers[i * 2 + 1] = CreateJustTimingMarker(
                    $"JustTimingMarker_Right_{i}",
                    Vector2.right * horizontalPosition);
            }
        }

        /// <summary>
        ///     指定位置へジャストタイミング表示用の帯を生成する。
        /// </summary>
        /// <param name="objectName"> 生成するオブジェクト名。 </param>
        /// <param name="anchoredPosition"> 生成位置。 </param>
        /// <returns> 生成した帯のRectTransform。 </returns>
        private RectTransform CreateJustTimingMarker(string objectName, Vector2 anchoredPosition)
        {
            GameObject markerObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            markerObject.layer = gameObject.layer;
            markerObject.transform.SetParent(_canvasGroup.transform, false);
            markerObject.transform.SetAsFirstSibling();

            RectTransform markerRectTransform = markerObject.GetComponent<RectTransform>();
            markerRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            markerRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            markerRectTransform.pivot = new Vector2(0.5f, 0.5f);
            markerRectTransform.anchoredPosition = anchoredPosition + Vector2.up * (_effectConfig.MarkerHeight * 0.5f - 20f);
            markerRectTransform.sizeDelta = new Vector2(
                Mathf.Max(0.1f, _effectConfig.MarkerWidth),
                Mathf.Max(0.1f, _effectConfig.MarkerHeight));

            Image markerImage = markerObject.GetComponent<Image>();
            markerImage.color = _effectConfig.MarkerColor;
            markerImage.raycastTarget = false;
            return markerRectTransform;
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
                _zoneBeatCounts = Array.Empty<int>();
                return;
            }

            _zoneStarts = new float[zones.Count];
            _zoneEnds = new float[zones.Count];
            _zoneBeatCounts = new int[zones.Count];

            for (int i = 0; i < zones.Count; i++)
            {
                _zoneStarts[i] = zones[i].StartNormalized;
                _zoneEnds[i] = zones[i].EndNormalized;
                _zoneBeatCounts[i] = zones[i].BeatCount;
            }
        }

        /// <summary>
        ///     判定ゾーン定義からスペクトラム風ビートのブロックを左右対称に生成し、
        ///     生成物とジャストタイミング位置を出力する。
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
        /// <param name="justTimingBeatBoxIndex"> 判定ゾーンごとのジャストタイミング位置のブロック番号。 </param>
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
            out MotionHandle[] handles,
            out int[] justTimingBeatBoxIndex)
        {
            //Outの初期化
            leftBeatImages = null;
            rightBeatImages = null;
            leftBeatRT = null;
            rightBeatRT = null;
            handles = null;
            justTimingBeatBoxIndex = null;

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
            justTimingBeatBoxIndex = new int[_zoneStarts.Length];

            //スペクトラム風ビートのブロックを生成
            for (int i = 0; i < beatBlockCount; i++)
            {
                Color color = _beatColor[GetBeatSectionIndex(i, scale, beatWidth)];

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

            for (int i = 0; i < justTimingBeatBoxIndex.Length; i++)
            {
                int beatCount = _zoneBeatCounts[i];
                // Justは「1小節をBeatCount(拍種)で割った位置」。ゲージ全長はGUIDE_LENGTH_IN_BARS小節分を表示しているため、
                // 小節内正規化位置(1/beatCount)をGUIDE_LENGTH_IN_BARSで割ってゲージ全長に対する位置へ変換する。
                float justNormalized = beatCount > 0 ? (1f / beatCount) / GUIDE_LENGTH_IN_BARS : 0f;
                float position = justNormalized * _displayLength;
                justTimingBeatBoxIndex[i] = Mathf.Clamp(
                    Mathf.FloorToInt(position * scale / beatWidth),
                    0,
                    beatBlockCount - 1);
            }
        }

        /// <summary>
        ///     ブロックインデックスがどの判定ゾーンに属するかを返す。
        /// </summary>
        /// <param name="blockIndex"> ブロックのインデックス。 </param>
        /// <param name="scale"> ビートのスケール。 </param>
        /// <param name="beatWidth"> 1ブロックの幅。 </param>
        /// <returns> 属する判定ゾーンのインデックス。 </returns>
        private int GetBeatSectionIndex(int blockIndex, float scale, float beatWidth)
        {
            float position = (blockIndex * beatWidth) / scale;

            // _zoneStarts/_zoneEndsは1小節基準（0～1）の正規化値のため、
            // GUIDE_LENGTH_IN_BARS小節分を表すゲージ全長へ変換してから比較する。
            for (int i = 0; i < _zoneStarts.Length; i++)
            {
                float start = (_zoneStarts[i] / GUIDE_LENGTH_IN_BARS) * _displayLength;
                float end = (_zoneEnds[i] / GUIDE_LENGTH_IN_BARS) * _displayLength;

                if (position >= start && position < end)
                {
                    return i;
                }
            }

            return _zoneStarts.Length - 1;
        }
    }
}
