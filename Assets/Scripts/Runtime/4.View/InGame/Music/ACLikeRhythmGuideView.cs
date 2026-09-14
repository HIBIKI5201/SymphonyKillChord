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
        ///     ガイド上で枠線を表示しているブロックと同じ基準で判定する。
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
        ///     チュートリアル等で対象となっているBeatCountを設定し、対象外ビートの表示を暗くする。
        /// </summary>
        /// <param name="targetBeatCount"> 対象のBeatCount。対象がない場合はnull。 </param>
        public void SetTargetBeatCount(int? targetBeatCount)
        {
            if (_targetBeatCount != targetBeatCount)
            {
                _targetBeatCount = targetBeatCount;
                UpdateBeatColors();
                UpdateJustOutlineColors();
                // 現在ビートの表示色は次のブロック遷移まで更新されないため、ここで即座に反映する。
                UpdateCurrentBeatColor();
            }
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
        ///     ジャスト位置の枠線の色を、現在の対象BeatCountに応じて更新する。
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

                // 枠線もガイドUIの一部のため、ブロックと同じ基準で対象外ビートを減光する。
                _justOutlineImages[i].color =
                    ApplyTargetDim(_effectConfig.JustOutlineColor, _justOutlineZoneIndices[i]);
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
            zoneIndex = GetBeatSectionIndex(blockIndex, _scale, _beatWidth);

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
        public void SetBeatsOffset(float normalizeOffset)
        {
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
        ///     現在カーソルが乗っているビートブロックの色を取得する。
        ///     全画面演出へ渡す色のため、ガイド表示上の減光は適用しない。
        ///     減光はガイド上で対象ビートを強調するための表現であり、演出の明るさまで変えると
        ///     対象外ビートの入力だけ演出が弱くなってしまう。
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
            return TryGetZoneColor(beatIndex, out color, out _);
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

        /// <summary> ゲージ全長が表す小節数。Justは小節内正規化位置(1/BeatCount)をこの値で割った位置になる。 </summary>
        private const float GUIDE_LENGTH_IN_BARS = 1.5f;

        /// <summary> ジャスト位置の枠線1つを構成する線の本数。上下左右の4本。 </summary>
        private const int OUTLINE_LINE_COUNT = 4;

        /// <summary> ジャスト位置の枠線の最小の太さ。 </summary>
        private const float MIN_OUTLINE_THICKNESS = 0.1f;

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
        private Image[] _justOutlineImages = Array.Empty<Image>();
        private int[] _justOutlineZoneIndices = Array.Empty<int>();
        private int[] _justTimingBeatBoxIndex;
        private MotionHandle[] _handles;
        private int _totalBeatBoxCount;
        private int _currentOpenIndex = -1;
        private float[] _zoneStarts = Array.Empty<float>();
        private float[] _zoneEnds = Array.Empty<float>();
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

            CreateJustOutlines();
            _currentOpenIndex = -1;
        }

        /// <summary>
        ///     生成済みビートオブジェクトを破棄する。
        /// </summary>
        private void ClearGeneratedBeatObjects()
        {
            // 枠線はブロックの子のため、ブロックの破棄に付随して消える。参照だけを捨てる。
            _justOutlineImages = Array.Empty<Image>();
            _justOutlineZoneIndices = Array.Empty<int>();

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
        ///     ジャストタイミング位置のブロックへ枠線を生成する。
        ///     枠線はブロックの子として生成するため、高さのアニメーションへ自動的に追従する。
        /// </summary>
        private void CreateJustOutlines()
        {
            if (_effectConfig == null
                || _justTimingBeatBoxIndex == null
                || _leftBeatRectTransforms == null
                || _rightBeatRectTransforms == null)
            {
                _justOutlineImages = Array.Empty<Image>();
                _justOutlineZoneIndices = Array.Empty<int>();
                return;
            }

            // 判定ゾーンごとに1ブロック、それを左右のガイド分で2倍の枠線を生成する。
            int lineCount = _justTimingBeatBoxIndex.Length * OUTLINE_LINE_COUNT * 2;
            _justOutlineImages = new Image[lineCount];
            _justOutlineZoneIndices = new int[lineCount];

            int writeIndex = 0;
            for (int i = 0; i < _justTimingBeatBoxIndex.Length; i++)
            {
                int blockIndex = _justTimingBeatBoxIndex[i];
                if (blockIndex < 0 || blockIndex >= _leftBeatRectTransforms.Length)
                {
                    continue;
                }

                int zoneIndex = GetBeatSectionIndex(blockIndex, _scale, _beatWidth);
                writeIndex = CreateJustOutline(
                    _leftBeatRectTransforms[blockIndex], $"JustOutline_Left_{i}", zoneIndex, writeIndex);
                writeIndex = CreateJustOutline(
                    _rightBeatRectTransforms[blockIndex], $"JustOutline_Right_{i}", zoneIndex, writeIndex);
            }

            // ブロック番号を解決できず生成を飛ばした分の空きを詰める。
            if (writeIndex < lineCount)
            {
                Array.Resize(ref _justOutlineImages, writeIndex);
                Array.Resize(ref _justOutlineZoneIndices, writeIndex);
            }
        }

        /// <summary>
        ///     1ブロック分の枠線を上下左右の4本で生成する。
        ///     ブロックは左右が隣と密着しているため、左右の線は内側へ描き、上下の線だけ外へ張り出す。
        ///     上と下の張り出し量は個別に設定できる。
        /// </summary>
        /// <param name="parent"> 枠線を付けるブロック。 </param>
        /// <param name="objectName"> 生成するオブジェクト名の接頭辞。 </param>
        /// <param name="zoneIndex"> ブロックが属する判定ゾーンのインデックス。 </param>
        /// <param name="writeIndex"> 生成した枠線を書き込む位置。 </param>
        /// <returns> 次に書き込む位置。 </returns>
        private int CreateJustOutline(RectTransform parent, string objectName, int zoneIndex, int writeIndex)
        {
            float thickness = Mathf.Max(MIN_OUTLINE_THICKNESS, _effectConfig.JustOutlineThickness);
            float upperExtend = Mathf.Max(0f, _effectConfig.JustOutlineUpperExtend);
            float lowerExtend = Mathf.Max(0f, _effectConfig.JustOutlineLowerExtend);
            Color color = ApplyTargetDim(_effectConfig.JustOutlineColor, zoneIndex);

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