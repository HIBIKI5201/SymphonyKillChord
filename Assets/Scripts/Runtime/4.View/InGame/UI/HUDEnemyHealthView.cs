using KillChord.Runtime.Utility.Collections;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KillChord.Runtime.View.InGame.UI
{
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class HUDEnemyHealthView : MonoBehaviour
    {
        public event Action OnUpdate;
        public void SetLockonEnable(bool isLockon)
        {
            _handle.TryComplete();
            _healthImage.enabled = isLockon;
        }

        public void SetHealth(float ratio)
        {
            if (float.IsNaN(ratio))
                return;
            int index = RatioToIndex(ratio);
            if (_index != index)
            {
                _index = index;
                _healthImage.sprite = _sprites[_index];

                _handle.TryCancel();
                _handle = LSequence.Create()
                    .Join(LMotion.Punch.Create(0f, 30f, 0.3f)
                        .WithEase(Ease.OutCirc)
                        .WithFrequency(Random.Range(6, 10))
                        .BindToAnchoredPositionY(_healthImage.rectTransform))
                    .Join(LMotion.Punch.Create(0f, 30f, 0.3f)
                        .WithEase(Ease.OutCirc)
                        .WithFrequency(Random.Range(6, 10))
                        .BindToAnchoredPositionX(_healthImage.rectTransform))
                    .Run(x => x.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
            }
        }
        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
        private void Awake()
        {
            _healthImage.sprite = _sprites[^1];
        }
        private void LateUpdate()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
            _handle.TryCancel();
        }
        private int RatioToIndex(float ratio)
        {
            return Mathf.Clamp(Mathf.RoundToInt(ratio * _sprites.Length), 0, _sprites.Length - 1);
        }

        [SerializeField] private Image _healthImage;
        [SerializeField] private Sprite[] _sprites;

        private int _index;
        private MotionHandle _handle;
    }
}
