using KillChord.Runtime.Adaptor.InGame.Music;
using LitMotion;
using LitMotion.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドをリング型で描くViewです。試遊でリズムUIの見た目を見比べるために使います。
    ///     1小節を24段階に分けた区切りを、時計回りに順に点けて進み具合を見せます。
    ///     スプライトを使わず、細長い四角を並べてリングを作ります。
    /// </summary>
    [AddComponentMenu("KillChord/View/Music/RhythmGuideRingView")]
    public sealed class RhythmGuideRingView : MonoBehaviour
    {
        /// <summary>
        ///     リングの形と色を設定します。生成した直後に1度だけ呼びます。
        /// </summary>
        /// <param name="shape"> リングの形です。 </param>
        /// <param name="beatColors"> 判定ゾーンの順に並んだ拍の色です。 </param>
        /// <param name="lockedColor"> 攻撃硬直で入力できない範囲の色です。 </param>
        /// <param name="isOneBarGauge"> 1小節のゲージの案を有効にしている場合はtrue。 </param>
        public void Initialize(RhythmGuideRingShape shape, IReadOnlyList<Color> beatColors, Color lockedColor,
            bool isOneBarGauge)
        {
            _shape = shape;
            _beatColors = beatColors ?? Array.Empty<Color>();
            _lockedColor = lockedColor;
            _isOneBarGauge = isOneBarGauge;

            RectTransform rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        /// <summary>
        ///     判定ゾーンを設定し、変わった場合はリングの区切りを作り直します。
        /// </summary>
        /// <param name="zones"> 判定ゾーンの一覧です。 </param>
        /// <param name="guideLengthInBars"> タイムアウトまでの小節数です。 </param>
        public void ConfigureZones(IReadOnlyList<RhythmGuideZoneDto> zones, float guideLengthInBars)
        {
            _timeoutBarCount = guideLengthInBars;
            float lengthInBars = _isOneBarGauge ? Mathf.Min(ONE_BAR, guideLengthInBars) : guideLengthInBars;
            Vector2 size = ((RectTransform)transform).rect.size;
            if (Mathf.Approximately(_lengthInBars, lengthInBars) && size == _builtSize && !HasZonesChanged(zones))
            {
                return;
            }

            _lengthInBars = lengthInBars;
            _builtSize = size;
            CacheZones(zones);
            RebuildSegments();
        }

        /// <summary>
        ///     攻撃硬直で入力できない範囲を設定します。
        /// </summary>
        /// <param name="lockedBarProgress"> 直前の入力位置から入力できない範囲の長さ（小節単位）です。 </param>
        public void SetLockedRange(float lockedBarProgress)
        {
            lockedBarProgress = Mathf.Max(0f, lockedBarProgress);
            if (Mathf.Approximately(_lockedBarProgress, lockedBarProgress))
            {
                return;
            }

            _lockedBarProgress = lockedBarProgress;
            _isColorDirty = true;
        }

        /// <summary>
        ///     チュートリアル等で対象の拍を設定し、対象外の拍を薄くします。
        /// </summary>
        /// <param name="targetBeatCount"> 対象の拍です。対象がない場合はnull。 </param>
        public void SetTargetBeatCount(int? targetBeatCount)
        {
            if (_targetBeatCount == targetBeatCount)
            {
                return;
            }

            _targetBeatCount = targetBeatCount;
            _isColorDirty = true;
        }

        /// <summary>
        ///     攻撃目標の有無に合わせて透明度を変えます。
        /// </summary>
        /// <param name="hasTarget"> 攻撃目標がある場合はtrue。 </param>
        public void SetAlpha(bool hasTarget)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = hasTarget ? TARGET_ALPHA : NO_TARGET_ALPHA;
            }
        }

        /// <summary>
        ///     進み具合を更新します。
        /// </summary>
        /// <param name="barProgress"> 前回の入力からの経過（小節単位）です。 </param>
        /// <param name="isJustTiming"> 今がJustの範囲の中ならtrue。 </param>
        public void SetBeatsOffset(float barProgress, bool isJustTiming)
        {
            if (_segmentImages.Length == 0 || _lengthInBars <= 0f)
            {
                return;
            }

            int segmentCount = _segmentImages.Length;
            int litCount = Mathf.Clamp(Mathf.CeilToInt(barProgress / _lengthInBars * segmentCount), 0, segmentCount);
            int vanishedCount = CalculateVanishedCount(barProgress, segmentCount);
            bool wasJustTiming = _isJustTiming;
            _isJustTiming = isJustTiming;
            if (litCount == _litCount && vanishedCount == _vanishedCount && !_isColorDirty
                && wasJustTiming == isJustTiming)
            {
                return;
            }

            int previousLitCount = _litCount;
            _litCount = litCount;
            _vanishedCount = vanishedCount;
            _isColorDirty = false;
            UpdateSegmentColors();

            // 新しく点いた区切りを弾ませ、Justの範囲の中では白く光らせて、進んだことを伝える。
            int currentIndex = litCount - 1;
            if (currentIndex >= 0 && (litCount != previousLitCount || (isJustTiming && !wasJustTiming)))
            {
                PlaySegmentPulse(currentIndex, isJustTiming);
            }
        }

        /// <summary> 1小節を分ける区切りの数です。 </summary>
        private const int STEPS_PER_BAR = 24;

        /// <summary> 1小節を表す小節数です。 </summary>
        private const float ONE_BAR = 1f;

        /// <summary> 大きいリングの半径の、画面の短い辺に対する割合です。 </summary>
        private const float CIRCLE_RADIUS_RATE = 0.3f;

        /// <summary> 大きいリングの区切りの太さです。 </summary>
        private const float CIRCLE_THICKNESS = 28f;

        /// <summary> 外周のリングの、画面の端からの余白です。 </summary>
        private const float EDGE_INSET = 28f;

        /// <summary> 外周のリングの区切りの太さです。 </summary>
        private const float EDGE_THICKNESS = 18f;

        /// <summary> 区切りどうしの隙間です。 </summary>
        private const float SEGMENT_GAP = 6f;

        /// <summary> Justがある区切りの太さに掛ける倍率です。 </summary>
        private const float JUST_THICKNESS_RATE = 1.8f;

        /// <summary> Justがある区切りの色を白へ寄せる割合です。 </summary>
        private const float JUST_WHITE_BLEND = 0.5f;

        /// <summary> まだ進んでいない区切りの透明度の倍率です。 </summary>
        private const float FUTURE_ALPHA_RATE = 0.3f;

        /// <summary> チュートリアルで対象外の拍の透明度の倍率です。 </summary>
        private const float DIM_ALPHA_RATE = 0.3f;

        /// <summary> 攻撃目標があるときの透明度です。 </summary>
        private const float TARGET_ALPHA = 1f;

        /// <summary> 攻撃目標がないときの透明度です。 </summary>
        private const float NO_TARGET_ALPHA = 0.6f;

        /// <summary> 点いた区切りを弾ませる大きさです。 </summary>
        private const float PULSE_STRENGTH = 0.5f;

        /// <summary> 点いた区切りを弾ませる時間（秒）です。 </summary>
        private const float PULSE_DURATION = 0.15f;

        /// <summary> 外周のリングで、画面の角を回る向きの数です。 </summary>
        private const int EDGE_SIDE_COUNT = 4;

        private RhythmGuideRingShape _shape;
        private IReadOnlyList<Color> _beatColors = Array.Empty<Color>();
        private Color _lockedColor;
        private bool _isOneBarGauge;
        private CanvasGroup _canvasGroup;
        private Image[] _segmentImages = Array.Empty<Image>();
        private RectTransform[] _segmentRectTransforms = Array.Empty<RectTransform>();
        private int[] _segmentZoneIndices = Array.Empty<int>();
        private bool[] _segmentJustFlags = Array.Empty<bool>();
        private float[] _segmentStartProgresses = Array.Empty<float>();
        private RhythmGuideZoneDto[] _zones = Array.Empty<RhythmGuideZoneDto>();
        private Vector2 _builtSize;
        private float _lengthInBars;
        private float _timeoutBarCount;
        private float _lockedBarProgress;
        private int? _targetBeatCount;
        private int _litCount = -1;
        private int _vanishedCount;
        private bool _isJustTiming;
        private bool _isColorDirty = true;
        private MotionHandle _pulseHandle;

        /// <summary>
        ///     破棄時に再生中の演出を止めます。
        /// </summary>
        private void OnDestroy()
        {
            _pulseHandle.TryCancel();
        }

        /// <summary>
        ///     判定ゾーンが前回と変わったか判定します。
        /// </summary>
        /// <param name="zones"> 今回の判定ゾーンです。 </param>
        /// <returns> 変わった場合はtrue。 </returns>
        private bool HasZonesChanged(IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            int count = zones?.Count ?? 0;
            if (count != _zones.Length)
            {
                return true;
            }

            for (int i = 0; i < count; i++)
            {
                if (!zones[i].Equals(_zones[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     判定ゾーンを控えます。
        /// </summary>
        /// <param name="zones"> 控える判定ゾーンです。 </param>
        private void CacheZones(IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            int count = zones?.Count ?? 0;
            _zones = new RhythmGuideZoneDto[count];
            for (int i = 0; i < count; i++)
            {
                _zones[i] = zones[i];
            }
        }

        /// <summary>
        ///     リングの区切りを破棄し、今の形・大きさ・判定ゾーンで作り直します。
        /// </summary>
        private void RebuildSegments()
        {
            _pulseHandle.TryCancel();
            for (int i = 0; i < _segmentRectTransforms.Length; i++)
            {
                if (_segmentRectTransforms[i] != null)
                {
                    Destroy(_segmentRectTransforms[i].gameObject);
                }
            }

            int segmentCount = _zones.Length > 0 && _lengthInBars > 0f
                ? Mathf.Max(1, Mathf.RoundToInt(STEPS_PER_BAR * _lengthInBars))
                : 0;
            _segmentImages = new Image[segmentCount];
            _segmentRectTransforms = new RectTransform[segmentCount];
            _segmentZoneIndices = new int[segmentCount];
            _segmentJustFlags = new bool[segmentCount];
            _segmentStartProgresses = new float[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                float startProgress = _lengthInBars * i / segmentCount;
                float endProgress = _lengthInBars * (i + 1) / segmentCount;
                _segmentStartProgresses[i] = startProgress;
                _segmentZoneIndices[i] = ResolveZoneIndex((startProgress + endProgress) * 0.5f);
                _segmentJustFlags[i] = ContainsJust(startProgress, endProgress);
                CreateSegment(i, segmentCount);
            }

            _litCount = -1;
            _vanishedCount = 0;
            _isColorDirty = true;
        }

        /// <summary>
        ///     区切りを1つ作り、形に合わせて置きます。
        /// </summary>
        /// <param name="index"> 区切りの番号です。 </param>
        /// <param name="segmentCount"> 区切りの数です。 </param>
        private void CreateSegment(int index, int segmentCount)
        {
            GameObject segmentObject = new GameObject($"RingSegment_{index}", typeof(RectTransform), typeof(Image));
            segmentObject.layer = gameObject.layer;
            segmentObject.transform.SetParent(transform, false);
            RectTransform segmentRect = segmentObject.GetComponent<RectTransform>();
            segmentRect.anchorMin = new Vector2(0.5f, 0.5f);
            segmentRect.anchorMax = new Vector2(0.5f, 0.5f);
            segmentRect.pivot = new Vector2(0.5f, 0.5f);

            float thicknessRate = _segmentJustFlags[index] ? JUST_THICKNESS_RATE : 1f;
            if (_shape == RhythmGuideRingShape.ScreenEdge)
            {
                PlaceOnScreenEdge(segmentRect, index, segmentCount, EDGE_THICKNESS * thicknessRate);
            }
            else
            {
                PlaceOnCircle(segmentRect, index, segmentCount, CIRCLE_THICKNESS * thicknessRate);
            }

            Image image = segmentObject.GetComponent<Image>();
            image.raycastTarget = false;
            _segmentImages[index] = image;
            _segmentRectTransforms[index] = segmentRect;
        }

        /// <summary>
        ///     区切りを、画面の中央の円の上に、真上から時計回りに置きます。
        /// </summary>
        /// <param name="segmentRect"> 置く区切りです。 </param>
        /// <param name="index"> 区切りの番号です。 </param>
        /// <param name="segmentCount"> 区切りの数です。 </param>
        /// <param name="thickness"> 区切りの太さです。 </param>
        private void PlaceOnCircle(RectTransform segmentRect, int index, int segmentCount, float thickness)
        {
            float radius = Mathf.Min(_builtSize.x, _builtSize.y) * CIRCLE_RADIUS_RATE;
            float angle = 90f - (index + 0.5f) / segmentCount * 360f;
            float radian = angle * Mathf.Deg2Rad;
            float length = Mathf.Max(1f, 2f * Mathf.PI * radius / segmentCount - SEGMENT_GAP);
            segmentRect.anchoredPosition = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * radius;
            // 横の辺を円の接線に合わせる。時計回りに進むため、接線は半径の向きから90度戻した向き。
            segmentRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            segmentRect.sizeDelta = new Vector2(length, thickness);
        }

        /// <summary>
        ///     区切りを、画面の外周に沿って、上の辺の中央から時計回りに置きます。
        /// </summary>
        /// <param name="segmentRect"> 置く区切りです。 </param>
        /// <param name="index"> 区切りの番号です。 </param>
        /// <param name="segmentCount"> 区切りの数です。 </param>
        /// <param name="thickness"> 区切りの太さです。 </param>
        private void PlaceOnScreenEdge(RectTransform segmentRect, int index, int segmentCount, float thickness)
        {
            float halfWidth = Mathf.Max(0f, _builtSize.x * 0.5f - EDGE_INSET);
            float halfHeight = Mathf.Max(0f, _builtSize.y * 0.5f - EDGE_INSET);
            float perimeter = 4f * (halfWidth + halfHeight);
            float distance = (index + 0.5f) / segmentCount * perimeter;
            float length = Mathf.Max(1f, perimeter / segmentCount - SEGMENT_GAP);

            // 上の辺の右半分、右の辺、下の辺、左の辺、上の辺の左半分の順に、外周を時計回りにたどる。
            Vector2[] corners =
            {
                new Vector2(halfWidth, halfHeight),
                new Vector2(halfWidth, -halfHeight),
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(-halfWidth, halfHeight),
            };
            Vector2 start = new Vector2(0f, halfHeight);
            Vector2 position = start;
            float angle = 0f;
            float remaining = distance;
            for (int side = 0; side <= EDGE_SIDE_COUNT; side++)
            {
                Vector2 end = side < EDGE_SIDE_COUNT ? corners[side] : new Vector2(0f, halfHeight);
                Vector2 direction = end - start;
                float sideLength = direction.magnitude;
                if (remaining <= sideLength || side == EDGE_SIDE_COUNT)
                {
                    Vector2 normalized = sideLength > 0f ? direction / sideLength : Vector2.right;
                    position = start + normalized * Mathf.Min(remaining, sideLength);
                    angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
                    break;
                }

                remaining -= sideLength;
                start = end;
            }

            segmentRect.anchoredPosition = position;
            segmentRect.localRotation = Quaternion.Euler(0f, 0f, angle);
            segmentRect.sizeDelta = new Vector2(length, thickness);
        }

        /// <summary>
        ///     指定位置が属する判定ゾーンを求めます。判定側と同じく、Justの範囲を先に見ます。
        /// </summary>
        /// <param name="barProgress"> 小節単位の位置です。 </param>
        /// <returns> 判定ゾーンの番号です。求められない場合は-1。 </returns>
        private int ResolveZoneIndex(float barProgress)
        {
            for (int i = 0; i < _zones.Length; i++)
            {
                if (barProgress >= _zones[i].JustStartNormalized && barProgress < _zones[i].JustEndNormalized)
                {
                    return i;
                }
            }

            // 通常の判定は1小節の末尾へ寄せ、最後の色をタイムアウトまで延ばす。
            float clamped = Mathf.Clamp01(barProgress);
            for (int i = 0; i < _zones.Length; i++)
            {
                if (clamped >= _zones[i].StartNormalized && clamped <= _zones[i].EndNormalized)
                {
                    return i;
                }
            }

            return _zones.Length - 1;
        }

        /// <summary>
        ///     区間の中にJustの範囲の中央があるか判定します。
        /// </summary>
        /// <param name="startProgress"> 区間の始まり（小節単位）です。 </param>
        /// <param name="endProgress"> 区間の終わり（小節単位）です。 </param>
        /// <returns> ある場合はtrue。 </returns>
        private bool ContainsJust(float startProgress, float endProgress)
        {
            for (int i = 0; i < _zones.Length; i++)
            {
                float justCenter = (_zones[i].JustStartNormalized + _zones[i].JustEndNormalized) * 0.5f;
                if (justCenter >= startProgress && justCenter < endProgress)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     1小節のゲージの案で、伸びきったあとに始まりから消えた区切りの数を求めます。
        /// </summary>
        /// <param name="barProgress"> 前回の入力からの経過（小節単位）です。 </param>
        /// <param name="segmentCount"> 区切りの数です。 </param>
        /// <returns> 消えた区切りの数です。 </returns>
        private int CalculateVanishedCount(float barProgress, int segmentCount)
        {
            float vanishLength = _timeoutBarCount - _lengthInBars;
            if (!_isOneBarGauge || vanishLength <= 0f)
            {
                return 0;
            }

            float vanishRatio = Mathf.Clamp01((barProgress - _lengthInBars) / vanishLength);
            return Mathf.FloorToInt(vanishRatio * segmentCount);
        }

        /// <summary>
        ///     全ての区切りの色を、進み具合・硬直・チュートリアルの対象に合わせて更新します。
        /// </summary>
        private void UpdateSegmentColors()
        {
            for (int i = 0; i < _segmentImages.Length; i++)
            {
                if (_segmentImages[i] == null)
                {
                    continue;
                }

                bool isVisible = i >= _vanishedCount;
                _segmentImages[i].enabled = isVisible;
                if (isVisible)
                {
                    _segmentImages[i].color = GetSegmentColor(i, i < _litCount);
                }
            }
        }

        /// <summary>
        ///     区切りの色を求めます。
        /// </summary>
        /// <param name="index"> 区切りの番号です。 </param>
        /// <param name="isLit"> もう進んだ区切りならtrue。 </param>
        /// <returns> 区切りの色です。 </returns>
        private Color GetSegmentColor(int index, bool isLit)
        {
            int zoneIndex = _segmentZoneIndices[index];
            Color color = zoneIndex >= 0 && zoneIndex < _beatColors.Count ? _beatColors[zoneIndex] : Color.white;
            if (_lockedBarProgress > 0f && _segmentStartProgresses[index] < _lockedBarProgress)
            {
                color = _lockedColor;
            }
            else if (_segmentJustFlags[index])
            {
                // Justがある区切りは白へ寄せて、範囲の帯の代わりに区切りそのもので位置を伝える。
                float alpha = color.a;
                color = Color.Lerp(color, Color.white, JUST_WHITE_BLEND);
                color.a = alpha;
            }

            if (_targetBeatCount.HasValue && zoneIndex >= 0 && zoneIndex < _zones.Length
                && _zones[zoneIndex].BeatCount != _targetBeatCount.Value)
            {
                color.a *= DIM_ALPHA_RATE;
            }

            if (!isLit)
            {
                color.a *= FUTURE_ALPHA_RATE;
            }

            return color;
        }

        /// <summary>
        ///     点いた区切りを弾ませます。Justの範囲の中では白から元の色へ戻します。
        /// </summary>
        /// <param name="index"> 区切りの番号です。 </param>
        /// <param name="isJustTiming"> 今がJustの範囲の中ならtrue。 </param>
        private void PlaySegmentPulse(int index, bool isJustTiming)
        {
            _pulseHandle.TryComplete();
            RectTransform segmentRect = _segmentRectTransforms[index];
            Image segmentImage = _segmentImages[index];
            if (segmentRect == null || segmentImage == null)
            {
                return;
            }

            MotionSequenceBuilder sequence = LSequence.Create()
                .Append(LMotion.Punch.Create(Vector3.one, Vector3.one * PULSE_STRENGTH, PULSE_DURATION)
                    .WithFrequency(1)
                    .BindToLocalScale(segmentRect));
            if (isJustTiming)
            {
                sequence.Join(LMotion.Create(Color.white, segmentImage.color, PULSE_DURATION)
                    .BindToColor(segmentImage));
            }

            // ジャストのスロー演出中も同じ長さで見せるため、時間の倍率を無視する。
            _pulseHandle = sequence.Run(motion => motion.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
        }
    }
}
