using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.Pause
{
    public class TimeScaleByTypeButton : MonoBehaviour
    {
        public void PauseByType()
        {
            _timeScaleController.PauseByType(_type);
        }
        public void ResumeByType()
        {
            _timeScaleController.ResumeScaleByType(_type);
        }
        public void ChangeScaleByType(float scale)
        {
            _timeScaleController.ModifyScaleByType(_type, scale);
        }
        private void Awake()
        {
            _timeScaleController = FindAnyObjectByType<TimeScaleController>();
        }
        [SerializeField] private TimeScaleType _type;
        private TimeScaleController _timeScaleController;
    }
}
