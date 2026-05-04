using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

#if UNITY_EDITOR

namespace KillChord.Runtime.Composition.InGame.Debugger
{
    /// <summary>
    ///     カメラシステムのパラメータをデバッグ表示するためのクラス。
    /// </summary>
    public sealed class CameraSystemParameterDebug : MonoBehaviour
    {
        public void SetCameraParameter(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        [SerializeField] private CameraSystemParameter _parameter;
    }
}

#endif