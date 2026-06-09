using KillChord.Runtime.View.InGame.Sequence;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class BPMRhythmGuideView : MonoBehaviour, IGameplayControllable
    {
        public event Action OnUpdate;
        public event Action OnStartGameplay;
        public event Action OnStopGameplay;

        [Header("左側のビートのガイド")]

        [Tooltip("ビート達の親RectTransform")]
        [SerializeField] private RectTransform _leftBeatsRootRectTransform;

        [Tooltip("落ちてくるビート達のRectTransform")]
        [SerializeField] private RectTransform[] _leftBeatRectTransforms;

        [Tooltip("落ちてくるビート達のImage(Alpha用)")]
        [SerializeField] private Image[] _leftBeatImages;


        [Header("右側のビートのガイド")]
        [Tooltip("ビート達の親RectTransform")]
        [SerializeField] private RectTransform _rightBeatsRootRectTransform;

        [Tooltip("落ちてくるビート達のRectTransform")]
        [SerializeField] private RectTransform[] _rightBeatRectTransforms;

        [Tooltip("落ちてくるビート達のImage(Alpha用)")]
        [SerializeField] private Image[] _rightBeatImages;


        [Tooltip("ビートの位置を決めるための配列。(_beatRectTransforms.Length + 1の長さにしてください)")]
        [SerializeField] private float[] _beats;

        [Tooltip("ビートの位置を決めるためのスケール")]
        [SerializeField] private float _scale;

        [Space]

        [Tooltip("ビートのAlphaを決めるためのCanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Tooltip("ターゲット時の透明度。")]
        [Range(0f, 1f)]
        [SerializeField] private float _targetAlpha;
        [Tooltip("非ターゲット時の透明度。")]
        [Range(0f, 1f)]
        [SerializeField] private float _noTargetAlpha;

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
            float offset = (-Mathf.Lerp(_beats[0], _beats[^1], normalizeOffset) * _scale);
            _leftBeatsRootRectTransform.anchoredPosition = Vector2.left * offset;
            _rightBeatsRootRectTransform.anchoredPosition = Vector2.right * offset;

            float targetValue = Mathf.Lerp(_beats[0], _beats[^1], normalizeOffset);


            int activeIndex = -1;
            if (_beats != null && _beats.Length >= 2)
            {
                for (int i = 0; i < _beats.Length - 1; i++)
                {
                    float a = _beats[i];
                    float b = _beats[i + 1];

                    if (a <= b)
                    {
                        if (targetValue >= a && targetValue <= b)
                        {
                            activeIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        if (targetValue <= a && targetValue >= b)
                        {
                            activeIndex = i;
                            break;
                        }
                    }
                }
                if (activeIndex == -1)
                {
                    if (targetValue <= Mathf.Min(_beats[0], _beats[^1]))
                        activeIndex = 0;
                    else
                        activeIndex = _beats.Length - 2;
                }
            }

            Image img;
            Color c;
            bool isActive;
            for (int i = 0; i < _leftBeatImages.Length; i++)
            {
                img = _leftBeatImages[i];
                if (img != null)
                {
                    c = img.color;
                    isActive = (i == activeIndex);
                    c.a = isActive ? 1f : 0.5f;
                    img.color = c;
                }
                img = _rightBeatImages[i];
                if (img != null)
                {
                    c = img.color;
                    isActive = (i == activeIndex);
                    c.a = isActive ? 1f : 0.5f;
                    img.color = c;
                }
            }
        }

        public void StartGameplay()
        {
            OnStartGameplay?.Invoke();
        }

        public void StopGameplay()
        {
            OnStopGameplay?.Invoke();
        }

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
            OnStopGameplay = null;
            OnStartGameplay = null;
        }

        [ContextMenu("ビートの位置を初期化")]
        private void InitBeatRectTransforms()
        {
            if (_leftBeatRectTransforms.Length != _beats.Length - 1)
            {
                Debug.LogError("RectTransformとBeatsの配列の長さが一致していません。\n_leftBeatRectTransforms.Length = _beats.Length - 1になるように設定してください。");
                return;
            }
            if (_rightBeatRectTransforms.Length != _beats.Length - 1)
            {
                Debug.LogError("RectTransformとBeatsの配列の長さが一致していません。\n_rightBeatRectTransforms.Length = _beats.Length - 1になるように設定してください。");
                return;
            }

            float startY;
            float endY;
            for (int i = 0; i < _leftBeatRectTransforms.Length; i++)
            {
                if (_leftBeatRectTransforms[i] == null)
                {
                    Debug.LogError($"_leftBeatRectTransforms[{i}]がnullです。正しいRectTransformを設定してください。");
                    continue;
                }
                if (_rightBeatRectTransforms[i] == null)
                {
                    Debug.LogError($"_rightBeatRectTransforms[{i}]がnullです。正しいRectTransformを設定してください。");
                    continue;
                }

                startY = _beats[i] * _scale;
                endY = _beats[i + 1] * _scale;
                _leftBeatRectTransforms[i].sizeDelta = new Vector2(_leftBeatRectTransforms[i].sizeDelta.x, Mathf.Abs(endY - startY));
                _leftBeatRectTransforms[i].anchoredPosition = Vector2.up * ((endY + startY) / 2);

                _rightBeatRectTransforms[i].sizeDelta = new Vector2(_rightBeatRectTransforms[i].sizeDelta.x, Mathf.Abs(endY - startY));
                _rightBeatRectTransforms[i].anchoredPosition = Vector2.up * ((endY + startY) / 2);
            }
        }
    }
}
