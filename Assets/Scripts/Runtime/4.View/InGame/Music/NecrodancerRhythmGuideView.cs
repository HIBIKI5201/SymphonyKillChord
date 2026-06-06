using KillChord.Runtime.View.InGame.Sequence;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class NecrodancerRhythmGuideView : MonoBehaviour, IGameplayControllable
    {
        public event Action OnUpdate;
        public event Action OnStartGameplay;
        public event Action OnStopGameplay;

        [Tooltip("ビート達の親RectTransform")]
        [SerializeField] private RectTransform _beatsRootRectTransform;

        [Tooltip("落ちてくるビート達のRectTransform")]
        [SerializeField] private RectTransform[] _beatRectTransforms;

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
            _beatsRootRectTransform.anchoredPosition = Vector2.up * (-Mathf.Lerp(_beats[0], _beats[^1], normalizeOffset) * _scale);
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
            Debug.Assert(_canvasGroup != null, "_canvasGroup が null です", this);

            InitBeatRectTransforms();
        }
        void Update()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
        }


        [ContextMenu("ビートの位置を初期化")]
        private void InitBeatRectTransforms()
        {
            if (_beatRectTransforms.Length != _beats.Length - 1)
            {
                Debug.LogError("RectTransformとBeatsの配列の長さが一致していません。\n_beatRectTransforms.Length = _beats.Length - 1になるように設定してください。");
                return;
            }

            float startY;
            float endY;
            for (int i = 0; i < _beatRectTransforms.Length; i++)
            {
                if (_beatRectTransforms[i] == null)
                {
                    Debug.LogError($"_beatRectTransforms[{i}]がnullです。正しいRectTransformを設定してください。");
                    continue;
                }

                startY = _beats[i] * _scale;
                endY = _beats[i + 1] * _scale;
                _beatRectTransforms[i].sizeDelta = new Vector2(_beatRectTransforms[i].sizeDelta.x, Mathf.Abs(endY - startY));
                _beatRectTransforms[i].anchoredPosition = Vector2.up * ((endY + startY) / 2);
            }
        }
    }
}
