using UnityEngine;

namespace DevelopProducts.Pause
{
    public class TimeScaleByIdButton : MonoBehaviour
    {
        public void PauseByType()
        {
            _timeScaleController.PauseById(_enemy.InstanceId);
        }
        public void ResumeByType()
        {
            _timeScaleController.ResumeScaleById(_enemy.InstanceId);
        }
        public void ChangeScaleByType(float scale)
        {
            _timeScaleController.ModifyScaleById(_enemy.InstanceId, scale);
        }
        private void Awake()
        {
            _timeScaleController = FindAnyObjectByType<TimeScaleController>();
        }
        [SerializeField] private EnemyTest _enemy;
        private TimeScaleController _timeScaleController;
    }
}
