using UnityEngine;

namespace DevelopProducts.Pause
{
    public class EnemyTest : MonoBehaviour, ITimeScalable
    {
        /// <summary>識別ID</summary>
        public int InstanceId => this.GetInstanceID();

        /// <summary>スケールタイプ</summary>
        public TimeScaleType TimeScaleType => TimeScaleType.Enemy;

        public void Inject(TimeScaleData data)
        {
            _timeScaleData = data;
        }

        public void Tick()
        {
            float scaledDelta = _timeScaleData.ApplyScale(Time.deltaTime);
            _angle += scaledDelta * _speed;
            float x = Mathf.Cos(_angle) * _radius;
            float z = Mathf.Sin(_angle) * _radius;
            transform.position = _currentPos + new Vector3(x, 0f, z);
        }
        private void Start()
        {
            TimeScaleSystem.TimeScaleManager.Register(this, defaultScale: 1f);
            _currentPos = this.transform.position;
        }
        private void OnDestroy()
        {
            TimeScaleSystem.TimeScaleManager.Unregister(this);
        }
        private float _angle = 0f;
        private float _radius = 3f;
        private float _speed = 1f;
        private Vector3 _currentPos;
        private TimeScaleData _timeScaleData;
    }
}
