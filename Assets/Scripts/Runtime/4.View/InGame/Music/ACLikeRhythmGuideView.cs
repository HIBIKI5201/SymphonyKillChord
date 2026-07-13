using KillChord.Runtime.View.InGame.Sequence;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        ///    ビートのアニメーションを更新する。
        /// </summary>
        /// <param name="closeIndex"></param>
        /// <param name="openIndex"></param>
        /// <param name="isJustTiming"></param>
        public void SetBeatAnimation(int openIndex, bool isJustTiming)
        {
            if (_handles == null || _leftBeatRectTransforms == null || _rightBeatRectTransforms == null)
            {
                return;
            }

            float targetSizeDelta = isJustTiming ? _justTimingSizeDelta : _inTimingSizeDelta;
            if (openIndex != -1)
            {
                _handles[openIndex].TryComplete();
                _handles[openIndex] = LSequence.Create()
                    .Join(LMotion.Create(targetSizeDelta, _outTimingSizeDelta, _outTimingDuration)
                        .WithEase(Ease.OutCirc)
                        .BindToSizeDeltaY(_leftBeatRectTransforms[openIndex]))
                    .Join(LMotion.Create(targetSizeDelta, _outTimingSizeDelta, _outTimingDuration)
                        .WithEase(Ease.OutCirc)
                        .BindToSizeDeltaY(_rightBeatRectTransforms[openIndex]))
                    .Run();
            }
        }



        [Space]

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
        private int[] _justTimingBeatBoxIndex;
        private MotionHandle[] _handles;
        private int _totalBeatBoxCount;
        private int _currentOpenIndex = -1;
        private float[] _zoneStarts = Array.Empty<float>();
        private float[] _zoneEnds = Array.Empty<float>();
        private int[] _zoneBeatCounts = Array.Empty<int>();

        private void Awake()
        {
            RebuildBeatRectTransforms();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
            OnStartGameplay = null;
            OnStopGameplay = null;
            if (_handles == null)
            {
                return;
            }

            for (int i = 0; i < _handles.Length; i++)
            {
                _handles[i].TryCancel();
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

            _currentOpenIndex = -1;
        }

        /// <summary>
        ///     生成済みビートオブジェクトを破棄する。
        /// </summary>
        private void ClearGeneratedBeatObjects()
        {
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
