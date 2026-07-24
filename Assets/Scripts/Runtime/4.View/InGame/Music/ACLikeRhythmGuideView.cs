using KillChord.Runtime.View.InGame.Sequence;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class ACLikeRhythmGuideView : MonoBehaviour, IGameplayControllable
    {
        public event Action OnUpdate;
        public event Action OnStartGameplay;
        public event Action OnStopGameplay;

        public void StartGameplay()
        {
            OnStartGameplay?.Invoke();
        }

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
        public void ConfigureZones(IReadOnlyList<KillChord.Runtime.Adaptor.InGame.Music.RhythmGuideZoneDto> zones)
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

            for (int i = 0; i < _beatPositionImages.Length; i++)
            {
                _beatPositionImages[i].fillAmount = Mathf.Clamp01(normalizeOffset);
            }

            int activeIndex = Mathf.Clamp(
                                (int)(_totalBeatBoxCount * Mathf.Clamp01(normalizeOffset)),
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
                || openIndex >= _handles.Length)
            {
                return;
            }

            _handles[openIndex].TryComplete();
            Color beatColor = _beatColor[GetBeatSectionIndex(openIndex, _scale, _beatWidth)];

            if (isJustTiming && _effectConfig != null)
            {
                _handles[openIndex] = CreateJustTimingMotion(openIndex, beatColor);
                PlayJustTimingVignette(beatColor);
                return;
            }

            float targetSizeDelta = isJustTiming ? _justTimingSizeDelta : _inTimingSizeDelta;
            Ease ease = _effectConfig != null ? _effectConfig.NormalTimingEase : Ease.OutCirc;
            _handles[openIndex] = CreateNormalTimingMotion(openIndex, targetSizeDelta, ease);
        }



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
        [SerializeField] private RectTransform[] _beatPositionRectTransfroms;

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
        private MotionHandle _vignetteMotion;
        private Volume _vignetteVolume;
        private VolumeProfile _vignetteProfile;
        private Vignette _justTimingVignette;
        private int _totalBeatBoxCount;
        private int _currentOpenIndex = -1;
        private float _lastVignetteTimestamp = float.NegativeInfinity;
        private float[] _zoneStarts = Array.Empty<float>();
        private float[] _zoneEnds = Array.Empty<float>();
        private int[] _zoneBeatCounts = Array.Empty<int>();

        private void Awake()
        {
            if (_effectConfig == null)
            {
                Debug.LogWarning($"[{nameof(ACLikeRhythmGuideView)}] ジャストタイミング演出設定が未設定です。", this);
            }

            InitializeJustTimingVignette();
            RebuildBeatRectTransforms();
        }

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
            _vignetteMotion.TryCancel();

            if (_handles != null)
            {
                for (int i = 0; i < _handles.Length; i++)
                {
                    _handles[i].TryCancel();
                }
            }

            if (_vignetteVolume != null)
            {
                _vignetteVolume.profile = null;
            }

            if (_justTimingVignette != null)
            {
                if (UnityEngine.Application.isPlaying)
                {
                    Destroy(_justTimingVignette);
                }
                else
                {
                    DestroyImmediate(_justTimingVignette);
                }
            }

            if (_vignetteProfile != null)
            {
                if (UnityEngine.Application.isPlaying)
                {
                    Destroy(_vignetteProfile);
                }
                else
                {
                    DestroyImmediate(_vignetteProfile);
                }
            }
        }

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
            markerRectTransform.anchoredPosition = anchoredPosition;
            markerRectTransform.sizeDelta = new Vector2(
                Mathf.Max(0.1f, _effectConfig.MarkerWidth),
                Mathf.Max(0.1f, _effectConfig.MarkerHeight));

            Image markerImage = markerObject.GetComponent<Image>();
            markerImage.color = _effectConfig.MarkerColor;
            markerImage.raycastTarget = false;
            return markerRectTransform;
        }

        /// <summary>
        ///     ジャストタイミング用の実行時Vignetteを初期化する。
        /// </summary>
        private void InitializeJustTimingVignette()
        {
            if (_effectConfig == null || !_effectConfig.IsVignetteEnabled)
            {
                return;
            }

            GameObject volumeObject = new GameObject("JustTimingVignetteVolume", typeof(Volume));
            volumeObject.transform.SetParent(transform, false);
            volumeObject.layer = 0;

            _vignetteVolume = volumeObject.GetComponent<Volume>();
            _vignetteVolume.isGlobal = true;
            _vignetteVolume.priority = _effectConfig.VignettePriority;
            _vignetteVolume.weight = 1f;

            _vignetteProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            _vignetteProfile.name = "JustTimingVignetteProfile";
            _vignetteProfile.hideFlags = HideFlags.DontSave;
            _justTimingVignette = _vignetteProfile.Add<Vignette>(true);
            _justTimingVignette.color.value = Color.black;
            _justTimingVignette.intensity.value = 0f;
            _justTimingVignette.smoothness.value = Mathf.Clamp01(_effectConfig.VignetteSmoothness);
            _justTimingVignette.rounded.value = false;
            _vignetteVolume.profile = _vignetteProfile;
        }

        /// <summary>
        ///     ビート色に連動した全画面Vignetteを再生する。
        /// </summary>
        /// <param name="beatColor"> Vignetteへ反映するビート色。 </param>
        private void PlayJustTimingVignette(Color beatColor)
        {
            if (_effectConfig == null || !_effectConfig.IsVignetteEnabled || _justTimingVignette == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now - _lastVignetteTimestamp < Mathf.Max(0f, _effectConfig.VignetteMinimumInterval))
            {
                return;
            }

            _lastVignetteTimestamp = now;
            _vignetteMotion.TryCancel();

            beatColor.a = 1f;
            float intensity = Mathf.Clamp01(_effectConfig.VignetteIntensity);
            _justTimingVignette.color.value = beatColor;
            _justTimingVignette.smoothness.value = Mathf.Clamp01(_effectConfig.VignetteSmoothness);
            _justTimingVignette.intensity.value = intensity;

            _vignetteMotion = LMotion.Create(
                    intensity,
                    0f,
                    Mathf.Max(0.01f, _effectConfig.VignetteDuration))
                .WithEase(_effectConfig.VignetteEase)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(_justTimingVignette, static (value, vignette) => vignette.intensity.value = value);
        }

        /// <summary>
        ///     判定ゾーン再構築が必要か判定する。
        /// </summary>
        /// <param name="zones"> 判定ゾーンの一覧。 </param>
        /// <returns> 再構築が必要な場合はtrue。 </returns>
        private bool NeedsRebuild(IReadOnlyList<KillChord.Runtime.Adaptor.InGame.Music.RhythmGuideZoneDto> zones)
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
        private void CacheZones(IReadOnlyList<KillChord.Runtime.Adaptor.InGame.Music.RhythmGuideZoneDto> zones)
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
                Debug.LogError("_beatColor の長さが判定ゾーン数より少ないです。", this);
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
                float position = ((_zoneStarts[i] + _zoneEnds[i]) * 0.5f) * _displayLength;
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

            for (int i = 0; i < _zoneStarts.Length; i++)
            {
                float start = _zoneStarts[i] * _displayLength;
                float end = _zoneEnds[i] * _displayLength;

                if (position >= start && position < end)
                {
                    return i;
                }
            }

            return _zoneStarts.Length - 1;
        }
        private const float DEFAULT_DISPLAY_LENGTH = 120f;
    }
}
