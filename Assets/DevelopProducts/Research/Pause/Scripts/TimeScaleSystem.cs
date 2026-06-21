using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     ITimeScalableオブジェクトのライフサイクル管理を行うクラス
    /// </summary>
    public class TimeScaleSystem : MonoBehaviour
    {
        public static TimeScaleManager TimeScaleManager { get; } = new();

        private static TimeScaleSystem _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            foreach (var obj in TimeScaleManager.GetAll().ToArray())
            {
                obj.Tick();
            }
        }
    }
}
