using KillChord.Runtime.View.InGame.Sequence;
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
        }

        [Header("左側のビートのガイド")]

        [Tooltip("落ちてくるビート達のRectTransform")]
        [SerializeField] private RectTransform[] _leftBeatRectTransforms;

        [Tooltip("落ちてくるビート達のImage(Alpha用)")]
        [SerializeField] private Image[] _leftBeatImages;


        [Header("右側のビートのガイド")]

        [Tooltip("落ちてくるビート達のRectTransform")]
        [SerializeField] private RectTransform[] _rightBeatRectTransforms;

        [Tooltip("落ちてくるビート達のImage(Alpha用)")]
        [SerializeField] private Image[] _rightBeatImages;

        [Space]

        [Tooltip("ビートの位置を決めるための配列。(_beatRectTransforms.Length + 1の長さにしてください)")]
        [SerializeField] private float[] _beats;

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
        }
        [ContextMenu("ビートの位置を初期化")]
        private void InitBeatRectTransforms()
        {
            if (_beatPositionImages.Length != _beatPositionRectTransfroms.Length)
            {
                Debug.LogError("ビート位置を表示するImageとRectTransformの配列の長さが一致していません。\n_beatPositionImages.Length = _beatPositionRectTransfroms.Lengthになるように設定してください。");
                return;
            }

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

            for (int i = 0; i < _beatPositionRectTransfroms.Length; i++)
            {
                Vector2 size = _beatPositionRectTransfroms[i].sizeDelta;
                size.x = _beats[^1] * _scale;
                _beatPositionRectTransfroms[i].sizeDelta = size;
            }
        }
    }
}
