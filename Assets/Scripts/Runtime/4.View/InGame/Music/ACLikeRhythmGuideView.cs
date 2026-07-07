using KillChord.Runtime.View.InGame.Sequence;
using LitMotion;
using LitMotion.Extensions;
using System;
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
        ///    ビートの位置を更新する。
        /// </summary>
        /// <param name="normalizeOffset"> ビートの位置(0～1の値範囲)</param>
        public void SetBeatsOffset(float normalizeOffset)
        {
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

        [Tooltip("ビートの位置を決めるための配列。(_beatRectTransforms.Length + 1の長さにしてください)")]
        [SerializeField] private float[] _beats;

        [Tooltip("ビートの色")]
        [SerializeField] private Color[] _beatColor;

        [Tooltip("ビートの幅")]
        [SerializeField] private float _beatWidth;

        [Tooltip("ビートの位置を決めるためのスケール")]
        [SerializeField] private float _scale;

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

        private void Awake()
        {
            InitBeatRectTransforms();
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
            for (int i = 0; i < _handles.Length; i++)
            {
                _handles[i].TryCancel();
            }
        }

        [ContextMenu("ビートの位置を初期化")]
        private void InitBeatRectTransforms()
        {
            InitBeatGUI(
                _canvasGroup.gameObject,
                _beats,
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
        private void InitBeatGUI(
            in GameObject parent,
            in float[] beats,
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

            if (_beatColor == null || _beatColor.Length < beats.Length - 1)
            {
                Debug.LogError("_beatColor の長さが beats の区間数(beats.Length - 1)より少ないです。", this);
                totalBeatBoxCount = 0;
                return;
            }

            //スペクトラム風ビートのブロック数を計算
            float beatLength = Mathf.Max(beats);
            float guiLength = beatLength * scale;
            int beatBlockCount = (int)(guiLength / beatWidth);
            totalBeatBoxCount = beatBlockCount;

            //Out配列の初期化
            leftBeatImages = new Image[beatBlockCount];
            rightBeatImages = new Image[beatBlockCount];
            leftBeatRT = new RectTransform[beatBlockCount];
            rightBeatRT = new RectTransform[beatBlockCount];
            handles = new MotionHandle[beatBlockCount];
            justTimingBeatBoxIndex = new int[beats.Length - 1];

            //スペクトラム風ビートのブロックを生成
            for (int i = 0; i < beatBlockCount; i++)
            {
                Color color = _beatColor[GetBeatSectionIndex(i, beats, scale, beatWidth)];

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
                float position = (beats[i] + beats[i + 1]) / 2;
                justTimingBeatBoxIndex[i] = (int)(position * scale / beatWidth);
            }
        }
        /// <summary>
        /// ブロックインデックスがどのbeats区間に属するかを返す
        /// </summary>
        /// <param name="blockIndex">ブロックのインデックス</param>
        /// <param name="beats">ビートの位置配列</param>
        /// <param name="scale">ビートのスケール</param>
        /// <param name="beatWidth">1ブロックの幅</param>
        /// <returns>属するbeats区間のインデックス（beats[i]～beats[i+1]のi）</returns>
        private int GetBeatSectionIndex(int blockIndex, float[] beats, float scale, float beatWidth)
        {
            // ブロックインデックスをbeats空間に逆変換
            float position = (blockIndex * beatWidth) / scale;

            for (int i = 0; i < beats.Length - 1; i++)
            {
                if (position >= beats[i] && position < beats[i + 1])
                {
                    return i;
                }
            }

            // 末尾のブロックは最後の区間に属する
            return beats.Length - 2;
        }
    }
}
