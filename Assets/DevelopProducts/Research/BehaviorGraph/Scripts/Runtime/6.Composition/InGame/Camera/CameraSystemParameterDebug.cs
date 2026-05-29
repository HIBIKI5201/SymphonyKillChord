using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Camera;
using UnityEngine;

#if UNITY_EDITOR

namespace DevelopProducts.BehaviorGraph.Runtime.Composition.InGame.Debugger
{
    /// <summary>
    ///     カメラシステムのパラメータをデバッグ表示するためのクラス。
    /// </summary>
    public sealed class CameraSystemParameterDebug : MonoBehaviour
    {
        [SerializeField] private CameraSystemParameter _parameter;

        public void SetCameraParameter(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }
    }
}

#endif