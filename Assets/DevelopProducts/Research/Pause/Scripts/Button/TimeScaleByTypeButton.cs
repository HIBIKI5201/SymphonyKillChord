using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.Pause
{
    public class TimeScaleByTypeButton : MonoBehaviour
    {
        public void PauseByType()
        {
            if (_timeScaleController == null) return;
            _timeScaleController.PauseByType(_type);
        }
        public void ResumeByType()
        {
            if (_timeScaleController == null) return;
            _timeScaleController.ResumeScaleByType(_type);
        }
        public void ChangeScaleByType(float scale)
        {
            if (_timeScaleController == null) return;
            _timeScaleController.ModifyScaleByType(_type, scale);
        }
        private void Awake()
        {
            _timeScaleController = FindAnyObjectByType<TimeScaleController>();
            if (_timeScaleController == null)
            {
                Debug.LogError($"{nameof(TimeScaleByTypeButton)}: {nameof(TimeScaleController)} が見つかりません。", this);
            }
        }
        [SerializeField] private TimeScaleType _type;
        private TimeScaleController _timeScaleController;
    }
}
