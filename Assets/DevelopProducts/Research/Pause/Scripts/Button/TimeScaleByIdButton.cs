using UnityEngine;

namespace DevelopProducts.Pause
{
    public class TimeScaleByIdButton : MonoBehaviour
    {
        public void PauseByType()
        {
            if (_timeScaleController == null || _enemy == null) return;
            _timeScaleController.PauseById(_enemy.InstanceId);
        }
        public void ResumeByType()
        {
            if (_timeScaleController == null || _enemy == null) return;
            _timeScaleController.ResumeScaleById(_enemy.InstanceId);
        }
        public void ChangeScaleByType(float scale)
        {
            if (_timeScaleController == null || _enemy == null) return;
            _timeScaleController.ModifyScaleById(_enemy.InstanceId, scale);
        }
        private void Awake()
        {
            _timeScaleController = FindAnyObjectByType<TimeScaleController>(); 
            if (_timeScaleController == null)
            {
                Debug.LogError($"{nameof(TimeScaleByIdButton)}: {nameof(TimeScaleController)} が見つかりません。", this);
            }
            if (_enemy == null)
            {
                Debug.LogError($"{nameof(TimeScaleByIdButton)}: {nameof(_enemy)} が未設定です。", this);
            }
        }
        [SerializeField] private EnemyTest _enemy;
        private TimeScaleController _timeScaleController;
    }
}
